using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Users.Api.Routing;

/// <summary>
/// A route attribute responsible for applying a default template containing prefix to the given controller or endpoint route.
/// The default pattern is /api/{route}
/// If a route that starts with '~/' is provided, adds the default '/api' prefix to the beginning.
/// </summary>
public class ApiRouteAttribute : RouteAttribute
{
    private const string Prefix = "api";

    public ApiRouteAttribute() : base(Prefix) { }

    public ApiRouteAttribute(
        [StringSyntax("Route")] string template)
        : base(template.StartsWith("~/")
            ? $"~/{Prefix}/{template[2..]}"
            : $"{Prefix}/{template}") { }
}