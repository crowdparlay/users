using CrowdParlay.Users.Application.Abstractions.Communication;
using KafkaFlow.Producers;
using Microsoft.Extensions.Configuration;

namespace CrowdParlay.Users.Infrastructure.Communication.Services;

public sealed class KafkaMessageBroker : IMessageBroker
{
    public IMessageDestination<UserCreatedEvent> UserCreatedEvent { get; }
    public IMessageDestination<UserUpdatedEvent> UserUpdatedEvent { get; }
    public IMessageDestination<UserDeletedEvent> UserDeletedEvent { get; }

    public KafkaMessageBroker(IConfiguration configuration, IProducerAccessor producerAccessor)
    {
        var producerName = configuration["KAFKA_PRODUCER_NAME"]!;
        var producer = producerAccessor.GetProducer(producerName);

        UserCreatedEvent = new KafkaTopic<UserCreatedEvent>("user-created-event", producer);
        UserUpdatedEvent = new KafkaTopic<UserUpdatedEvent>("user-updated-event", producer);
        UserDeletedEvent = new KafkaTopic<UserDeletedEvent>("user-deleted-event", producer);
    }
}