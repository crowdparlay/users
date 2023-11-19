using CrowdParlay.Users.Infrastructure.Persistence.Abstractions;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CrowdParlay.Users.Infrastructure.Persistence.Services;

public class DatabaseInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDbConnectionFactory _connectionFactory;

    public DatabaseInitializer(IServiceScopeFactory scopeFactory, IDbConnectionFactory connectionFactory)
    {
        _scopeFactory = scopeFactory;
        _connectionFactory = connectionFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var migrations = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        await CreateDatabaseAsync("users");
        migrations.ListMigrations();
        migrations.MigrateUp();

        var openIddictDbContext = scope.ServiceProvider.GetRequiredService<OpenIddictDbContext>();
        await openIddictDbContext.Database.MigrateAsync(cancellationToken);
    }

    private async Task CreateDatabaseAsync(string databaseName)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync();

        const string query = $"SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @{nameof(databaseName)})";
        var databaseExists = await connection.QuerySingleAsync<bool>(query, new { databaseName });

        if (!databaseExists)
            await connection.ExecuteAsync($"CREATE DATABASE {databaseName}");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}