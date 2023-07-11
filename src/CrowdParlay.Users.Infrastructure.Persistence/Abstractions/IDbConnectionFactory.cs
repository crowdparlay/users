using System.Data.Common;

namespace CrowdParlay.Users.Infrastructure.Persistence.Abstractions;

public interface IDbConnectionFactory
{
    public Task<DbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}