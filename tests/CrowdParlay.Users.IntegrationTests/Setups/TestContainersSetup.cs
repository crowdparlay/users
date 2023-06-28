using Nito.AsyncEx;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace CrowdParlay.Users.IntegrationTests.Setups;

public class TestContainersSetup : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var postgresConfiguration = CustomizePostgres();
        var rabbitMqConfiguration = CustomizeRabbitMq();
        
        fixture.Inject(postgresConfiguration);
        fixture.Inject(rabbitMqConfiguration);
    }

    private PostgresContainerConfiguration CustomizePostgres()
    {
        var postgresContainer = new PostgreSqlBuilder()
            .WithExposedPort(5432)
            .WithPortBinding(5432, true)
            .Build();
        
        AsyncContext.Run(async () => await postgresContainer.StartAsync());

        return new PostgresContainerConfiguration
        {
            ConnectionString = postgresContainer.GetConnectionString()
        };
    }
    
    private RabbitMqContainerConfiguration CustomizeRabbitMq()
    {
        var rabbitMqContainer = new RabbitMqBuilder().Build();
        
        AsyncContext.Run(async () => await rabbitMqContainer.StartAsync());
        
        return new RabbitMqContainerConfiguration
        {
            AmqpServerUrl = rabbitMqContainer.GetConnectionString()
        };
    }
}