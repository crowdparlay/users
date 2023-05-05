using CrowdParlay.Users.Application.Abstractions.Communication;
using KafkaFlow;

namespace CrowdParlay.Users.Infrastructure.Communication.Services;

public class KafkaTopic<TMessage> : IMessageDestination<TMessage>
{
    private readonly string _name;
    private readonly IMessageProducer _producer;

    public KafkaTopic(string name, IMessageProducer producer)
    {
        _name = name;
        _producer = producer;
    }

    public async Task PublishAsync(string key, TMessage message) =>
        await _producer.ProduceAsync(_name, key, message);
}