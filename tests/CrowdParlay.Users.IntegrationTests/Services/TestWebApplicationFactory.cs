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

    public TestWebApplicationFactory(string postgresConnectionString) =>
        _postgresConnectionString = postgresConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["POSTGRES_CONNECTION_STRING"] = _postgresConnectionString,
            ["CORS_ORIGINS"] = "http://localhost"
        }));

        builder.ConfigureServices(services =>
        {
            var googleOidcService = services.First(service => service.ServiceType == typeof(IGoogleOidcService));
            services.Remove(googleOidcService);
            services.AddScoped<IGoogleOidcService, TestGoogleOidcService>();

            var massTransitDescriptors = services
                .Where(service => service.ServiceType.Namespace?.Split('.').First() == nameof(MassTransit))
                .ToArray();

            foreach (var descriptor in massTransitDescriptors)
                services.Remove(descriptor);

            services.AddMassTransitTestHarness();
        });
    }
}