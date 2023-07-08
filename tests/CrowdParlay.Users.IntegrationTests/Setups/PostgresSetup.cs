using Nito.AsyncEx;
using Testcontainers.PostgreSql;

namespace CrowdParlay.Users.IntegrationTests.Setups;

public class PostgresSetup : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var container = new PostgreSqlBuilder()
            .WithExposedPort(5432)
            .WithPortBinding(5432, true)
            .Build();

        AsyncContext.Run(async () => await container.StartAsync());
        fixture.Inject(container);
    }
}