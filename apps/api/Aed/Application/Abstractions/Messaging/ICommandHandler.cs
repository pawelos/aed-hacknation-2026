using Mediator;
using SharedKernel;

namespace Application.Abstractions.Messaging;

/// <summary>
/// Handler of a command.
/// </summary>
/// <typeparam name="TCommand">Type of the command.</typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

/// <summary>
/// Handler of a command.
/// </summary>
/// <typeparam name="TCommand">Type of the command.</typeparam>
/// <typeparam name="TResponse">Type of the returned data.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
