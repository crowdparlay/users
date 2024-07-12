using CrowdParlay.Users.Api;
using CrowdParlay.Users.IntegrationTests.Services;
using Testcontainers.PostgreSql;

namespace CrowdParlay.Users.IntegrationTests.Fixtures;

public class WebApplicationFixture : IAsyncDisposable
{
    public readonly IServiceProvider Services;
    public readonly TestWebApplicationFactory<Program> WebApplicationFactory;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithExposedPort(5432)
        .WithPortBinding(5432, true)
        .Build();

    public WebApplicationFixture()
    {
        Task.Run(async () => await _postgres.StartAsync()).Wait();
        WebApplicationFactory = new TestWebApplicationFactory<Program>(_postgres.GetConnectionString());
        Services = WebApplicationFactory.Services;
    }

    public async ValueTask DisposeAsync() => await _postgres.DisposeAsync();
}