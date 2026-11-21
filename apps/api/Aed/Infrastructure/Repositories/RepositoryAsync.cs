using Application.Abstractions.Data;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Repositories;

/// <summary>
/// Generic repository based on <see cref="Infrastructure.Database.ApplicationDbContext"/>.
/// </summary>
/// <typeparam name="T">Type of the entity.</typeparam>
/// <typeparam name="TId">Type of the entity identifier.</typeparam>
internal sealed class RepositoryAsync<T, TId>(ApplicationDbContext dbContext)
    : IRepositoryAsync<T, TId>
    where T : class, IEntity<TId>
{
    /// <inheritdoc />
    public IQueryable<T> Entities => dbContext.Set<T>();

    /// <inheritdoc />
    public async Task<T> AddAsync(T entity)
    {
        await dbContext.Set<T>().AddAsync(entity);
        return entity;
    }

    /// <inheritdoc />
    public Task DeleteAsync(T entity)
    {
        dbContext.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<List<T>> GetAllAsync() =>
        dbContext.Set<T>().ToListAsync();

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(TId id) =>
        await dbContext.Set<T>().FindAsync([id]);

    /// <inheritdoc />
    public Task<List<T>> GetPagedResponseAsync(int pageNumber, int pageSize) =>
        dbContext
            .Set<T>()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
}
