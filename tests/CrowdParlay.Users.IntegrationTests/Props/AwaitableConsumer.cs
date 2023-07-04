using CrowdParlay.Communication.Abstractions;

namespace CrowdParlay.Users.IntegrationTests.Props;

public class AwaitableConsumer<TMessage> : IMessageListener<TMessage> where TMessage : Message
{
    private TaskCompletionSource<TMessage> _tcs = new();

    public async Task<TMessage> ConsumeOne()
    {
        var message = await _tcs.Task;
        _tcs = new TaskCompletionSource<TMessage>();
        return message;
    }

    public Task HandleAsync(TMessage message)
    {
        _tcs.SetResult(message);
        return Task.CompletedTask;
    }
}