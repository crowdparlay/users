using System.Text.Json;
using CrowdParlay.Users.Api.Services;

namespace CrowdParlay.Users.IntegrationTests;

public static class GlobalSerializerOptions
{
    public static JsonSerializerOptions SnakeCase { get; } = new()
    {
        PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy(), 
        DictionaryKeyPolicy = new SnakeCaseJsonNamingPolicy() 
    };
}