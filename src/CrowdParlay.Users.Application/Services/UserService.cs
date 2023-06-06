using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using Dodo.Primitives;

namespace CrowdParlay.Users.Application.Services;

public class UserService : IUserService
{
    public async Task<IEnumerable<string>?> CreateAsync(string username, string displayName, string password)
    {
        var user = new User
        {
            Id = new Uuid(),
            Username = username,
            DisplayName = displayName
        };

        /*var result = await _userManager.CreateAsync(user, password);
        return result.Succeeded
            ? null
            : result.Errors.Select(x => x.Description);*/
        
        throw new NotImplementedException();
    }

    public async Task<User?> FindByIdAsync(Guid userId) =>
        throw new NotImplementedException(); //await _userManager.FindByIdAsync(userId.ToString());

    public async Task<User?> FindByEmailAsync(string email) =>
        throw new NotImplementedException(); //await _userManager.FindByEmailAsync(email);

    public async Task<User?> FindByUsernameAsync(string username) =>
        throw new NotImplementedException(); //await _userManager.FindByNameAsync(username);

    public async Task<IEnumerable<string>> GetRolesAsync(User user) =>
        throw new NotImplementedException(); //await _userManager.GetRolesAsync(user);
}