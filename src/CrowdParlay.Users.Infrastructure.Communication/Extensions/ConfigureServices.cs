using CrowdParlay.Users.Application.Abstractions.Communication;
using CrowdParlay.Users.Infrastructure.Communication.Services;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Users.Infrastructure.Communication.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection ConfigureCommunicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var producerName = configuration["KAFKA_PRODUCER_NAME"]!;
        var kafkaBootstrapHost = configuration["KAFKA_BOOTSTRAP_SERVER"]!;

        services.AddKafka(kafka => kafka
            .UseMicrosoftLog()
            .AddCluster(cluster => cluster
                .WithBrokers(new[] { kafkaBootstrapHost })
                .AddProducer(producerName, producer => producer
                    .AddMiddlewares(middlewares => middlewares.AddSerializer<NewtonsoftJsonSerializer>())
                )
            )
        );

        return services
            .AddScoped<IMessageBroker, KafkaMessageBroker>()
            .AddHostedService<KafkaBusRunner>();
    }
}