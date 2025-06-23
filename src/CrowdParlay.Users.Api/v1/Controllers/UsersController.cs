using System.Net;
using System.Net.Mime;
using System.Text.Json;
using CrowdParlay.Users.Api.Extensions;
using CrowdParlay.Users.Api.v1.DTOs;
using CrowdParlay.Users.Application;
using CrowdParlay.Users.Application.Exceptions;
using CrowdParlay.Users.Application.Features.Users.Commands;
using CrowdParlay.Users.Application.Features.Users.Queries;
using CrowdParlay.Users.Application.Models;
using CrowdParlay.Users.Domain;
using Dodo.Primitives;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Users.Api.v1.Controllers;

[ApiVersion("1.0")]
public class UsersController : ApiControllerBase
{
    private readonly IDataProtector _externalLoginTicketProtector;

    public UsersController(IDataProtectionProvider dataProtectionProvider) =>
        _externalLoginTicketProtector = dataProtectionProvider.CreateProtector(ExternalLoginTicketsConstants.DataProtectionPurpose);

    /// <summary>
    /// Returns users.
    /// </summary>
    [HttpGet]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Page<Search.Response>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblem), (int)HttpStatusCode.BadRequest)]
    public async Task<Page<Search.Response>> Search(SortingStrategy order, int offset, int count) =>
        await Mediator.Send(new Search.Query(order, offset, count));

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

        Register.Command command;
        if (request.ExternalLoginTicketId is null)
            command = new Register.Command(request.Username, request.Email, request.DisplayName, request.Password, request.AvatarUrl, null);
        else
        {
            var ticketCookieKey = string.Format(ExternalLoginTicketsConstants.CookieKeyTemplate, request.ExternalLoginTicketId);
            var encryptedTicketJson =
                Request.Cookies[ticketCookieKey]
                ?? throw new ValidationException(nameof(request.ExternalLoginTicketId),
                    "No external login ticket with the specified ID can be retrieved from Cookies.");

            var ticketJson = _externalLoginTicketProtector.Unprotect(encryptedTicketJson);
            var ticket = JsonSerializer.Deserialize<ExternalLoginTicket>(ticketJson, GlobalSerializerOptions.SnakeCase)!;
            command = new Register.Command(request.Username, request.Email, request.DisplayName, request.Password, request.AvatarUrl, ticket);
        }

        var response = await Mediator.Send(command);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, response.Id);

        var ticketCookies = Request.Cookies.Where(cookie => ExternalLoginTicketsConstants.CookieKeyRegex.IsMatch(cookie.Key));
        foreach (var cookie in ticketCookies)
            Response.Cookies.Delete(cookie.Key);

        return response;
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
    public async Task<GetById.Response> Self() =>
        await Mediator.Send(new GetById.Query(HttpContext.GetUserId()!.Value));

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

        await HttpContext.SignOutAsync();
        await Mediator.Send(new Delete.Command(userId));
    }
}