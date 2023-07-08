using CrowdParlay.Users.Api;
using CrowdParlay.Users.IntegrationTests.Services;

namespace CrowdParlay.Users.IntegrationTests.Setups;

public class WebApplicationSetup : ICustomization
{
    public void Customize(IFixture fixture)
    {
        var client = new TestWebApplicationFactory<Program>(fixture).CreateClient();
        fixture.Inject(client);
    }
}