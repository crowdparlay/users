using CrowdParlay.Communication.RabbitMq.DependencyInjection;
using CrowdParlay.Users.Api.Middlewares;
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
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.With<ActivityLoggingEnricher>()
            .CreateLogger();

        services
            .ConfigureAuthentication()
            .ConfigureOpenIddict(configuration, environment)
            .ConfigureSwagger(configuration)
            .AddEndpointsApiExplorer()
            .AddSingleton<ExceptionHandlingMiddleware>();

        var mvcBuilder = services.AddControllers(options =>
        {
            var transformer = new KebabCaseParameterPolicy();
            options.Conventions.Add(new RouteTokenTransformerConvention(transformer));
        });

        mvcBuilder.AddNewtonsoftJson();

        var rabbitMqAmqpServerUrl =
            configuration["RABBITMQ_AMQP_SERVER_URL"]
            ?? throw new InvalidOperationException("Missing required configuration 'RABBITMQ_AMQP_SERVER_URL'.");

        return services.AddRabbitMqCommunication(options => options
            .UseAmqpServer(rabbitMqAmqpServerUrl));
    }
}