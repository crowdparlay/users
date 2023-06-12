using System.Data.Common;

namespace CrowdParlay.Users.Infrastructure.Persistence.Abstractions;

internal interface IDbConnectionFactory
{
    public Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}