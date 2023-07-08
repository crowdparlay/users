using CrowdParlay.Communication.RabbitMq.DependencyInjection;
using CrowdParlay.Users.Api.Filters;
using CrowdParlay.Users.Api.Routing;
using CrowdParlay.Users.Api.Services;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Serilog;

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

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.With<ActivityLoggingEnricher>()
            .CreateLogger();

        services.AddControllers(options =>
        {
            var transformer = new KebabCaseParameterPolicy();
            options.Conventions.Add(new RouteTokenTransformerConvention(transformer));
            options.Filters.Add<ApiExceptionFilterAttribute>();
        });

        var rabbitMqAmqpServerUrl =
            configuration["RABBITMQ_AMQP_SERVER_URL"]
            ?? throw new InvalidOperationException("Missing required configuration 'RABBITMQ_AMQP_SERVER_URL'.");

        return services.AddRabbitMqCommunication(options => options
            .UseAmqpServer(rabbitMqAmqpServerUrl));
    }
}