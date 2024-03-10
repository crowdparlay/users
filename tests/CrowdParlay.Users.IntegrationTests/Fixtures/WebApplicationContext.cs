using CrowdParlay.Users.Api;
using CrowdParlay.Users.IntegrationTests.Services;
using Grpc.Net.Client;
using MassTransit.Testing;
using Nito.AsyncEx;
using Testcontainers.PostgreSql;

// ReSharper disable ClassNeverInstantiated.Global

namespace CrowdParlay.Users.IntegrationTests.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global
public class WebApplicationContext
{
    public readonly IServiceProvider Services;
    public readonly HttpClient Client;
    public readonly GrpcChannel GrpcChannel;
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
        GrpcChannel = GrpcChannel.ForAddress(Client.BaseAddress!, new GrpcChannelOptions { HttpClient = Client });
        Harness = webApplicationFactory.Services.GetTestHarness();
        Services = webApplicationFactory.Services;
    }
}