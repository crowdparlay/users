using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Services;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Infrastructure.Persistence.Abstractions;
using CrowdParlay.Users.Infrastructure.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Users.Infrastructure.Persistence.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection ConfigurePersistenceServices(this IServiceCollection services)
    {
        var connectionString = Environment.ExpandEnvironmentVariables(
            """
            Host     = %POSTGRES_HOST%;
            Port     = %POSTGRES_PORT%;
            Database = %POSTGRES_DB%;
            Username = %POSTGRES_USER%;
            Password = %POSTGRES_PASSWORD%
            """);

        return services
            .AddDbContext<OpenIddictDbContext>(options => options.UseNpgsql(connectionString))
            .AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString))
            .AddScoped<IUsersRepository, UsersRepository>()
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddHostedService<DataStoreInitializer>();
    }
}