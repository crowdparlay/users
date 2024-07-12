using System.Security.Cryptography.X509Certificates;
using CrowdParlay.Users.Api.Services.OpenIddict;
using CrowdParlay.Users.Infrastructure.Persistence;
using OpenIddict.Server;
using OpenIddict.Validation.AspNetCore;

namespace CrowdParlay.Users.Api.Extensions;

public static class ConfigureOpenIddictExtensions
{
    public static IServiceCollection ConfigureOpenIddict(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var builder = services.AddOpenIddict();

        builder.AddCore(options => options
            .UseEntityFrameworkCore()
            .UseDbContext<OpenIddictDbContext>());

        builder.AddServer(options =>
        {
            options.SetTokenEndpointUris("/connect/token");

            options
                .AllowPasswordFlow()
                .AllowRefreshTokenFlow()
                .AcceptAnonymousClients();

            options.AddEventHandler<OpenIddictServerEvents.HandleTokenRequestContext>(handlerDescriptor =>
                handlerDescriptor.UseScopedHandler<RefreshTokenGrantEventHandler>());

            options.AddEventHandler<OpenIddictServerEvents.HandleTokenRequestContext>(handlerDescriptor =>
                handlerDescriptor.UseScopedHandler<PasswordGrantEventHandler>());

            options
                .SetAccessTokenLifetime(TimeSpan.FromMinutes(30))
                .SetRefreshTokenLifetime(TimeSpan.FromDays(7));

            options.DisableAccessTokenEncryption();

            if (environment.IsDevelopment())
            {
                options
                    .AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();
            }
            else
            {
                var encryptionCertificatePath = configuration["ENCRYPTION_CERTIFICATE_PATH"]!;
                var signingCertificatePath = configuration["SIGNING_CERTIFICATE_PATH"]!;

                var encryptionCertificate = X509Certificate2.CreateFromPemFile(encryptionCertificatePath);
                var signingCertificate = X509Certificate2.CreateFromPemFile(signingCertificatePath);

                options
                    .AddEncryptionCertificate(encryptionCertificate)
                    .AddSigningCertificate(signingCertificate);
            }

            options
                .UseAspNetCore()
                .DisableTransportSecurityRequirement();
        });

        builder.AddValidation(options =>
        {
            options.UseLocalServer();
            options.UseAspNetCore();
            options.Configure(validationOptions =>
            {
                validationOptions.TokenValidationParameters.ValidateIssuer = false;
                validationOptions.TokenValidationParameters.ValidateAudience = false;
                validationOptions.TokenValidationParameters.ValidateLifetime = false;
            });
        });

        services.AddAuthentication(options => options.DefaultScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        return services;
    }
}