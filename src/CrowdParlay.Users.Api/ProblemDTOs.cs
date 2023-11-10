using System.Net;
using System.Text.Json.Serialization;

namespace CrowdParlay.Users.Api;

public class Problem
{
    [JsonIgnore] public HttpStatusCode HttpStatusCode { get; init; }
    public required string ErrorDescription { get; set; }
}

public class ValidationProblem : Problem
{
    public required IDictionary<string, string[]> ValidationErrors { get; set; }
}