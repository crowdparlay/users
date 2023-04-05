using CrowdParlay.Auth.Application.Exceptions;

namespace CrowdParlay.Auth.Application.Abstractions.Identity;

public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates user by credentials.
    /// </summary>
    /// <exception cref="NotFoundException">User with the specified username does not exist.</exception>
    /// <returns>True if the specified credentials are valid, otherwise false.</returns>
    public Task<bool> AuthenticateAsync(IUser user, string password);
    public Task<bool> CanSignInAsync(IUser user);
}