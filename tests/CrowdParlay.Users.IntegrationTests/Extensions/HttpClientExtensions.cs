using System.Net.Http.Json;
using System.Text.Json.Nodes;

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
        var oauthResponse = await oauthMessage.Content.ReadFromJsonAsync<JsonObject>();
        return oauthResponse!["access_token"]!.GetValue<string>();
    }
}