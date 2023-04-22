using System.Collections.Immutable;
using System.Security.Claims;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using OpenIddict.Abstractions;

namespace CrowdParlay.Users.Application.Extensions;

public static class ClaimsIdentityExtensions
{
    public static async Task InjectClaimsAsync(this ClaimsIdentity identity, User user, IUserService users)
    {
        identity.AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());

        if (user.Email is not null)
            identity.AddClaim(OpenIddictConstants.Claims.Email, user.Email);

        if (user.UserName is not null)
            identity.AddClaim(OpenIddictConstants.Claims.Name, user.UserName);

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