using CrowdParlay.Communication.RabbitMq;
using Nito.AsyncEx;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace CrowdParlay.Users.IntegrationTests.Setups;

public class RabbitMqSetup : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var container = new RabbitMqBuilder().Build();
        AsyncContext.Run(async () => await container.StartAsync());

        var amqpServerUrl = container.GetConnectionString();
        var connectionFactory = new ConnectionFactory { Uri = new Uri(amqpServerUrl) };
        var broker = new RabbitMqMessageBroker(connectionFactory);
        
        fixture.Inject(broker);
        fixture.Inject(container);
    }
}