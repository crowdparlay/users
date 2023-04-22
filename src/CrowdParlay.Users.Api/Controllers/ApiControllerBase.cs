using CrowdParlay.Users.Api.Routing;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Users.Api.Controllers;

/// <summary>
/// A base class for HTTP API controllers. Provides some default annotations (e. g. default API routing convention) and common dependencies.
/// </summary>
[ApiController, ApiRoute("[controller]")]
public abstract class ApiControllerBase : Controller
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator
        ??= HttpContext.RequestServices.GetRequiredService<ISender>();
}