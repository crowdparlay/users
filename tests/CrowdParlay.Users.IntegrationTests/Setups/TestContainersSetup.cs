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
        var container = new PostgreSqlBuilder().Build();
        AsyncContext.Run(async () => await container.StartAsync());
        fixture.Inject(container);
    }

    private void CustomizeRabbitMq(IFixture fixture)
    {
        var container = new RabbitMqBuilder().Build();
        AsyncContext.Run(async () => await container.StartAsync());
        fixture.Inject(container);

        var amqpServerUrl = container.GetConnectionString();
        var connectionFactory = new ConnectionFactory { Uri = new Uri(amqpServerUrl) };
        var broker = new RabbitMqMessageBroker(connectionFactory);
        fixture.Inject(broker);
    }
}