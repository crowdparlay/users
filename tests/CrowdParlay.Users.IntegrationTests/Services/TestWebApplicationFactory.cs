using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CrowdParlay.Users.IntegrationTests.Services;

internal class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
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
            var massTransitDescriptors = services
                .Where(x => x.ServiceType.Namespace?.StartsWith(nameof(MassTransit)) == true)
                .ToArray();

            foreach (var descriptor in massTransitDescriptors)
                services.Remove(descriptor);

            services.AddMassTransitTestHarness();
        });
    }
}