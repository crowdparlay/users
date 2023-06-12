using CrowdParlay.Users.Application.Abstractions.Communication;
using CrowdParlay.Users.Infrastructure.Communication.Services;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Users.Infrastructure.Communication.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection ConfigureCommunicationServices(this IServiceCollection services, IConfiguration configuration) =>
        configuration.GetValue<bool>("Circumstances:FakeCommunication")
            ? services.AddFakeCommunication()
            : services.AddKafkaCommunication(configuration);

    private static IServiceCollection AddFakeCommunication(this IServiceCollection services) => services
        .AddScoped<IMessageBroker, FakeMessageBroker>();

    private static IServiceCollection AddKafkaCommunication(this IServiceCollection services, IConfiguration configuration)
    {
        var producerName =
            configuration["KAFKA_PRODUCER_NAME"]
            ?? throw new InvalidOperationException("Missing required configuration 'KAFKA_PRODUCER_NAME'");

        var kafkaBootstrapHost =
            configuration["KAFKA_BOOTSTRAP_SERVER"]
            ?? throw new InvalidOperationException("Missing required configuration 'KAFKA_BOOTSTRAP_SERVER'");

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