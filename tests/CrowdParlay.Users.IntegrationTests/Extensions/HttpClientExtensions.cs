using System.Net.Http.Json;
using System.Text.Json;

namespace CrowdParlay.Users.IntegrationTests.Extensions;

public static class HttpClientExtensions
{
    public static async Task<string> AcquireAccessToken(this HttpClient client, string username, string password)
    {
        var oauthRequest = new Dictionary<string, string>
        {
            { "grant_type", "password" },
            { "username", username },
            { "password", password }
        };

        var oauthMessage = await client.PostAsync("/connect/token", new FormUrlEncodedContent(oauthRequest));
        var oauthResponseDocument = await oauthMessage.Content.ReadFromJsonAsync<JsonDocument>();
        return oauthResponseDocument!.RootElement.GetProperty("access_token").GetString()!;
    }
}