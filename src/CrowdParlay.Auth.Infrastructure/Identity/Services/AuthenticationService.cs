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

    public async Task<bool> AuthenticateAsync(IUser user, string password) =>
        await CanSignInAsync(user) && await _userManager.CheckPasswordAsync((User)user, password);

    public async Task<bool> CanSignInAsync(IUser user) =>
        await _signInManager.CanSignInAsync((User)user);
}