using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace CrowdParlay.Users.IntegrationTests.Services;

internal class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly IFixture _fixture;

    public TestWebApplicationFactory(IFixture fixture) => _fixture = fixture;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var postgresContainer = _fixture.Create<PostgreSqlContainer>();
        var rabbitMqContainer = _fixture.Create<RabbitMqContainer>();

        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["POSTGRES_CONNECTION_STRING"] = postgresContainer.GetConnectionString(),
            ["RABBITMQ_AMQP_SERVER_URL"] = rabbitMqContainer.GetConnectionString()
        }));
    }
}