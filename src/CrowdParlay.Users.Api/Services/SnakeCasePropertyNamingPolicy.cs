using System.Text.Json;

namespace CrowdParlay.Users.Api.Services;

public class SnakeCasePropertyNamingPolicy : JsonNamingPolicy
{
    public static SnakeCasePropertyNamingPolicy Instance { get; } = new();

    public override string ConvertName(string name)
    {
        var nameWithUnderscoredCapitals = name.Select((character, position) =>
            position > 0 && char.IsUpper(character)
                ? $"_{character}"
                : character.ToString());

        return string.Concat(nameWithUnderscoredCapitals).ToLower();
    }
}