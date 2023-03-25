using System.Security.Claims;
using CrowdParlay.Auth.Application.Abstractions;

namespace CrowdParlay.Auth.Api.Services;

/// <summary>
/// Provides brief information containing unique identifier and authentication data related to the request issuer.
/// </summary>
public class CurrentUserProvider : ICurrentUserProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserProvider(IHttpContextAccessor httpContextAccessor) =>
        _httpContextAccessor = httpContextAccessor;

    public int? AccountId
    {
        get
        {
            var claimValue = _httpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            var success = int.TryParse(claimValue, out var accountId);
            return success ? accountId : null;
        }
    }

    public bool IsAuthenticated => AccountId is not null;
}