using DbContext = CrowdParlay.Auth.Infrastructure.Persistence.DbContext;

namespace CrowdParlay.Auth.Api.Extensions;

public static class ConfigureOpenIddictExtensions
{
    public static IServiceCollection ConfigureOpenIddict(this IServiceCollection services)
    {
        var builder = services.AddOpenIddict();

        builder.AddCore(options => options
            .UseEntityFrameworkCore()
            .UseDbContext<DbContext>());

        builder.AddServer(options =>
        {
            options
                .SetTokenEndpointUris("/connect/token")
                .SetUserinfoEndpointUris("/connect/userinfo");

            options
                .AllowPasswordFlow()
                .AllowRefreshTokenFlow()
                .AcceptAnonymousClients();

            options
                .SetAccessTokenLifetime(TimeSpan.FromMinutes(30))
                .SetRefreshTokenLifetime(TimeSpan.FromDays(7));

            options
                .DisableAccessTokenEncryption();

            options
                .AddDevelopmentEncryptionCertificate()
                .AddDevelopmentSigningCertificate();

            options
                .UseAspNetCore()
                .DisableTransportSecurityRequirement()
                .EnableTokenEndpointPassthrough();
        });

        builder.AddValidation(options =>
        {
            options.UseLocalServer();
            options.UseAspNetCore();
        });

        return services;
    }
}