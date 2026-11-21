using Mediator;

namespace SharedKernel;

/// <summary>
/// Domain event published after the changes are saved.
/// </summary>
public interface IDomainEvent : INotification;
