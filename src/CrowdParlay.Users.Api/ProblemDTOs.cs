namespace CrowdParlay.Users.Api;

public record Problem(string ErrorDescription);

public record ValidationProblem(string ErrorDescription, IDictionary<string, string[]> ValidationErrors) : Problem(ErrorDescription);