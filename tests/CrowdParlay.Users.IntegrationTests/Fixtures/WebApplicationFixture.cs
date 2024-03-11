using CrowdParlay.Users.Api;
using CrowdParlay.Users.IntegrationTests.Services;
using Grpc.Net.Client;
using MassTransit.Testing;
using Testcontainers.PostgreSql;

namespace CrowdParlay.Users.IntegrationTests.Fixtures;

public class WebApplicationFixture : IAsyncDisposable
{
    public readonly IServiceProvider Services;
    public readonly HttpClient Client;
    public readonly GrpcChannel GrpcChannel;
    public readonly ITestHarness Harness;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithExposedPort(5432)
        .WithPortBinding(5432, true)
        .Build();

    public WebApplicationFixture()
    {
        Task.Run(async () => await _postgres.StartAsync()).Wait();
        var webApplicationFactory = new TestWebApplicationFactory<Program>(_postgres.GetConnectionString());
        Client = webApplicationFactory.CreateClient();
        GrpcChannel = GrpcChannel.ForAddress(Client.BaseAddress!, new GrpcChannelOptions { HttpClient = Client });
        Harness = webApplicationFactory.Services.GetTestHarness();
        Services = webApplicationFactory.Services;
    }

    public async ValueTask DisposeAsync() => await _postgres.DisposeAsync();
}