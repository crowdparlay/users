using CrowdParlay.Users.Application.Abstractions.Communication;
using CrowdParlay.Users.Infrastructure.Communication.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace CrowdParlay.Users.Infrastructure.Communication.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection ConfigureCommunicationServices(this IServiceCollection services, IConfiguration configuration) => services
        .AddScoped<IMessageBroker, RabbitMqMessageBroker>()
        .AddSingleton<IConnectionFactory>(new ConnectionFactory
        {
            Uri = new Uri(
                configuration["RABBITMQ_AMQP_SERVER_URL"]
                ?? throw new InvalidOperationException("Missing required configuration 'RABBITMQ_AMQP_SERVER_URL'"))
        });
}