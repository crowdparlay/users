using CrowdParlay.Users.Application.Abstractions;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Users.IntegrationTests.Services;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly string _postgresConnectionString;
    private readonly string _redisConnectionString;

    public TestWebApplicationFactory(string postgresConnectionString, string redisConnectionString)
    {
        _postgresConnectionString = postgresConnectionString;
        _redisConnectionString = redisConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["POSTGRES_CONNECTION_STRING"] = _postgresConnectionString,
            ["CORS_ORIGINS"] = "http://localhost",
            ["GOOGLEOAUTH:CLIENTID"] = "60239123456-is4a4ksd03944fszonic6nsdfhmlwdlp.apps.googleusercontent.com",
            ["GOOGLEOAUTH:CLIENTSECRET"] = "DLDK60jdAxnZAfdfs9df2F-X",
            ["GOOGLEOAUTH:AUTHORIZATIONFLOWREDIRECTURI"] = "http://localhost/api/v1/authentication/sign-in-google-callback",
            ["DATA_PROTECTION_REDIS_CONNECTION_STRING"] = _redisConnectionString,
            ["SIGN_UP_PAGE_URI"] = "http://localhost/sign-up"
        }));

        builder.ConfigureServices(services =>
        {
            var googleOidcService = services.First(service => service.ServiceType == typeof(IGoogleOAuthService));
            services.Remove(googleOidcService);
            services.AddScoped<IGoogleOAuthService, TestGoogleOAuthService>();

            var massTransitDescriptors = services
                .Where(service => service.ServiceType.Namespace?.Split('.').First() == nameof(MassTransit))
                .ToArray();

            foreach (var descriptor in massTransitDescriptors)
                services.Remove(descriptor);

            services.AddMassTransitTestHarness();
        });
    }
}