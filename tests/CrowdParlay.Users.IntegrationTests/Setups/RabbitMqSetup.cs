using CrowdParlay.Communication.RabbitMq;
using Nito.AsyncEx;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

namespace CrowdParlay.Users.IntegrationTests.Setups;

public class RabbitMqSetup : ICustomization
{
    private RabbitMqContainer? _container;
    private RabbitMqMessageBroker? _broker;

    public void Customize(IFixture fixture)
    {
        fixture.Register(() =>
        {
            if (_container is null)
            {
                _container = new RabbitMqBuilder().Build();
                AsyncContext.Run(async () => await _container.StartAsync());
            }

            return _container!;
        });
        
        fixture.Register((RabbitMqContainer container) =>
        {
            if (_broker is null)
            {
                var amqpServerUrl = container.GetConnectionString();
                var connectionFactory = new ConnectionFactory { Uri = new Uri(amqpServerUrl) };
                _broker = new RabbitMqMessageBroker(connectionFactory);
            }

            return _broker!;
        });
    }
}