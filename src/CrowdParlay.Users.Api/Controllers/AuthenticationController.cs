using CrowdParlay.Users.Application.Features.Authentication.Commands;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace CrowdParlay.Users.Api.Controllers;

public class AuthenticationController : ApiControllerBase
{
    [HttpPost("~/connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest()!;
        return request.GrantType switch
        {
            OpenIddictConstants.GrantTypes.Password => await ExchangePasswordAsync(
                new ExchangePassword.Command(request.Username!, request.Password!, request.Scope ?? string.Empty)),
            OpenIddictConstants.GrantTypes.RefreshToken => await ExchangeRefreshTokenAsync(
                new ExchangeRefreshToken.Command(request.Username!, request.Scope ?? string.Empty, HttpContext)),
            _ => BadRequest("The specified grant type is not supported.")
        };
    }

    private async Task<IActionResult> ExchangePasswordAsync(ExchangePassword.Command command)
    {
        var response = await Mediator.Send(command);
        return SignIn(response.Principal, response.Properties, response.AuthenticationScheme);
    }

    private async Task<IActionResult> ExchangeRefreshTokenAsync(ExchangeRefreshToken.Command command)
    {
        var response = await Mediator.Send(command with { Context = HttpContext });
        return SignIn(response.Principal, response.Properties, response.AuthenticationScheme);
    }
}