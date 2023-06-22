using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Application.Features.Users.Commands;
using Dodo.Primitives;
using Mediator;
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
    public async Task Delete([FromBody] Delete.Command command)
    {
        if (User.Identity?.IsAuthenticated != true)
            throw new UnauthorizedException();

        var userId = Uuid.Parse(User.GetClaim(OpenIddictConstants.Claims.Subject)!);

        if (command.Id == userId)
            throw new ForbiddenException("The specified ID doesn't equals the ID of the authenticated user");

        await Mediator.Send(command);
    }
}