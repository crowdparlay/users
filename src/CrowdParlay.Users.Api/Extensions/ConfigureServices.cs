using CrowdParlay.Communication;
using CrowdParlay.Users.Api.Middlewares;
using CrowdParlay.Users.Api.Services;
using MassTransit;
using Serilog;

namespace CrowdParlay.Users.Api.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection ConfigureApiServices(
        this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.With<ActivityLoggingEnricher>()
            .CreateLogger();

        services
            .ConfigureEndpoints()
            .ConfigureOpenIddict(configuration, environment)
            .ConfigureCors(configuration)
            .AddSingleton<ExceptionHandlingMiddleware>()
            .AddGrpc();

        return services.AddMassTransit(bus => bus.UsingRabbitMq((context, configurator) =>
        {
            var amqpServerUrl =
                configuration["RABBITMQ_AMQP_SERVER_URL"]
                ?? throw new InvalidOperationException("Missing required configuration 'RABBITMQ_AMQP_SERVER_URL'.");

            configurator.Host(amqpServerUrl);
            configurator.ConfigureEndpoints(context);
            configurator.ConfigureTopology();
        }));
    }
}