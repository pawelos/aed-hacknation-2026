using Application.Abstractions.Messaging;

namespace Application.Modules.OpenData.Export;

/// <summary>
/// The open data set as a file, whole or for one voivodeship or source.
/// </summary>
/// <param name="Format">File format.</param>
/// <param name="Voivodeship">Polish name or TERYT code of a voivodeship (<c>dolnośląskie</c>, <c>02</c>); null = all.</param>
/// <param name="DataSourceId">Only the records of this source; null = all.</param>
public sealed record ExportAedQuery(OpenDataFormat Format, string? Voivodeship, Guid? DataSourceId)
    : IQuery<OpenDataFile>;
