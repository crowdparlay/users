using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace CrowdParlay.Users.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthenticationService(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<bool> AuthenticateAsync(User user, string password) =>
        await CanSignInAsync(user) && await _userManager.CheckPasswordAsync(user, password);

    public async Task<bool> CanSignInAsync(User user) =>
        await _signInManager.CanSignInAsync(user);
}