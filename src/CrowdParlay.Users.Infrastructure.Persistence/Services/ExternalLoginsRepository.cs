using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using CrowdParlay.Users.Infrastructure.Persistence.Abstractions;
using Dapper;
using Dodo.Primitives;

namespace CrowdParlay.Users.Infrastructure.Persistence.Services;

public class ExternalLoginsRepository : IExternalLoginsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ExternalLoginsRepository(IDbConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;
    
    public async Task<IEnumerable<ExternalLogin>> GetByUserIdAsync(Uuid userId, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<ExternalLogin>(
            $"SELECT * FROM {ExternalLoginsSchema.Table} WHERE {ExternalLoginsSchema.UserId} = @{nameof(userId)}",
            new { userId });
    }
    
    public async Task AddAsync(ExternalLogin entity, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            $"""
             INSERT INTO {ExternalLoginsSchema.Table} (
                 {ExternalLoginsSchema.Id},
                 {ExternalLoginsSchema.UserId},
                 {ExternalLoginsSchema.ProviderId},
                 {ExternalLoginsSchema.Identity}
             )
             VALUES (
                 @{nameof(ExternalLogin.Id)},
                 @{nameof(ExternalLogin.UserId)},
                 @{nameof(ExternalLogin.ProviderId)},
                 @{nameof(ExternalLogin.Identity)}
             )
             """,
            entity);
    }
    
    public async Task DeleteAsync(Uuid id, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var count = await connection.ExecuteAsync(
            $"DELETE FROM {ExternalLoginsSchema.Table} WHERE {ExternalLoginsSchema.Id} = @{nameof(id)}",
            new { id });

        if (count == 0)
            throw new NotFoundException();
    }
}