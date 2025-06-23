using CrowdParlay.Users.Application.Services;

namespace CrowdParlay.Users.Application.Abstractions;

public interface IGoogleAuthenticationService
{
    public Task<GoogleAuthenticationResult> AuthenticateUserByAuthorizationCodeAsync(string code, CancellationToken cancellationToken = default);
    public string GetAuthorizationFlowUrl(string returnUrl);
}