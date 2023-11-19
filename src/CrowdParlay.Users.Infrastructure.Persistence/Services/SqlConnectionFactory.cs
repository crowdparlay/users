using System.Data.Common;
using CrowdParlay.Users.Infrastructure.Persistence.Abstractions;
using Npgsql;

namespace CrowdParlay.Users.Infrastructure.Persistence.Services;

internal class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString) => _connectionString = connectionString;

    public async Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}