using System.Collections;
using Application.Abstractions.Data;
using Infrastructure.Database;
using SharedKernel;

namespace Infrastructure.Repositories;

/// <summary>
/// Unit of work based on <see cref="ApplicationDbContext"/> (Car Reminder without the cache).
/// </summary>
/// <typeparam name="TId">Type of the entity identifiers.</typeparam>
internal sealed class UnitOfWork<TId>(ApplicationDbContext dbContext) : IUnitOfWork<TId>
{
    private readonly ApplicationDbContext _dbContext =
        dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    private Hashtable? _repositories;
    private bool _disposed;

    /// <inheritdoc />
    public IRepositoryAsync<TEntity, TId> Repository<TEntity>()
        where TEntity : class, IEntity<TId>
    {
        _repositories ??= [];

        string type = typeof(TEntity).Name;

        if (!_repositories.ContainsKey(type))
        {
            object? repositoryInstance = Activator.CreateInstance(
                typeof(RepositoryAsync<,>).MakeGenericType(typeof(TEntity), typeof(TId)),
                _dbContext);

            _repositories.Add(type, repositoryInstance);
        }

        return (IRepositoryAsync<TEntity, TId>)_repositories[type]!;
    }

    /// <inheritdoc />
    public async Task<long> Commit(CancellationToken cancellationToken) =>
        await _dbContext.SaveChangesAsync(cancellationToken);

    /// <inheritdoc />
    public Task Rollback()
    {
        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry in
                 _dbContext.ChangeTracker.Entries().ToList())
        {
            entry.Reload();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _repositories?.Clear();
        _disposed = true;
    }
}
