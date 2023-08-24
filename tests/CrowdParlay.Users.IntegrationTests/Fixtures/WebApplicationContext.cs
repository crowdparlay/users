using CrowdParlay.Users.Api;
using CrowdParlay.Users.IntegrationTests.Services;
using MassTransit.Testing;
using Nito.AsyncEx;
using Testcontainers.PostgreSql;

namespace CrowdParlay.Users.IntegrationTests.Fixtures;

public class WebApplicationContext
{
    public readonly HttpClient Client;
    public readonly ITestHarness Harness;

    public WebApplicationContext()
    {
        var postgres = new PostgreSqlBuilder()
            .WithExposedPort(5432)
            .WithPortBinding(5432, true)
            .Build();

        AsyncContext.Run(async () => await postgres.StartAsync());

        var webApplicationFactory = new TestWebApplicationFactory<Program>(postgres.GetConnectionString());
        Client = webApplicationFactory.CreateClient();
        Harness = webApplicationFactory.Services.GetTestHarness();
    }
}