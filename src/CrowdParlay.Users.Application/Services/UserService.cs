using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CrowdParlay.Users.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager) =>
        _userManager = userManager;

    public async Task<IEnumerable<string>?> CreateAsync(string username, string displayName, string password)
    {
        var user = new User
        {
            UserName = username,
            DisplayName = displayName
        };

        var result = await _userManager.CreateAsync(user, password);
        return result.Succeeded
            ? null
            : result.Errors.Select(x => x.Description);
    }

    public async Task<User?> FindByIdAsync(Guid userId) =>
        await _userManager.FindByIdAsync(userId.ToString());

    public async Task<User?> FindByEmailAsync(string email) =>
        await _userManager.FindByEmailAsync(email);

    public async Task<User?> FindByUsernameAsync(string username) =>
        await _userManager.FindByNameAsync(username);

    public async Task<IEnumerable<string>> GetRolesAsync(User user) =>
        await _userManager.GetRolesAsync(user);
}