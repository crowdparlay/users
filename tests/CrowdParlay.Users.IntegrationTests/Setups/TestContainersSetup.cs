using CrowdParlay.Communication.RabbitMq;
using Nito.AsyncEx;
using RabbitMQ.Client;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace CrowdParlay.Users.IntegrationTests.Setups;

public class TestContainersSetup : ICustomization
{
    public void Customize(IFixture fixture)
    {
        CustomizePostgres(fixture);
        CustomizeRabbitMq(fixture);
    }

    private void CustomizePostgres(IFixture fixture)
    {
        var container = new PostgreSqlBuilder()
            .WithExposedPort(5432)
            .WithPortBinding(5432, true)
            .Build();

        AsyncContext.Run(async () => await container.StartAsync());

        fixture.Inject(new PostgresContainerConfiguration
        {
            ConnectionString = container.GetConnectionString()
        });
    }

    private void CustomizeRabbitMq(IFixture fixture)
    {
        var container = new RabbitMqBuilder().Build();
        AsyncContext.Run(async () => await container.StartAsync());

        var amqpServerUrl = container.GetConnectionString();
        var connectionFactory = new ConnectionFactory { Uri = new Uri(amqpServerUrl) };
        var broker = new RabbitMqMessageBroker(connectionFactory);
        
        fixture.Inject(broker);
        fixture.Inject(new RabbitMqContainerConfiguration
        {
            AmqpServerUrl = container.GetConnectionString()
        });
    }
}