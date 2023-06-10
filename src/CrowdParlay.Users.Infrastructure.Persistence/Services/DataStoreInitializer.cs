using CrowdParlay.Users.Domain.Entities;
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
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            $"""
            CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
            CREATE TABLE IF NOT EXISTS users (
                {UserSchema.Id} UUID PRIMARY KEY,
                {UserSchema.Username} TEXT UNIQUE NOT NULL,
                {UserSchema.DisplayName} TEXT NOT NULL,
                {UserSchema.PasswordHash} TEXT NOT NULL,
                {UserSchema.CreatedAt} TIMESTAMP WITHOUT TIME ZONE
            );
            """);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}