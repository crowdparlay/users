using CrowdParlay.Users.Application.Models;

namespace CrowdParlay.Users.Application.Abstractions;

public interface IGoogleOAuthService
{
    public Task<GoogleUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken = default);

    public Task<string?> GetAccessTokenAsync(
        string code, string redirectUri, IEnumerable<string> scopes, CancellationToken cancellationToken = default);
}