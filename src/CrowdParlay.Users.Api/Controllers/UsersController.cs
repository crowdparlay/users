using CrowdParlay.Users.Application.Features.Users.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Users.Api.Controllers;

public class UsersController : ApiControllerBase
{
    [HttpPost, Route("[action]"), AllowAnonymous]
    public async Task<Register.Response> Register([FromBody] Register.Command command) =>
        await Mediator.Send(command with { IsAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false });
}