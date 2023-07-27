using Nito.AsyncEx;
using Testcontainers.PostgreSql;

namespace CrowdParlay.Users.IntegrationTests.Setups;

public class PostgresSetup : ICustomization
{
    private PostgreSqlContainer? _container;

    public void Customize(IFixture fixture) => fixture.Register(() =>
    {
        if (_container is null)
        {
            _container = new PostgreSqlBuilder()
                .WithExposedPort(5432)
                .WithPortBinding(5432, true)
                .Build();
            
            AsyncContext.Run(async () => await _container.StartAsync());
        }

        return _container;
    });
}