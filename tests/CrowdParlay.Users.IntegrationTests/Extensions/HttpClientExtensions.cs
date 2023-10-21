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
        var oauthResponseDocument = await JsonDocument.ParseAsync(await oauthMessage.Content.ReadAsStreamAsync());
        return oauthResponseDocument.RootElement.GetProperty("access_token").GetString()!;
    }
}