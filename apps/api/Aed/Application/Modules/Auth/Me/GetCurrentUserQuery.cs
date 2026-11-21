using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;

namespace Application.Modules.Auth.Me;

/// <summary>
/// Returns the account of the signed-in user.
/// </summary>
public sealed record GetCurrentUserQuery : IQuery<UserAccount>;
