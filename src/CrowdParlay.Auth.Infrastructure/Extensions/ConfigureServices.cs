using CrowdParlay.Auth.Application.Abstractions.Identity;
using CrowdParlay.Auth.Infrastructure.Identity;
using CrowdParlay.Auth.Infrastructure.Identity.Services;
using CrowdParlay.Auth.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DbContext = CrowdParlay.Auth.Infrastructure.Persistence.DbContext;

namespace CrowdParlay.Auth.Infrastructure.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection ConfigurePersistenceServices(this IServiceCollection services)
    {
        services
            .AddTransient(typeof(Application.Abstractions.Identity.IPasswordValidator<>), typeof(Identity.Services.PasswordValidator<>))
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