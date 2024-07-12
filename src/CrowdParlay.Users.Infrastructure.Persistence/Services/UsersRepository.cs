using System.Runtime.CompilerServices;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using CrowdParlay.Users.Infrastructure.Persistence.Abstractions;
using Dapper;
using Dodo.Primitives;

namespace CrowdParlay.Users.Infrastructure.Persistence.Services;

internal class UsersRepository : IUsersRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UsersRepository(IDbConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory;

    public async IAsyncEnumerable<User> GetByIdsAsync(Guid[] ids, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var reader = await connection.ExecuteReaderAsync(
            $"SELECT * FROM {UserSchema.Table} WHERE {UserSchema.Id} = ANY(@{nameof(ids)})",
            new { ids });

        var parser = reader.GetRowParser<User>();
        while (await reader.ReadAsync(cancellationToken))
            yield return parser(reader);
    }

    public async Task<User?> GetByIdAsync(Uuid id, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            $"SELECT * FROM {UserSchema.Table} WHERE {UserSchema.Id} = @{nameof(id)}",
            new { id });
    }

    public async Task<User?> GetByUsernameExactAsync(string username, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            $"SELECT * FROM {UserSchema.Table} WHERE {UserSchema.Username} = @{nameof(username)}",
            new { username });
    }

    public async Task<User?> GetByUsernameNormalizedAsync(string username, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            $"SELECT * FROM {UserSchema.Table} WHERE {UserSchema.UsernameNormalized} = normalize_username(@{nameof(username)})",
            new { username });
    }

    public async Task<User?> GetByEmailNormalizedAsync(string email, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(
            $"SELECT * FROM {UserSchema.Table} WHERE {UserSchema.EmailNormalized} = normalize_email(@{nameof(email)})",
            new { email });
    }

    public async Task<User?> GetByUsernameOrEmailNormalizedAsync(string usernameOrEmail, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<User>(sql:
            $"""
             SELECT * FROM {UserSchema.Table}
             WHERE {UserSchema.UsernameNormalized} = normalize_username(@{nameof(usernameOrEmail)})
             OR {UserSchema.EmailNormalized} = normalize_email(@{nameof(usernameOrEmail)})
             """,
            new { usernameOrEmail });
    }

    public async Task<IEnumerable<User>> GetManyAsync(int count, int page, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<User>(
            $"SELECT * FROM {UserSchema.Table} LIMIT @{nameof(count)} OFFSET @{nameof(count)} * @{nameof(page)}",
            new { count, page });
    }

    public async Task AddAsync(User entity, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            $"""
             INSERT INTO {UserSchema.Table} (
                 {UserSchema.Id},
                 {UserSchema.Username},
                 {UserSchema.Email},
                 {UserSchema.DisplayName},
                 {UserSchema.AvatarUrl},
                 {UserSchema.PasswordHash},
                 {UserSchema.CreatedAt}
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
             UPDATE {UserSchema.Table} SET
             {UserSchema.Id} = @{nameof(User.Id)},
             {UserSchema.Username} = @{nameof(User.Username)},
             {UserSchema.Email} = @{nameof(User.Email)},
             {UserSchema.DisplayName} = @{nameof(User.DisplayName)},
             {UserSchema.AvatarUrl} = @{nameof(User.AvatarUrl)},
             {UserSchema.PasswordHash} = @{nameof(User.PasswordHash)},
             {UserSchema.CreatedAt} = @{nameof(User.CreatedAt)}
             WHERE {UserSchema.Id} = @{nameof(entity.Id)}
             """,
            entity);
    }

    public async Task DeleteAsync(Uuid id, CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var count = await connection.ExecuteAsync(
            $"DELETE FROM {UserSchema.Table} WHERE {UserSchema.Id} = @{nameof(id)}",
            new { id });

        if (count == 0)
            throw new NotFoundException();
    }
}