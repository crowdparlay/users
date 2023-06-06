using System.Data;
using System.Data.SqlClient;
using CrowdParlay.Users.Infrastructure.Persistence.Abstractions;

namespace CrowdParlay.Users.Infrastructure.Persistence.Services;

internal class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(string connectionString) => _connectionString = connectionString;

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken? cancellationToken = null)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken ?? CancellationToken.None);
        return connection;
    }
}