using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Base;

/// <summary>
/// Base API controller that resolves Mediator <see cref="ISender"/> from the request scope.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private ISender? _sender;

    /// <summary>
    /// Gets the Mediator sender for the current HTTP request.
    /// </summary>
    protected ISender Sender => _sender ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}
