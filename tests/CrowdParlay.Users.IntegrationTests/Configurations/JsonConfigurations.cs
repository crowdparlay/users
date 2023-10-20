using System.Text.Json;
using CrowdParlay.Users.Api.Services;

namespace CrowdParlay.Users.IntegrationTests.Configurations;

public static class JsonConfigurations
{
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy(), 
        DictionaryKeyPolicy = new SnakeCaseJsonNamingPolicy() 
    };
}