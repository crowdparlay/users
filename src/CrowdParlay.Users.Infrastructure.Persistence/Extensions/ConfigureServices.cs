using System.Reflection;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Infrastructure.Persistence.Abstractions;
using CrowdParlay.Users.Infrastructure.Persistence.Services;
using Dapper;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Users.Infrastructure.Persistence.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection ConfigurePersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        var connectionString =
            configuration["POSTGRES_CONNECTION_STRING"]
            ?? throw new InvalidOperationException("Missing required configuration 'POSTGRES_CONNECTION_STRING'");

        services.AddHealthChecks()
            .AddNpgSql(connectionString);

        return services
            .AddFluentMigratorCore()
            .ConfigureRunner(builder => builder.AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(Assembly.GetExecutingAssembly()).For.All())
            .AddDbContext<OpenIddictDbContext>(options => options
                .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                .UseNpgsql(connectionString)
                .UseOpenIddict())
            .AddSingleton<IDbConnectionFactory>(new SqlConnectionFactory(connectionString))
            .AddHostedService<DatabaseInitializer>()
            .AddScoped<IUsersRepository, UsersRepository>()
            .AddScoped<IExternalLoginsRepository, ExternalLoginsRepository>();
    }
}