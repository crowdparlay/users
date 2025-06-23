using System.Runtime.CompilerServices;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using CrowdParlay.Users.Infrastructure.Persistence.Abstractions;
using Dapper;

namespace CrowdParlay.Users.Infrastructure.Persistence.Services;

internal class UsersRepository : IUsersRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UsersRepository(IDbConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async Task<Page<User>> SearchAsync(SortingStrategy order, int offset, int count, CancellationToken cancellationToken)
    {
        var orderByCreatedAtDirection = order switch
        {
            SortingStrategy.NewestFirst => "DESC",
            SortingStrategy.OldestFirst => "ASC",
            _ => throw new ArgumentOutOfRangeException(nameof(order), order, "The specified order is not supported.")
        };

        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var result = await connection.QueryMultipleAsync(
            $"""
            SELECT COUNT(*) FROM {UsersSchema.Table};
            SELECT * FROM {UsersSchema.Table}
            ORDER BY {UsersSchema.CreatedAt} {orderByCreatedAtDirection}
            LIMIT @{nameof(count)} OFFSET @{nameof(offset)};
            """,
            new { offset, count });

        var totalCount = await result.ReadSingleAsync<int>();
        var users = await result.ReadAsync<User>();
        return new Page<User>(totalCount, users);
    }

    public async IAsyncEnumerable<User> GetByIdsAsync(Guid[] ids, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var reader = await connection.ExecuteReaderAsync(
            $"SELECT * FROM {UsersSchema.Table} WHERE {UsersSchema.Id} = ANY(@{nameof(ids)})",
            new { ids });

        var parser = reader.GetRowParser<User>();
        while (await reader.ReadAsync(cancellationToken))
            yield return parser(reader);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            $"SELECT * FROM {UsersSchema.Table} WHERE {UsersSchema.Id} = @{nameof(id)}",
            new { id });
    }

    public async Task<User?> GetByUsernameExactAsync(string username, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            $"SELECT * FROM {UsersSchema.Table} WHERE {UsersSchema.Username} = @{nameof(username)}",
            new { username });
    }

    public async Task<User?> GetByUsernameNormalizedAsync(string username, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            $"SELECT * FROM {UsersSchema.Table} WHERE {UsersSchema.UsernameNormalized} = normalize_username(@{nameof(username)})",
            new { username });
    }

    public async Task<User?> GetByEmailNormalizedAsync(string email, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            $"SELECT * FROM {UsersSchema.Table} WHERE {UsersSchema.EmailNormalized} = normalize_email(@{nameof(email)})",
            new { email });
    }

    public async Task<User?> GetByUsernameOrEmailNormalizedAsync(string usernameOrEmail, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            $"""
            SELECT * FROM {UsersSchema.Table}
            WHERE {UsersSchema.UsernameNormalized} = normalize_username(@{nameof(usernameOrEmail)})
            OR {UsersSchema.EmailNormalized} = normalize_email(@{nameof(usernameOrEmail)})
            """,
            new { usernameOrEmail });
    }

    public async Task AddAsync(User entity, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            $"""
             INSERT INTO {UsersSchema.Table} (
                 {UsersSchema.Id},
                 {UsersSchema.Username},
                 {UsersSchema.Email},
                 {UsersSchema.DisplayName},
                 {UsersSchema.AvatarUrl},
                 {UsersSchema.PasswordHash},
                 {UsersSchema.CreatedAt}
             )
             VALUES (
                 @{nameof(User.Id)},
                 @{nameof(User.Username)},
                 @{nameof(User.Email)},
                 @{nameof(User.DisplayName)},
                 @{nameof(User.AvatarUrl)},
                 @{nameof(User.PasswordHash)},
                 @{nameof(User.CreatedAt)}
             )
             """,
            entity);
    }

    public async Task UpdateAsync(User entity, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            $"""
             UPDATE {UsersSchema.Table} SET
             {UsersSchema.Id} = @{nameof(User.Id)},
             {UsersSchema.Username} = @{nameof(User.Username)},
             {UsersSchema.Email} = @{nameof(User.Email)},
             {UsersSchema.DisplayName} = @{nameof(User.DisplayName)},
             {UsersSchema.AvatarUrl} = @{nameof(User.AvatarUrl)},
             {UsersSchema.PasswordHash} = @{nameof(User.PasswordHash)},
             {UsersSchema.CreatedAt} = @{nameof(User.CreatedAt)}
             WHERE {UsersSchema.Id} = @{nameof(entity.Id)}
             """,
            entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var count = await connection.ExecuteAsync(
            $"DELETE FROM {UsersSchema.Table} WHERE {UsersSchema.Id} = @{nameof(id)}",
            new { id });

        if (count == 0)
            throw new NotFoundException();
    }

    public async Task<User?> GetByExternalLoginAsync(string providerId, string identity, CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            $"""
             SELECT users.* FROM {UsersSchema.Table} users
             JOIN {ExternalLoginProvidersSchema.Table} providers ON providers.{ExternalLoginProvidersSchema.Id} = @{nameof(providerId)}
             JOIN {ExternalLoginsSchema.Table} logins ON logins.{ExternalLoginsSchema.Identity} = @{nameof(identity)}
             WHERE users.{UsersSchema.Id} = logins.{ExternalLoginsSchema.UserId}
             LIMIT 1
             """,
            new { providerId, identity });
    }
}