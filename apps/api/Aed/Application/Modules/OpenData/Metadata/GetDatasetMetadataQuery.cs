using Application.Abstractions.Messaging;

namespace Application.Modules.OpenData.Metadata;

/// <summary>
/// Metadata of the open AED data set.
/// </summary>
public sealed record GetDatasetMetadataQuery : IQuery<DatasetMetadata>;
