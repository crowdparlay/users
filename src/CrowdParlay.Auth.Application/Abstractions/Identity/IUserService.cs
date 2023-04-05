namespace CrowdParlay.Auth.Application.Abstractions.Identity;

public interface IUserService
{
    /// <returns>Error descriptions if failed, otherwise null.</returns>
    public Task<IEnumerable<string>?> CreateAsync(string email, string displayName, string password);
    public Task<IUser?> FindByIdAsync(Guid userId);
    public Task<IUser?> FindByEmailAsync(string email);
    public Task<IUser?> FindByUsernameAsync(string username);
    public Task<IEnumerable<string>> GetRolesAsync(IUser user);
}