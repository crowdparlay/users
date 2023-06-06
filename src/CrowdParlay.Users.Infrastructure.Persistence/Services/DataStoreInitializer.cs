using CrowdParlay.Users.Infrastructure.Persistence.Abstractions;
using Dapper;
using Microsoft.Extensions.Hosting;

namespace CrowdParlay.Users.Infrastructure.Persistence.Services;

internal class DataStoreInitializer : IHostedService
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DataStoreInitializer(IDbConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS users (
                id UUID PRIMARY KEY,
                username TEXT UNIQUE NOT NULL,
                display_name TEXT NOT NULL,
                created_at TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'utc')
            );
        ");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}