namespace SharedKernel;

/// <summary>
/// Marker interface of entities.
/// </summary>
public interface IEntity;

/// <summary>
/// Entity with an identifier.
/// </summary>
/// <typeparam name="TId">Type of the identifier.</typeparam>
public interface IEntity<TId> : IEntity
{
    /// <summary>
    /// Identifier of the entity.
    /// </summary>
    TId Id { get; set; }
}
