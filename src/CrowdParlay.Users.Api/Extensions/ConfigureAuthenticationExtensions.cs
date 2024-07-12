using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using OpenIddict.Validation.AspNetCore;

namespace CrowdParlay.Users.Api.Extensions;

public static class ConfigureAuthenticationExtensions
{
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
    {
        var builder = services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);

        builder.AddCookie(options =>
        {
            options.Cookie.Name = ".CrowdParlay.Authentication";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
            options.SlidingExpiration = true;
            options.Events.OnRedirectToLogin = _ => Task.CompletedTask;
        });

        services.AddAuthorization(options =>
        {
            var authorizationPolicy = new AuthorizationPolicyBuilder(
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

            authorizationPolicy.RequireAuthenticatedUser();
            authorizationPolicy.RequireAssertion(context => context.User.Identities.Count() == 1);
            options.DefaultPolicy = authorizationPolicy.Build();
        });

        return services;
    }
}