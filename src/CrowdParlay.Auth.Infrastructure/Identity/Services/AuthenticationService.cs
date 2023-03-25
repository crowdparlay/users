using CrowdParlay.Auth.Application.Abstractions.Identity;
using CrowdParlay.Auth.Application.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace CrowdParlay.Auth.Infrastructure.Identity.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthenticationService(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IUser?> AuthenticateAsync(string username, string password)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user is null)
            return null;

        if (!await CanSignInAsync(user))
            throw new ForbiddenException();

        return await _userManager.CheckPasswordAsync(user, password)
            ? user
            : null;
    }

    public async Task<bool> CanSignInAsync(IUser user) =>
        await _signInManager.CanSignInAsync((User)user);
}