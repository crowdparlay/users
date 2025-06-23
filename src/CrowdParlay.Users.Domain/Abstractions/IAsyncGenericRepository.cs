using CrowdParlay.Users.Domain.Entities;

namespace CrowdParlay.Users.Domain.Abstractions;

public interface IAsyncGenericRepository<TEntity, in TKey> where TEntity : EntityBase<TKey> where TKey : notnull
{
    public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    public Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}