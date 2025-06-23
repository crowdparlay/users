using CrowdParlay.Communication;
using CrowdParlay.Users.Api.Middlewares;
using CrowdParlay.Users.Api.Services;
using MassTransit;
using Microsoft.AspNetCore.DataProtection;
using Serilog;
using StackExchange.Redis;

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
            .ConfigureAuthentication()
            .ConfigureCors(configuration)
            .AddKeyedSingleton("SnakeCase", GlobalSerializerOptions.SnakeCase)
            .AddSingleton<ExceptionHandlingMiddleware>()
            .AddHttpClient()
            .AddGrpc();

        var dataProtectionRedisConnectionString = configuration["DATA_PROTECTION_REDIS_CONNECTION_STRING"]!;
        var dataProtectionRedisMultiplexer = ConnectionMultiplexer.Connect(dataProtectionRedisConnectionString);
        services.AddDataProtection().PersistKeysToStackExchangeRedis(dataProtectionRedisMultiplexer);

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