using CrowdParlay.Users.Application.Abstractions.Communication;
using KafkaFlow.Producers;
using Microsoft.Extensions.Configuration;

namespace CrowdParlay.Users.Infrastructure.Communication.Services;

public sealed class KafkaMessageBroker : IMessageBroker
{
    public IMessageDestination UserEvents { get; }

    public KafkaMessageBroker(IConfiguration configuration, IProducerAccessor producerAccessor)
    {
        var producerName =
            configuration["KAFKA_PRODUCER_NAME"]
            ?? throw new InvalidOperationException("Missing required configuration 'KAFKA_PRODUCER_NAME'");

        var producer = producerAccessor.GetProducer(producerName);

        UserEvents = new KafkaTopic("user-events", producer);
    }
}