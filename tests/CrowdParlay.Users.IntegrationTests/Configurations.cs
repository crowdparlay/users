namespace CrowdParlay.Users.IntegrationTests;

public class PostgresContainerConfiguration
{
    public required string ConnectionString;
}

public class RabbitMqContainerConfiguration
{
    public required string AmqpServerUrl;
}