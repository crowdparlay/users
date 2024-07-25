using System.Security.Claims;
using Dodo.Primitives;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace CrowdParlay.Users.Api.Extensions;

public static class HttpContextExtensions
{
    public static Uuid? GetUserId(this HttpContext context)
    {
        var userId =
            context.User.GetClaim(CookieAuthenticationConstants.UserIdClaim)
            ?? context.User.GetClaim(OpenIddictConstants.Claims.Subject);

        return Uuid.TryParse(userId, out var value) ? value : null;
    }

    public static async Task SignInAsync(this HttpContext context, string authenticationScheme, Uuid userId)
    {
        var userIdClaimType = authenticationScheme switch
        {
            CookieAuthenticationDefaults.AuthenticationScheme => CookieAuthenticationConstants.UserIdClaim,
            OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme => OpenIddictConstants.Claims.Subject,
            _ => throw new NotSupportedException($"Cannot sign in with '{authenticationScheme}': authentication scheme is not supported.")
        };

        var identity = new ClaimsIdentity(authenticationScheme);
        identity.AddClaim(userIdClaimType, userId.ToString());
        await context.SignInAsync(authenticationScheme, new ClaimsPrincipal(identity));
    }
}