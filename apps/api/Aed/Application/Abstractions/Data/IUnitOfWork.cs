using SharedKernel;

namespace Application.Abstractions.Data;

/// <summary>
/// Unit of work that groups repository changes into one transaction.
/// </summary>
/// <typeparam name="TId">Type of the entity identifiers.</typeparam>
public interface IUnitOfWork<TId> : IDisposable
{
    /// <summary>
    /// Returns the repository of an entity type.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    /// <returns>The repository.</returns>
    IRepositoryAsync<T, TId> Repository<T>() where T : class, IEntity<TId>;

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of written rows.</returns>
    Task<long> Commit(CancellationToken cancellationToken);

    /// <summary>
    /// Discards all pending changes.
    /// </summary>
    Task Rollback();
}
