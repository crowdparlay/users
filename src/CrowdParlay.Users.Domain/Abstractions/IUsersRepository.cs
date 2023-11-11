using CrowdParlay.Users.Domain.Entities;
using Dodo.Primitives;

namespace CrowdParlay.Users.Domain.Abstractions;

public interface IUsersRepository : IAsyncGenericRepository<User, Uuid>
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    public Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
}