using Application.Abstractions.Messaging;
using Application.Configurations;
using Application.Modules.OpenData.Metadata;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace Application.Modules.OpenData.Dcat;

/// <summary>
/// Handler for <see cref="GetDcatCatalogQuery"/>.
/// </summary>
internal sealed class GetDcatCatalogQueryHandler(DatasetMetadataBuilder builder, IOptions<OpenDataOptions> options)
    : IQueryHandler<GetDcatCatalogQuery, OpenDataFile>
{
    /// <summary>
    /// File name of the catalog.
    /// </summary>
    public const string FileName = "catalog.rdf";

    /// <summary>
    /// Builds the catalog.
    /// </summary>
    public async ValueTask<Result<OpenDataFile>> Handle(GetDcatCatalogQuery query, CancellationToken cancellationToken)
    {
        DatasetMetadata metadata = await builder.BuildAsync(cancellationToken);
        string catalogUrl = options.Value.PublicBaseUrl.TrimEnd('/') + OpenDataConstant.PublicPath + FileName;
        return new OpenDataFile(
            DcatCatalogWriter.Write(metadata, catalogUrl),
            "application/rdf+xml; charset=utf-8",
            FileName,
            metadata.Modified);
    }
}
