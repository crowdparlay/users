using System.Text.Json;
using System.Text.RegularExpressions;

namespace CrowdParlay.Users.Api.Services;

public partial class SnakeCaseJsonNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) =>
        BorderingWordsRegex().Replace(name, "$1_$2").ToLower();
    
    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex BorderingWordsRegex();
}