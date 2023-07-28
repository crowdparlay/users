namespace CrowdParlay.Users.IntegrationTests.Setups;

public class PrimitivesSetup : ICustomization
{
    public void Customize(IFixture fixture) => fixture.Register(() =>
        Guid.NewGuid().ToString()[..15]);
}