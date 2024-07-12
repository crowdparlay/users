using System.Net;
using System.Net.Mime;
using CrowdParlay.Users.Api.Extensions;
using CrowdParlay.Users.Api.v1.DTOs;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.Application.Features.Users.Queries;
using Dodo.Primitives;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Users.Api.v1.Controllers;

[ApiVersion("1.0")]
public class UsersController : ApiControllerBase
{
    /// <summary>
    /// Creates a user.
    /// </summary>
    [HttpPost("[action]")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Register.Response), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblem), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.Forbidden)]
    public async Task<Register.Response> Register([FromBody] UsersRegisterRequest request)
    {
        if (HttpContext.User.Identity?.IsAuthenticated == true)
            throw new ForbiddenException();

        return await Mediator.Send(request.Adapt<Register.Command>());
    }

    /// <summary>
    /// Returns user with the specified ID.
    /// </summary>
    [HttpGet("{userId}")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(GetById.Response), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.NotFound)]
    public async Task<GetById.Response> GetById([FromRoute] Uuid userId) =>
        await Mediator.Send(new GetById.Query(userId));

    /// <summary>
    /// Returns current authenticated user.
    /// </summary>
    [HttpGet("[action]"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(GetById.Response), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.NotFound)]
    public async Task<UserInfoResponse> Self()
    {
        var user = await Mediator.Send(new GetById.Query(HttpContext.GetUserId()!.Value));
        return user.Adapt<UserInfoResponse>();
    }

    /// <summary>
    /// Returns user with the specified username.
    /// </summary>
    [HttpGet("[action]")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(GetByUsername.Response), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblem), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.NotFound)]
    public async Task<GetByUsername.Response> Resolve([FromQuery] string username) =>
        await Mediator.Send(new GetByUsername.Query(username));

    /// <summary>
    /// Updates user with the specified ID.
    /// </summary>
    [HttpPut("{userId}"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Update.Response), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblem), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.NotFound)]
    public async Task<Update.Response> Update([FromRoute] Uuid userId, [FromBody] UsersUpdateRequest request)
    {
        if (userId != HttpContext.GetUserId())
            throw new ForbiddenException();

        return await Mediator.Send(request.Adapt<Update.Command>() with { Id = userId });
    }

    /// <summary>
    /// Deletes user with the specified ID.
    /// </summary>
    [HttpDelete("{userId}"), Authorize]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Update.Response), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.Forbidden)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.NotFound)]
    public async Task Delete([FromRoute] Uuid userId)
    {
        if (userId != HttpContext.GetUserId())
            throw new ForbiddenException();

        await Mediator.Send(new Delete.Command(userId));
    }
}