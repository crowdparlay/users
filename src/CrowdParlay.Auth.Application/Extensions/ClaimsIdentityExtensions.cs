using System.Collections.Immutable;
using System.Security.Claims;
using CrowdParlay.Auth.Application.Abstractions.Identity;
using OpenIddict.Abstractions;

namespace CrowdParlay.Auth.Application.Extensions;

public static class ClaimsIdentityExtensions
{
    public static async Task InjectClaimsAsync(this ClaimsIdentity identity, IUser user, IUserService users)
    {
        identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());

        if (user.Email is not null)
            identity.AddClaim(OpenIddictConstants.Claims.Email, user.Email);

        if (user.Username is not null)
            identity.AddClaim(OpenIddictConstants.Claims.Name, user.Username);

        var roles = await users.GetRolesAsync(user);
        identity.AddClaims(OpenIddictConstants.Claims.Role, roles.ToImmutableArray());

        foreach (var claim in identity.Claims)
        {
            claim.SetDestinations(claim.Type switch
            {
                OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.Email
                    when identity.HasScope(OpenIddictConstants.Scopes.Profile) => new[]
                    {
                        OpenIddictConstants.Destinations.AccessToken,
                        OpenIddictConstants.Destinations.IdentityToken
                    },
                _ => new[] { OpenIddictConstants.Destinations.AccessToken }
            });
        }
    }
}