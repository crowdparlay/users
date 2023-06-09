using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Domain.Entities;

namespace CrowdParlay.Users.Infrastructure.Persistence.Services;

public class AuthenticationService : IAuthenticationService
{
    public async Task<bool> AuthenticateAsync(User user, string password) =>
        throw new NotImplementedException(); //await CanSignInAsync(user) && await _userManager.CheckPasswordAsync(user, password);

    public async Task<bool> CanSignInAsync(User user) =>
        throw new NotImplementedException(); //await _signInManager.CanSignInAsync(user);
}