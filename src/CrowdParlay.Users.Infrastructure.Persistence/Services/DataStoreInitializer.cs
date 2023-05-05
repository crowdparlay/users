using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CrowdParlay.Users.Infrastructure.Persistence.Services;

internal class DataStoreInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DataStoreInitializer(IServiceScopeFactory scopeFactory) =>
        _scopeFactory = scopeFactory;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}