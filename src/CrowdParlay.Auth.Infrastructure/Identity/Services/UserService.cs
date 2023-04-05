using CrowdParlay.Auth.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CrowdParlay.Auth.Infrastructure.Identity.Services;

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

    public async Task<IUser?> FindByIdAsync(Guid userId) =>
        await _userManager.Users.FirstOrDefaultAsync(user => user.Id == userId);

    public async Task<IUser?> FindByEmailAsync(string email) =>
        await _userManager.FindByEmailAsync(email);

    public async Task<IUser?> FindByUsernameAsync(string username) =>
        await _userManager.FindByNameAsync(username);

    public async Task<IEnumerable<string>> GetRolesAsync(IUser user) =>
        await _userManager.GetRolesAsync((User)user);
}