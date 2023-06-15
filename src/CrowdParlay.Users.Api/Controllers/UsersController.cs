using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Application.Features.Users.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace CrowdParlay.Users.Api.Controllers;

public class UsersController : ApiControllerBase
{
    [HttpPost, Route("[action]"), AllowAnonymous]
    public async Task<Register.Response> Register([FromBody] Register.Command command) =>
        await Mediator.Send(command with { IsAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false });

    [HttpDelete, Route("[action]")]
    public async Task<Delete.Response> Delete([FromBody] Delete.Command command)
    {
        if (HttpContext.User.Identity == null)
            throw new UnauthorizedAccessException();

        if (command.id
            .ToString()
            .Equals(HttpContext.User.GetClaim(OpenIddictConstants.Claims.Subject))
           )
            throw new ForbiddenException("The specified ID doesn't equals the ID of the authorized user");

        return await Mediator.Send(command);
    }
}