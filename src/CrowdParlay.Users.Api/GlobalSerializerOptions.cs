using System.Text.Json;
using CrowdParlay.Users.Api.Services;

namespace CrowdParlay.Users.Api;

public static class GlobalSerializerOptions
{
    public static JsonSerializerOptions SnakeCase { get; } = new()
    {
        PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy(), 
        DictionaryKeyPolicy = new SnakeCaseJsonNamingPolicy() 
    };
}