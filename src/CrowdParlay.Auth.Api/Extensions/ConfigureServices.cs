using CrowdParlay.Auth.Api.Filters;
using CrowdParlay.Auth.Api.Routing;
using CrowdParlay.Auth.Api.Services;
using CrowdParlay.Auth.Application.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.OpenApi.Models;
using Serilog;

namespace CrowdParlay.Auth.Api.Extensions;

public static class ConfigureServices
{
    private const string SwaggerIgnoredNamespaceIdentifiersKey = "Swagger:IgnoredNamespaceIdentifiers";
    
    public static IServiceCollection ConfigureApiServices(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services
            .ConfigureAuthentication()
            .ConfigureOpenIddict(configuration, environment);
        
        services
            .AddScoped<ICurrentUserProvider, CurrentUserProvider>()
            .AddEndpointsApiExplorer()
            .AddHealthChecks();
        
        // Logging (Serilog)
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.With<ActivityLoggingEnricher>()
            .CreateLogger();

        // Controllers, naming conventions and request filtering
        services.AddControllers(options =>
        {
            var transformer = new KebabCaseParameterPolicy();
            options.Conventions.Add(new RouteTokenTransformerConvention(transformer));
            options.Filters.Add<ApiExceptionFilterAttribute>();
        });

        // Swagger UI
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "CrowdParlay Auth API", Version = "v1" });

            options.CustomSchemaIds(type =>
            {
                // Ignored parts of namespaces, generally CQRS-conventional names,
                // such as 'Queries' and 'Commands'. These are skipped when generating
                // Swagger names for the public DTOs.
                var ignoredIdentifiers = configuration
                    .GetSection(SwaggerIgnoredNamespaceIdentifiersKey)
                    .Get<string[]>()!;

                // Generates unique and user-friendly names for CQRS entities.
                // For example, 'Features.Accounts.Commands.Create.Command' gets turned into 'AccountsCreateCommand'.
                var lastNames = type.FullName!.Split('.')
                    .Except(ignoredIdentifiers)
                    .TakeLast(2)
                    .Select(name => name.Replace("+", string.Empty));

                return string.Join(string.Empty, lastNames);
            });
        });
        
        return services;
    }
}