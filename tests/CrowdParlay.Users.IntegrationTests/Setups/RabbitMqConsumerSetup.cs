using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CrowdParlay.Users.IntegrationTests.Setups;

public class RabbitMqConsumerSetup : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var connectionFactory = new ConnectionFactory
        {
            Uri = new Uri(fixture.Create<RabbitMqContainerConfiguration>().AmqpServerUrl)
        };

        var connection = connectionFactory.CreateConnection();
        var channel = connection.CreateModel();
        fixture.Inject(channel);

        var consumer = new EventingBasicConsumer(channel);
        fixture.Inject(consumer);
    }
}