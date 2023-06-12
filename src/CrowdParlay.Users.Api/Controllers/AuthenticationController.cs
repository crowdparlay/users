using CrowdParlay.Users.Application.Features.Authentication.Commands;
using Dodo.Primitives;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace CrowdParlay.Users.Api.Controllers;

public class AuthenticationController : ApiControllerBase
{
    [HttpPost("~/connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()!;
        request.Scope ??= string.Empty;

        switch (request.GrantType)
        {
            case OpenIddictConstants.GrantTypes.Password:
            {
                var command = new ExchangePassword.Command(request.Username!, request.Password!, request.Scope);
                var response = await Mediator.Send(command);
                return SignIn(response.Principal, response.Properties, response.AuthenticationScheme);
            }
            case OpenIddictConstants.GrantTypes.RefreshToken:
            {
                var authenticationResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                var userId = Uuid.Parse(authenticationResult.Principal!.GetClaim(OpenIddictConstants.Claims.Subject)!);

                var command = new ExchangeRefreshToken.Command(userId, request.Scope);
                var response = await Mediator.Send(command);
                return SignIn(response.Principal, response.Properties, response.AuthenticationScheme);
            }
            default:
                return BadRequest("The specified grant type is not supported.");
        }
    }
}