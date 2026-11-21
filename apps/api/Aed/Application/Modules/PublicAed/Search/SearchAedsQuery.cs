using Application.Abstractions.Messaging;

namespace Application.Modules.PublicAed.Search;

/// <summary>
/// Text search by city, municipality, address or place name.
/// </summary>
/// <param name="Q">Search text.</param>
public sealed record SearchAedsQuery(string Q) : IQuery<AedSearchResponse>;
