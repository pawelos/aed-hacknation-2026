namespace SharedKernel;

/// <summary>
/// Base class of entities that can raise domain events.
/// </summary>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Copy of the domain events raised by the entity.
    /// </summary>
    public List<IDomainEvent> DomainEvents => _domainEvents.ToList();

    /// <summary>
    /// Removes all raised domain events.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Adds a domain event that is published after the changes are saved.
    /// </summary>
    /// <param name="domainEvent">Domain event to raise.</param>
    protected void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
