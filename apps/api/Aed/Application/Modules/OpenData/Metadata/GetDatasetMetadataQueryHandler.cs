using Application.Abstractions.Messaging;
using SharedKernel;

namespace Application.Modules.OpenData.Metadata;

/// <summary>
/// Handler for <see cref="GetDatasetMetadataQuery"/>.
/// </summary>
internal sealed class GetDatasetMetadataQueryHandler(DatasetMetadataBuilder builder)
    : IQueryHandler<GetDatasetMetadataQuery, DatasetMetadata>
{
    /// <summary>
    /// Returns the metadata.
    /// </summary>
    public async ValueTask<Result<DatasetMetadata>> Handle(GetDatasetMetadataQuery query, CancellationToken cancellationToken) =>
        await builder.BuildAsync(cancellationToken);
}
