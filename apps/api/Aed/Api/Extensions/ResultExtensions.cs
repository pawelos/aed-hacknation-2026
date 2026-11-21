using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Api.Extensions;

/// <summary>
/// Maps <see cref="Result"/> outcomes to ASP.NET Core action results.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a non-generic result into an HTTP action result.
    /// </summary>
    /// <param name="result">Operation result.</param>
    /// <param name="onSuccess">Factory invoked when the result is successful.</param>
    /// <returns>Matching HTTP action result.</returns>
    public static IActionResult ToActionResult(this Result? result, Func<IActionResult> onSuccess)
    {
        if (result is null)
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        if (result.IsSuccess)
        {
            return onSuccess();
        }

        return ToErrorResult(result);
    }

    /// <summary>
    /// Converts a generic result into an HTTP action result.
    /// </summary>
    /// <typeparam name="T">Result value type.</typeparam>
    /// <param name="result">Operation result.</param>
    /// <param name="onSuccess">Factory invoked when the result is successful.</param>
    /// <returns>Matching HTTP action result.</returns>
    public static IActionResult ToActionResult<T>(this Result<T>? result, Func<IActionResult> onSuccess)
    {
        if (result is null)
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        if (result.IsSuccess)
        {
            return onSuccess();
        }

        return ToErrorResult(result);
    }

    private static IActionResult ToErrorResult(Result result)
    {
        switch (result.Error.Type)
        {
            case ErrorType.NotFound:
                return new NotFoundObjectResult(result);
            case ErrorType.Conflict:
                return new ObjectResult(result) { StatusCode = StatusCodes.Status409Conflict };
            case ErrorType.Forbidden:
                return new ObjectResult(result) { StatusCode = StatusCodes.Status403Forbidden };
            case ErrorType.Validation:
            case ErrorType.Problem:
                return new BadRequestObjectResult(result);
            default:
                return new ObjectResult(result) { StatusCode = StatusCodes.Status500InternalServerError };
        }
    }
}
