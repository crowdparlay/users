using System.Security.Claims;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.Domain.Entities;
using OpenIddict.Abstractions;

namespace CrowdParlay.Users.Api.Extensions;

public static class ClaimsIdentityExtensions
{
    public static ClaimsIdentity AddUserClaims(this ClaimsIdentity identity, User user)
    {
        return identity
            .AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString())
            .AddClaim(OpenIddictConstants.Claims.Name, user.Username)
            .AddClaim(OpenIddictConstants.Claims.Email, user.Email);
    }

    public static ClaimsIdentity AddUserClaims(this ClaimsIdentity identity, Register.Response user)
    {
        return identity
            .AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString())
            .AddClaim(OpenIddictConstants.Claims.Name, user.Username)
            .AddClaim(OpenIddictConstants.Claims.Email, user.Email);
    }
}