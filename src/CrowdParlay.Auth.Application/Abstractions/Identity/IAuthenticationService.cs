namespace CrowdParlay.Auth.Application.Abstractions.Identity;

public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates user by credentials.
    /// </summary>
    /// <returns>User ID if the specified credentials are valid, otherwise null.</returns>
    public Task<IUser?> AuthenticateAsync(string username, string password);
    public Task<bool> CanSignInAsync(IUser user);
}