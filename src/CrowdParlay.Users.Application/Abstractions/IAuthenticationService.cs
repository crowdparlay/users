using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Entities;

namespace CrowdParlay.Users.Application.Abstractions;

public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates user by credentials.
    /// </summary>
    /// <exception cref="NotFoundException">User with the specified username does not exist.</exception>
    /// <returns>True if the specified credentials are valid, otherwise false.</returns>
    public Task<bool> AuthenticateAsync(User user, string password);
    public Task<bool> CanSignInAsync(User user);
}