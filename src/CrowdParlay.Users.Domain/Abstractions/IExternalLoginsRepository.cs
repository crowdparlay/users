using CrowdParlay.Users.Domain.Entities;
using Dodo.Primitives;

namespace CrowdParlay.Users.Domain.Abstractions;

public interface IExternalLoginsRepository
{
    public Task<IEnumerable<ExternalLogin>> GetByUserIdAsync(Uuid userId, CancellationToken cancellationToken = default);
    public Task AddAsync(ExternalLogin entity, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Uuid id, CancellationToken cancellationToken = default);
}