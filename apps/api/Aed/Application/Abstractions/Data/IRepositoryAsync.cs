using SharedKernel;

namespace Application.Abstractions.Data;

/// <summary>
/// Generic repository of entities.
/// </summary>
/// <typeparam name="T">Type of the entity.</typeparam>
/// <typeparam name="TId">Type of the entity identifier.</typeparam>
public interface IRepositoryAsync<T, in TId> where T : class, IEntity<TId>
{
    /// <summary>
    /// Queryable set of entities.
    /// </summary>
    IQueryable<T> Entities { get; }

    /// <summary>
    /// Returns an entity by identifier.
    /// </summary>
    /// <param name="id">Identifier of the entity.</param>
    /// <returns>The entity, or <c>null</c> when not found.</returns>
    Task<T?> GetByIdAsync(TId id);

    /// <summary>
    /// Returns all entities.
    /// </summary>
    /// <returns>The entities.</returns>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// Returns one page of entities.
    /// </summary>
    /// <param name="pageNumber">Page number, starting from 1.</param>
    /// <param name="pageSize">Number of entities per page.</param>
    /// <returns>The entities on the page.</returns>
    Task<List<T>> GetPagedResponseAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Adds an entity.
    /// </summary>
    /// <param name="entity">Entity to add.</param>
    /// <returns>The added entity.</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Removes an entity from the database.
    /// </summary>
    /// <param name="entity">Entity to remove.</param>
    Task DeleteAsync(T entity);
}
