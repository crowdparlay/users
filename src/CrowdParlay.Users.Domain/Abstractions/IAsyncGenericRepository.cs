using CrowdParlay.Users.Domain.Entities;

namespace CrowdParlay.Users.Domain.Abstractions;

public interface IAsyncGenericRepository<TEntity, in TKey> where TEntity : EntityBase<TKey>
{
    public Task<TEntity?> GetByIdAsync(TKey id);
    public Task<IEnumerable<TEntity>> GetManyAsync(int count, int page = 0);
    public Task AddAsync(TEntity entity);
    public Task UpdateAsync(TEntity entity);
    public Task DeleteAsync(TKey id);
}