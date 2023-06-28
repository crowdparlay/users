using CrowdParlay.Users.Application.Abstractions.Communication;
using RabbitMQ.Client;

namespace CrowdParlay.Users.Infrastructure.Communication.Services;

public sealed class RabbitMqMessageBroker : IMessageBroker
{
    public IMessageDestination UserEvents { get; }

    public RabbitMqMessageBroker(IConnectionFactory connectionFactory) =>
        UserEvents = new RabbitMqExchange(RabbitMqConstants.Exchanges.Users, connectionFactory);
}