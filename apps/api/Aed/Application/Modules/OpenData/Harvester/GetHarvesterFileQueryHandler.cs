using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Configurations;
using Application.Modules.OpenData.Metadata;
using Domain.Modules.AedDevice.Model;
using Domain.Modules.DataSource.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Application.Modules.OpenData.Harvester;

/// <summary>
/// Handler for <see cref="GetHarvesterFileQuery"/>. The provider of a wykaz is the owner of the data source: it
/// registers the address of its XML file in the portal and takes responsibility for the data. The whole data set
/// is the aggregate of all wykazy.
/// </summary>
internal sealed class GetHarvesterFileQueryHandler(
    IApplicationDbContext dbContext,
    DatasetMetadataBuilder builder,
    IOptions<OpenDataOptions> options)
    : IQueryHandler<GetHarvesterFileQuery, OpenDataFile>
{
    public async ValueTask<Result<OpenDataFile>> Handle(GetHarvesterFileQuery query, CancellationToken cancellationToken)
    {
        Result<HarvesterDataset> dataset = query.DataSourceId is null
            ? await WholeDataSetAsync(cancellationToken)
            : await ProviderDataSetAsync(query.DataSourceId.Value, cancellationToken);
        if (dataset.IsFailure)
        {
            return Result.Failure<OpenDataFile>(dataset.Error);
        }

        byte[] xml = HarvesterXmlWriter.Write([dataset.Value]);
        string name = OpenDataConstant.HarvesterFileBase(query.DataSourceId);
        return query.Checksum
            ? new OpenDataFile(HarvesterXmlWriter.Md5(xml), "text/plain; charset=utf-8", name + ".md5", dataset.Value.LastUpdateUtc)
            : new OpenDataFile(xml, "application/xml; charset=utf-8", name + ".xml", dataset.Value.LastUpdateUtc);
    }

    private async Task<Result<HarvesterDataset>> WholeDataSetAsync(CancellationToken cancellationToken)
    {
        DatasetMetadata metadata = await builder.BuildAsync(cancellationToken);
        string downloads = Downloads();
        return new HarvesterDataset
        {
            ExtIdent = DatasetMetadataBuilder.DatasetIdentifier,
            Title = metadata.Title,
            Description = metadata.Description,
            Url = metadata.LandingPage,
            RequiresAttribution = metadata.License == AedCodes.License(DataLicense.CcBy4).Code,
            Resources = Resources("aed-radar", downloads, string.Empty, "zbiór zintegrowany", "integrated data set"),
            TagsPl = metadata.KeywordsPl,
            TagsEn = metadata.KeywordsEn,
            // Nationwide: the 16 voivodeships.
            Regions = Enum.GetValues<Voivodeship>().Select(Teryt).ToList(),
            LastUpdateUtc = metadata.Modified ?? Midnight(options.Value.Issued)
        };
    }

    private async Task<Result<HarvesterDataset>> ProviderDataSetAsync(Guid dataSourceId, CancellationToken cancellationToken)
    {
        var source = await (
                from s in dbContext.DataSources.AsNoTracking()
                join o in dbContext.Owners on s.OwnerId equals o.Id
                where s.Id == dataSourceId && !s.IsDeleted
                select new { s.Name, s.License, Owner = o.Name })
            .FirstOrDefaultAsync(cancellationToken);
        if (source is null)
        {
            return Result.Failure<HarvesterDataset>(Error.NotFound(
                "OpenData.DataSourceNotFound", $"Data source '{dataSourceId}' was not found."));
        }

        var allDevices = dbContext.AedDevices.AsNoTracking().Where(d => d.DataSourceId == dataSourceId);
        // Deleted records count too: a deletion sets UpdatedAt and changes the data, so the data date moves on.
        DateTime? modified = await allDevices.MaxAsync(d => (DateTime?)(d.UpdatedAt ?? d.CreatedAt), cancellationToken);
        List<Voivodeship> voivodeships = await allDevices.Where(d => !d.IsDeleted)
            .Select(d => d.Voivodeship).Distinct().ToListAsync(cancellationToken);

        string id = dataSourceId.ToString("N");
        return new HarvesterDataset
        {
            // "aed-" + 32 hex digits = 36 characters, the limit of the schema.
            ExtIdent = $"aed-{id}",
            Title = new LocalizedText(
                $"Wykaz defibrylatorów AED – {source.Owner}",
                $"AED defibrillators – {source.Owner}"),
            Description = new LocalizedText(
                $"Wykaz defibrylatorów AED dostawcy {source.Owner} (źródło: {source.Name}) zgodny ze wzorcowym zakresem "
                + $"danych AED w wersji {AedDataSchema.Version}. Dostawca odpowiada za kompletność, aktualność i poprawność "
                + $"danych. Puste pole oznacza znak umowny X.",
                $"AED defibrillators of {source.Owner} (source: {source.Name}) following the AED reference data scope "
                + $"version {AedDataSchema.Version}. The provider is responsible for the completeness, timeliness and "
                + "correctness of the data."),
            Url = options.Value.PublicBaseUrl.TrimEnd('/') + OpenDataConstant.LandingPath,
            RequiresAttribution = source.License == DataLicense.CcBy4,
            Resources = Resources(id, Downloads(), $"?zrodlo={dataSourceId}", source.Owner, source.Owner),
            TagsPl = ["AED", "defibrylator", "pierwsza pomoc", "ratownictwo", "zdrowie"],
            TagsEn = ["AED", "defibrillator", "first aid"],
            Regions = voivodeships.Order().Select(Teryt).ToList(),
            LastUpdateUtc = modified ?? Midnight(options.Value.Issued)
        };
    }

    /// <summary>
    /// The CSV (downloaded by the portal and indexed as a table), GeoJSON and JSON resources.
    /// </summary>
    private static List<HarvesterResource> Resources(string idPrefix, string downloads, string filter, string namePl, string nameEn) =>
    [
        new($"{idPrefix}-csv", downloads + OpenDataConstant.FileName(OpenDataFormat.Csv) + filter,
            new LocalizedText($"Defibrylatory AED – CSV ({namePl})", $"AED defibrillators – CSV ({nameEn})"),
            new LocalizedText(
                "Plik CSV (UTF-8, przecinek, nagłówek z nazwami pól) zgodny ze wzorcowym zakresem danych AED; jeden wiersz to jedno urządzenie, współrzędne WGS 84.",
                "CSV file (UTF-8, comma, header with field names) following the AED reference data scope; one row per device, WGS 84 coordinates."),
            Local: true),
        new($"{idPrefix}-geo", downloads + OpenDataConstant.FileName(OpenDataFormat.GeoJson) + filter,
            new LocalizedText($"Defibrylatory AED – GeoJSON ({namePl})", $"AED defibrillators – GeoJSON ({nameEn})"),
            new LocalizedText("Punkty AED jako GeoJSON (RFC 7946, WGS 84) dla map i systemów GIS.", "AED points as GeoJSON (RFC 7946, WGS 84) for maps and GIS."),
            Local: false),
        new($"{idPrefix}-jsn", downloads + OpenDataConstant.FileName(OpenDataFormat.Json) + filter,
            new LocalizedText($"Defibrylatory AED – JSON ({namePl})", $"AED defibrillators – JSON ({nameEn})"),
            new LocalizedText("Te same rekordy w JSON z wersją schematu.", "The same records as JSON with the schema version."),
            Local: false)
    ];

    private string Downloads() => options.Value.PublicBaseUrl.TrimEnd('/') + OpenDataConstant.PublicPath;

    private static string Teryt(Voivodeship voivodeship) => ((int)voivodeship).ToString("00", System.Globalization.CultureInfo.InvariantCulture);

    private static DateTime Midnight(DateOnly date) => date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
}
