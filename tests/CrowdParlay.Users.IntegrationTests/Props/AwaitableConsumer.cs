using System.Threading.Channels;
using CrowdParlay.Communication.Abstractions;

namespace CrowdParlay.Users.IntegrationTests.Props;

public class AwaitableConsumer<TMessage> : IMessageListener<TMessage> where TMessage : Message
{
    private readonly Channel<Message> _channel = Channel.CreateUnbounded<Message>();

    public async Task<Message> ConsumeOne() => await _channel.Reader.ReadAsync();
    public async Task HandleAsync(TMessage message) => await _channel.Writer.WriteAsync(message);
}