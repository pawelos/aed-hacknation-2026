using Application.Abstractions.Data;
using Application.Configurations;
using Domain.Modules.DataSource.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Application.Modules.OpenData.Metadata;

/// <summary>
/// Builds <see cref="DatasetMetadata"/> from the database and <see cref="OpenDataOptions"/>; used by the
/// metadata endpoint and the DCAT-AP-PL catalog, so both describe the data set the same way.
/// </summary>
internal sealed class DatasetMetadataBuilder(IApplicationDbContext dbContext, IOptions<OpenDataOptions> options)
{
    /// <summary>
    /// Identifier of the data set in the catalog.
    /// </summary>
    public const string DatasetIdentifier = "aed-radar-defibrylatory-aed";

    private static readonly LocalizedText Title = new(
        "Defibrylatory AED w Polsce – zbiór zintegrowany AED Radar",
        "AED defibrillators in Poland – AED Radar integrated data set");

    private static readonly LocalizedText Description = new(
        "Ogólnopolski, otwarty zbiór lokalizacji automatycznych defibrylatorów zewnętrznych (AED) zebranych od wielu "
        + "dysponentów: samorządów, szkół, firm, OSP i organizacji społecznych. Każdy rekord wskazuje źródło danych, "
        + "jego licencję i zakres referencyjny oraz datę ostatniej weryfikacji. Zbiór zawiera współrzędne WGS 84, "
        + "adres z kodem TERYT, dostępność i godziny w formacie czytelnym maszynowo oraz sposób dostępu do urządzenia. "
        + "Pliki są generowane na bieżąco z bazy, więc adresy zasobów pozostają stałe. Dane w prototypie są przykładowe.",
        "Nationwide open data set of automated external defibrillator (AED) locations collected from many holders: "
        + "local governments, schools, companies, fire brigades and NGOs. Each record names its data source, licence, "
        + "reference scope and last verification date. It contains WGS 84 coordinates, an address with the TERYT code, "
        + "availability and machine-readable opening hours, and how to access the device. The files are generated "
        + "live from the database, so the resource addresses stay the same. The data in the prototype is fictional.");

    private static readonly string[] KeywordsPl =
        ["AED", "defibrylator", "pierwsza pomoc", "nagłe zatrzymanie krążenia", "ratownictwo", "zdrowie", "lokalizacje"];

    private static readonly string[] KeywordsEn =
        ["AED", "defibrillator", "first aid", "cardiac arrest", "emergency", "health", "locations"];

    public async Task<DatasetMetadata> BuildAsync(CancellationToken cancellationToken)
    {
        OpenDataOptions settings = options.Value;
        string baseUrl = settings.PublicBaseUrl.TrimEnd('/');
        string downloads = baseUrl + OpenDataConstant.PublicPath;

        var active = dbContext.AedDevices.AsNoTracking().Where(d => !d.IsDeleted);
        int records = await active.CountAsync(cancellationToken);
        // Deleted records count too: a deletion sets UpdatedAt and changes the data set.
        DateTime? modified = await dbContext.AedDevices.AsNoTracking()
            .MaxAsync(d => (DateTime?)(d.UpdatedAt ?? d.CreatedAt), cancellationToken);

        var sourceRows = await (
                from s in dbContext.DataSources.AsNoTracking()
                join o in dbContext.Owners on s.OwnerId equals o.Id
                where !s.IsDeleted
                orderby s.Name
                select new
                {
                    s.Id,
                    s.Name,
                    Owner = o.Name,
                    s.Url,
                    s.License,
                    s.ReferenceScope,
                    s.LastVerifiedAt,
                    Records = active.Count(d => d.DataSourceId == s.Id)
                })
            .ToListAsync(cancellationToken);

        List<DatasetSource> sources = sourceRows
            .Select(s => new DatasetSource(
                s.Id,
                s.Name,
                s.Owner,
                s.Url,
                AedCodes.License(s.License).Code,
                AedCodes.LicenseUrl(s.License),
                s.ReferenceScope,
                s.Records,
                s.LastVerifiedAt,
                $"{downloads}{OpenDataConstant.FileName(OpenDataFormat.Csv)}?zrodlo={s.Id}",
                $"{downloads}{OpenDataConstant.HarvesterFileBase(s.Id)}.xml"))
            .ToList();

        // The aggregate needs attribution as soon as one source does.
        DataLicense license = sourceRows.Any(s => s.License == DataLicense.CcBy4) ? DataLicense.CcBy4 : DataLicense.Cc0;

        return new DatasetMetadata
        {
            Identifier = DatasetIdentifier,
            Title = Title,
            Description = Description,
            KeywordsPl = KeywordsPl,
            KeywordsEn = KeywordsEn,
            Category = "Zdrowie",
            EuDataTheme = "http://publications.europa.eu/resource/authority/data-theme/HEAL",
            License = AedCodes.License(license).Code,
            LicenseUrl = AedCodes.LicenseUrl(license),
            UpdateFrequency = "na bieżąco (pliki generowane przy każdym pobraniu)",
            EuFrequency = "http://publications.europa.eu/resource/authority/frequency/UPDATE_CONT",
            Publisher = settings.PublisherName,
            ContactEmail = settings.ContactEmail,
            LandingPage = baseUrl + OpenDataConstant.LandingPath,
            SpatialCoverage = "Polska",
            Issued = settings.Issued,
            Modified = modified,
            Records = records,
            SchemaVersion = AedDataSchema.Version,
            Resources = Enum.GetValues<OpenDataFormat>().Select(f => Resource(f, downloads)).ToList(),
            Sources = sources,
            Fields = AedDataSchema.Fields
                .Select(f => new DatasetField(
                    f.Name,
                    AedDataSchema.TypeName(f.Type),
                    f.Required,
                    f.MaxLength,
                    f.Format,
                    new LocalizedText(f.DescriptionPl, f.DescriptionEn),
                    f.Example,
                    f.Dictionary))
                .ToList(),
            Dictionaries = AedDataSchema.Dictionaries,
            TemplateUrl = downloads + OpenDataConstant.TemplateFileName,
            DcatUrl = downloads + "catalog.rdf",
            HarvesterUrl = downloads + OpenDataConstant.HarvesterFileBase(null) + ".xml"
        };
    }

    private static DatasetResource Resource(OpenDataFormat format, string downloads)
    {
        string file = OpenDataConstant.FileName(format);
        switch (format)
        {
            case OpenDataFormat.Csv:
                return new DatasetResource("CSV", OpenDataConstant.CsvMediaType, OpenDataConstant.EuFileType(format), downloads + file,
                    new LocalizedText("Defibrylatory AED – CSV", "AED defibrillators – CSV"),
                    new LocalizedText(
                        "Wszystkie rekordy w CSV (RFC 4180: UTF-8 bez BOM, przecinek, nagłówek z nazwami pól). Filtry: ?wojewodztwo=02, ?zrodlo=<id>.",
                        "All records as CSV (RFC 4180: UTF-8 without BOM, comma, header with field names). Filters: ?wojewodztwo=02, ?zrodlo=<id>."));
            case OpenDataFormat.GeoJson:
                return new DatasetResource("GeoJSON", OpenDataConstant.GeoJsonMediaType, OpenDataConstant.EuFileType(format), downloads + file,
                    new LocalizedText("Defibrylatory AED – GeoJSON", "AED defibrillators – GeoJSON"),
                    new LocalizedText(
                        "Wszystkie rekordy jako GeoJSON (RFC 7946), punkty WGS 84, gotowe do map i systemów GIS.",
                        "All records as GeoJSON (RFC 7946), WGS 84 points, ready for maps and GIS."));
            case OpenDataFormat.Json:
                return new DatasetResource("JSON", OpenDataConstant.JsonMediaType, OpenDataConstant.EuFileType(format), downloads + file,
                    new LocalizedText("Defibrylatory AED – JSON", "AED defibrillators – JSON"),
                    new LocalizedText(
                        "Wszystkie rekordy w JSON z wersją schematu i datą wygenerowania.",
                        "All records as JSON with the schema version and generation time."));
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }
    }
}
