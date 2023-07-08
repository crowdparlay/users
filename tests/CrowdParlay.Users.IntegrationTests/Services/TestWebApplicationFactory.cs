using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CrowdParlay.Users.IntegrationTests.Services;

internal class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly IFixture _fixture;

    public TestWebApplicationFactory(IFixture fixture) => _fixture = fixture;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var postgresConfiguration = _fixture.Create<PostgresContainerConfiguration>();
        var rabbitMqConfiguration = _fixture.Create<RabbitMqContainerConfiguration>();

        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["POSTGRES_CONNECTION_STRING"] = postgresConfiguration.ConnectionString,
            ["RABBITMQ_AMQP_SERVER_URL"] = rabbitMqConfiguration.AmqpServerUrl
        }));
    }
}