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

    public async Task<User> GetByIdAsync(Uuid id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleAsync<User>(
            $"SELECT * FROM {UserSchema.Table} WHERE {UserSchema.Id} = @{nameof(id)}",
            new { id });
    }

    public async Task<IEnumerable<User>> GetManyAsync(int count, int page = 0)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<User>(
            $"SELECT * FROM {UserSchema.Table} LIMIT @{nameof(count)} OFFSET @{nameof(count)} * @{nameof(page)}",
            new { count, page });
    }

    public async Task AddAsync(User entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            $@"INSERT INTO {UserSchema.Table} (
                {UserSchema.Id},
                {UserSchema.Username},
                {UserSchema.DisplayName}
            )
            VALUES (
                @{nameof(User.Id)},
                @{nameof(User.Username)},
                @{nameof(User.DisplayName)}
            )",
            entity);
    }

    public async Task UpdateAsync(User entity)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            $@"UPDATE {UserSchema.Table} SET
            {UserSchema.Id} = @{nameof(User.Id)},
            {UserSchema.Username} = @{nameof(User.Username)},
            {UserSchema.DisplayName} = @{nameof(User.DisplayName)}
            WHERE {UserSchema.Id} = @{nameof(entity.Id)}",
            entity);
    }

    public async Task DeleteAsync(Uuid id)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            $@"DELETE FROM {UserSchema.Table} WHERE @{UserSchema.Id} = @{nameof(id)}",
            new { id });
    }
}