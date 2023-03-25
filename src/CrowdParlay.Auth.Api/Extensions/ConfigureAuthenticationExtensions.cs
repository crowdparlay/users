using Microsoft.AspNetCore.Authentication.Cookies;

namespace CrowdParlay.Auth.Api.Extensions;

public static class ConfigureAuthenticationExtensions
{
    public static IServiceCollection ConfigureAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                options.LoginPath = "/account/login");

        return services;
    }
}