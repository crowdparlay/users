using CrowdParlay.Users.Domain.Entities;

namespace CrowdParlay.Users.Domain.Abstractions;

public interface IExternalLoginsRepository
{
    public Task<IEnumerable<ExternalLogin>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task AddAsync(ExternalLogin entity, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}