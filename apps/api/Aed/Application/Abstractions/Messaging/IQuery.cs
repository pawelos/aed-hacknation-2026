using Mediator;
using SharedKernel;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Query that reads data and returns a result.
/// </summary>
/// <typeparam name="TResponse">Type of the returned data.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
