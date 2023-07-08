using CrowdParlay.Users.IntegrationTests.Setups;

namespace CrowdParlay.Users.IntegrationTests.Attributes;

public class ApiSetupAttribute : AutoDataAttribute
{
    public ApiSetupAttribute() : base(() => new Fixture()
        .Customize(new PostgresSetup())
        .Customize(new RabbitMqSetup())
        .Customize(new WebApplicationSetup())) { }
}