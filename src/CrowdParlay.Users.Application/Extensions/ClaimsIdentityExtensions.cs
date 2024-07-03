using System.Security.Claims;
using CrowdParlay.Users.Domain.Entities;
using OpenIddict.Abstractions;

namespace CrowdParlay.Users.Application.Extensions;

public static class ClaimsIdentityExtensions
{
    public static ClaimsIdentity AddUserClaims(this ClaimsIdentity identity, User user) => identity
        .AddClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString())
        .AddClaim(OpenIddictConstants.Claims.Name, user.Username)
        .AddClaim(OpenIddictConstants.Claims.Email, user.Email);
}