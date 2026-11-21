using Mediator;
using SharedKernel;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Command that changes state and returns a result.
/// </summary>
public interface ICommand : IRequest<Result>, IBaseCommand;

/// <summary>
/// Command that changes state and returns a result.
/// </summary>
/// <typeparam name="TResponse">Type of the returned data.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

/// <summary>
/// Marker interface of all commands.
/// </summary>
public interface IBaseCommand;
