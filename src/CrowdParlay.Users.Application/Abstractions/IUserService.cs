using CrowdParlay.Users.Domain.Entities;

namespace CrowdParlay.Users.Application.Abstractions;

public interface IUserService
{
    public Task<IEnumerable<string>?> CreateAsync(string email, string displayName, string password);
    public Task<User?> FindByIdAsync(Guid userId);
    public Task<User?> FindByEmailAsync(string email);
    public Task<User?> FindByUsernameAsync(string username);
    public Task<IEnumerable<string>> GetRolesAsync(User user);
}