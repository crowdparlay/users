using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Services;
using CrowdParlay.Users.Domain.Entities;
using CrowdParlay.Users.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Users.Infrastructure.Persistence.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection ConfigurePersistenceServices(this IServiceCollection services)
    {
        services
            .AddTransient(typeof(Application.Abstractions.IPasswordValidator<>), typeof(Application.Services.PasswordValidator<>))
            .AddScoped<IUserService, UserService>()
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddHostedService<DataStoreInitializer>();

        services
            .AddIdentity<User, IdentityRole<Guid>>()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<DbContext>();

        var connectionString = Environment.ExpandEnvironmentVariables(
            "Host     = %POSTGRES_HOST%;" +
            "Port     = %POSTGRES_PORT%;" +
            "Database = %POSTGRES_DB%;" +
            "Username = %POSTGRES_USER%;" +
            "Password = %POSTGRES_PASSWORD%");

        services.AddDbContext<DbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            options.UseOpenIddict();
        });

        return services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 5;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = false;
        });
    }
}