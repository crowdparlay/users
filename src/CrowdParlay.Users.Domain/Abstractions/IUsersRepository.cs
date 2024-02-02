using CrowdParlay.Users.Domain.Entities;
using Dodo.Primitives;

namespace CrowdParlay.Users.Domain.Abstractions;

public interface IUsersRepository : IAsyncGenericRepository<User, Uuid>
{
    public Task<User?> GetByUsernameExactAsync(string username, CancellationToken cancellationToken = default);
    public Task<User?> GetByUsernameNormalizedAsync(string username, CancellationToken cancellationToken = default);
    public Task<User?> GetByEmailNormalizedAsync(string email, CancellationToken cancellationToken = default);
    public Task<User?> GetByUsernameOrEmailNormalizedAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
}