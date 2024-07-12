using Dodo.Primitives;
using OpenIddict.Abstractions;

namespace CrowdParlay.Users.Api.Extensions;

public static class HttpContextExtensions
{
    public static Uuid? GetUserId(this HttpContext context)
    {
        var userId = context.User.GetClaim(OpenIddictConstants.Claims.Subject);
        return Uuid.TryParse(userId, out var value) ? value : null;
    }
}