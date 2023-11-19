using CrowdParlay.Users.Domain.Entities;

namespace CrowdParlay.Users.Domain.Abstractions;

public interface IAsyncGenericRepository<TEntity, in TKey> where TEntity : EntityBase<TKey>
{
    public Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    public Task<IEnumerable<TEntity>> GetManyAsync(int count, int page = 0, CancellationToken cancellationToken = default);
    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    public Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
}