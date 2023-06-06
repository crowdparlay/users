using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Services;
using CrowdParlay.Users.Infrastructure.Persistence.Abstractions;
using CrowdParlay.Users.Infrastructure.Persistence.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Users.Infrastructure.Persistence.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection ConfigurePersistenceServices(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory>(_ =>
        {
            var connectionString = Environment.ExpandEnvironmentVariables(@"
                Host     = %POSTGRES_HOST%;
                Port     = %POSTGRES_PORT%;
                Database = %POSTGRES_DB%;
                Username = %POSTGRES_USER%;
                Password = %POSTGRES_PASSWORD%");

            return new SqlConnectionFactory(connectionString);
        });

        return services
            .AddScoped<IUserService, UserService>()
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddHostedService<DataStoreInitializer>();
    }
}