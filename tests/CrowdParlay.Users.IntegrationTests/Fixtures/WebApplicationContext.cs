using CrowdParlay.Users.IntegrationTests.Setups;

namespace CrowdParlay.Users.IntegrationTests.Fixtures;

public class WebApplicationContext
{
    public readonly IFixture Fixture = new Fixture()
        .Customize(new PostgresSetup())
        .Customize(new RabbitMqSetup())
        .Customize(new WebApplicationSetup());
}