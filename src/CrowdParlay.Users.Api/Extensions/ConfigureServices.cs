using CrowdParlay.Users.Api.Filters;
using CrowdParlay.Users.Api.Routing;
using CrowdParlay.Users.Api.Services;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CrowdParlay.Users.Api.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection ConfigureApiServices(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services
            .ConfigureAuthentication()
            .ConfigureOpenIddict(configuration, environment)
            .ConfigureSwagger(configuration);

        services
            .AddEndpointsApiExplorer()
            .AddHealthChecks();

        // Controllers, naming conventions and request filtering
        services.AddControllers(options =>
        {
            var transformer = new KebabCaseParameterPolicy();
            options.Conventions.Add(new RouteTokenTransformerConvention(transformer));
            options.Filters.Add<ApiExceptionFilterAttribute>();
        });

        return services;
    }
}