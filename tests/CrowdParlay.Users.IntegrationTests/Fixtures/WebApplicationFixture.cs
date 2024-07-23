using CrowdParlay.Users.Api;
using CrowdParlay.Users.IntegrationTests.Services;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace CrowdParlay.Users.IntegrationTests.Fixtures;

public class WebApplicationFixture : IAsyncDisposable
{
    public readonly IServiceProvider Services;
    public readonly TestWebApplicationFactory<Program> WebApplicationFactory;

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithExposedPort(5432)
        .WithPortBinding(5432, true)
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder().Build();

    public WebApplicationFixture()
    {
        Task.WaitAll(_redis.StartAsync(), _postgres.StartAsync());
        WebApplicationFactory = new TestWebApplicationFactory<Program>(_postgres.GetConnectionString(), _redis.GetConnectionString());
        Services = WebApplicationFactory.Services;
    }

    public async ValueTask DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }
}