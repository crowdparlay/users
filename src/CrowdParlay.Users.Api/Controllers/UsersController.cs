using CrowdParlay.Users.Api.DTOs;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.Application.Features.Users.Queries;
using Dodo.Primitives;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace CrowdParlay.Users.Api.Controllers;

public class UsersController : ApiControllerBase
{
    [HttpPost, Route("[action]"), AllowAnonymous]
    public async Task<Register.Response> Register([FromBody] UsersRegisterRequest request)
    {
        if (HttpContext.User.Identity?.IsAuthenticated == true)
            throw new ForbiddenException();

        return await Mediator.Send(request.Adapt<Register.Command>());
    }

    [HttpDelete, Route("{userId}")]
    public async Task Delete([FromRoute] Uuid userId)
    {
        if (User.Identity?.IsAuthenticated != true)
            throw new UnauthorizedException();

        var claimUserId = Uuid.Parse(User.GetClaim(OpenIddictConstants.Claims.Subject)!);
        if (userId != claimUserId)
            throw new ForbiddenException("The specified ID isn't equal to the ID of the authenticated user.");

        await Mediator.Send(new Delete.Command(userId));
    }

    [HttpGet, Route("{slug}"), AllowAnonymous]
    public async Task<IActionResult> Read([FromRoute] string slug)
    {
        object request = Uuid.TryParse(slug, out var userId)
            ? new GetById.Query(userId)
            : new GetByUsername.Query(slug);

        var response = await Mediator.Send(request);
        return Ok(response);
    }

    [HttpPut, Route("{userId}")]
    public async Task<Update.Response> Update([FromRoute] Uuid userId, [FromBody] UsersUpdateRequest request) =>
        await Mediator.Send(request.Adapt<Update.Command>() with { Id = userId });
}