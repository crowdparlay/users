using CrowdParlay.Users.Application.Services;

namespace CrowdParlay.Users.Application.Abstractions;

public interface IGoogleAuthenticationService
{
    public Task<GoogleAuthenticationResult> AuthenticateUserByAuthorizationCodeAsync(
        string code, string redirectUri, CancellationToken cancellationToken = default);
}