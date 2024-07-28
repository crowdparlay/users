using System.Net;
using System.Net.Mime;
using System.Text.Json;
using CrowdParlay.Users.Api.Extensions;
using CrowdParlay.Users.Api.v1.DTOs;
using CrowdParlay.Users.Application;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Models;
using CrowdParlay.Users.Application.Services;
using CrowdParlay.Users.Domain.Abstractions;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Users.Api.v1.Controllers;

[ApiVersion("1.0")]
public class AuthenticationController : ApiControllerBase
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordService _passwordService;
    private readonly IGoogleAuthenticationService _googleAuthenticationService;
    private readonly IDataProtector _externalLoginTicketProtector;
    private readonly IConfiguration _configuration;
    private readonly LinkGenerator _linkGenerator;

    public AuthenticationController(
        IUsersRepository usersRepository,
        IPasswordService passwordService,
        IGoogleAuthenticationService googleAuthenticationService,
        IDataProtectionProvider dataProtectionProvider,
        IConfiguration configuration,
        LinkGenerator linkGenerator)
    {
        _usersRepository = usersRepository;
        _passwordService = passwordService;
        _googleAuthenticationService = googleAuthenticationService;
        _externalLoginTicketProtector = dataProtectionProvider.CreateProtector(ExternalLoginTicketsConstants.DataProtectionPurpose);
        _configuration = configuration;
        _linkGenerator = linkGenerator;
    }

    [HttpPost("[action]")]
    [Consumes("application/x-www-form-urlencoded"), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserInfoResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<UserInfoResponse>> SignIn([FromForm] string usernameOrEmail, [FromForm] string password)
    {
        var user = await _usersRepository.GetByUsernameOrEmailNormalizedAsync(usernameOrEmail);
        if (user?.PasswordHash is null || !_passwordService.VerifyPassword(user.PasswordHash, password))
            return Unauthorized(new Problem("The specified credentials are invalid."));

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user.Id);
        return Ok(user.Adapt<UserInfoResponse>());
    }

    [HttpPost("[action]"), Authorize]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public new async Task<IActionResult> SignOut()
    {
        await HttpContext.SignOutAsync();
        return Ok();
    }

    [HttpGet("sso/google"), ProducesResponseType((int)HttpStatusCode.Redirect)]
    public IActionResult SsoGoogle(string returnUrl) => Redirect(_googleAuthenticationService.GetAuthorizationFlowUrl(returnUrl));

    [HttpGet("[action]")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserInfoResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Redirect)]
    [ProducesResponseType(typeof(ValidationProblem), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.ServiceUnavailable)]
    public async Task<ActionResult<UserInfoResponse>> SignInGoogleCallback([FromQuery] string code, [FromQuery(Name = "state")] Uri? returnUri = null)
    {
        var authenticationResult = await _googleAuthenticationService.AuthenticateUserByAuthorizationCodeAsync(code);
        switch (authenticationResult.Status)
        {
            case GoogleAuthenticationStatus.Success:
            {
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, authenticationResult.User!.Id);
                return returnUri is not null
                    ? Redirect(returnUri.ToString())
                    : Ok(authenticationResult.User.Adapt<UserInfoResponse>());
            }
            case GoogleAuthenticationStatus.NoUserAssociatedWithGoogleIdentity:
            {
                var ticket = new ExternalLoginTicket(GoogleAuthenticationConstants.ExternalLoginProviderId, authenticationResult.Identity!);
                var ticketJson = JsonSerializer.Serialize(ticket, GlobalSerializerOptions.SnakeCase);
                var encryptedTicketJson = _externalLoginTicketProtector.Protect(ticketJson);

                var ticketId = Guid.NewGuid().ToString().Split('-').First();
                var ticketCookieKey = string.Format(ExternalLoginTicketsConstants.CookieKeyTemplate, ticketId);
                Response.Cookies.Append(ticketCookieKey, encryptedTicketJson, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddHours(1),
                    Path = _linkGenerator.GetPathByAction("Register", "Users", Request.RouteValues)
                });

                var query = new QueryBuilder
                {
                    { "provider", GoogleAuthenticationConstants.ExternalLoginProviderId },
                    { "ticket", ticketId }
                };

                if (returnUri is not null)
                    query.Add("returnUrl", returnUri.ToString());

                var signUpPageUri = _configuration["SIGN_UP_PAGE_URI"]!;
                return Redirect($"{signUpPageUri}{query}");
            }
            case GoogleAuthenticationStatus.GoogleApiUnavailable:
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new Problem("Google API is unavailable at the moment."));
            case GoogleAuthenticationStatus.InvalidAuthorizationCode:
                return Unauthorized(new Problem("The provided Google OAuth authorization code is invalid."));
            default:
                return Unauthorized(new Problem("Google authentication service returned an unexpected authentication status."));
        }
    }
}