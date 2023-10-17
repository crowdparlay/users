using System.Security.Claims;
using Dodo.Primitives;
using OpenIddict.Abstractions;

namespace CrowdParlay.Users.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Uuid? GetUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.GetClaim(OpenIddictConstants.Claims.Subject);
        return subject is not null
            ? Uuid.Parse(subject)
            : null;
    }
}