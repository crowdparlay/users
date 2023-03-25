using Mediator;
using Microsoft.Extensions.Logging;

namespace CrowdParlay.Auth.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IMessage
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

    public async ValueTask<TResponse> Handle(TRequest message, CancellationToken cancellationToken, MessageHandlerDelegate<TRequest, TResponse> next)
    {
        var requestType = typeof(TRequest);
        var requestTypeName = requestType.FullName is not null
            // For example, 'Accounts.Commands.Register+Command'
            ? string.Join('.', requestType.FullName.Split('.').TakeLast(3))
            : typeof(TRequest).Name;
        
        _logger.LogInformation("Handling {RequestName} {@Message}", requestTypeName, message);

        var response = await next(message, cancellationToken);

        _logger.LogInformation("Handled {RequestName}", requestTypeName);
        return response;
    }
}