using System.Data;

namespace CrowdParlay.Users.Infrastructure.Persistence.Abstractions;

internal interface IDbConnectionFactory
{
    public Task<IDbConnection> CreateConnectionAsync(CancellationToken? cancellationToken = null);
}