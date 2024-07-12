using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Infrastructure.Persistence.Extensions;
using CrowdParlay.Users.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Users.Infrastructure.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection ConfigureInfrastructureServices(this IServiceCollection services, IConfiguration configuration) => services
        .ConfigurePersistenceServices(configuration)
        .AddScoped<IGoogleOidcService, GoogleOidcService>();
}