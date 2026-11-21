using Application.Abstractions.Messaging;

namespace Application.Modules.PublicAed.Card;

/// <summary>
/// Public card of one AED.
/// </summary>
/// <param name="Id">Identifier of the record.</param>
public sealed record GetAedCardQuery(Guid Id) : IQuery<AedCardResponse>;
