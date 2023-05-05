using KafkaFlow;
using Microsoft.Extensions.Hosting;

namespace CrowdParlay.Users.Infrastructure.Communication.Services;

public class KafkaBusRunner : IHostedService
{
    private readonly IKafkaBus _bus;

    public KafkaBusRunner(IServiceProvider serviceProvider) =>
        _bus = serviceProvider.CreateKafkaBus();

    public async Task StartAsync(CancellationToken cancellationToken) =>
        await _bus.StartAsync(cancellationToken);

    public async Task StopAsync(CancellationToken cancellationToken) =>
        await _bus.StopAsync();
}