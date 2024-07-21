using System.Net;
using System.Net.Mime;
using System.Security.Claims;
using CrowdParlay.Users.Api.v1.DTOs;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Application.Services;
using CrowdParlay.Users.Domain.Abstractions;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Users.Api.v1.Controllers;

[ApiVersion("1.0")]
public class AuthenticationController : ApiControllerBase
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordService _passwordService;
    private readonly IGoogleAuthenticationService _googleAuthenticationService;

    public AuthenticationController(
        IUsersRepository usersRepository,
        IPasswordService passwordService,
        IGoogleAuthenticationService googleAuthenticationService)
    {
        _usersRepository = usersRepository;
        _passwordService = passwordService;
        _googleAuthenticationService = googleAuthenticationService;
    }

    [HttpPost("[action]")]
    [Consumes("application/x-www-form-urlencoded"), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserInfoResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<UserInfoResponse>> SignIn([FromForm] string usernameOrEmail, [FromForm] string password)
    {
        var user = await _usersRepository.GetByUsernameOrEmailNormalizedAsync(usernameOrEmail);
        if (user is null || !_passwordService.VerifyPassword(user.PasswordHash, password))
            return Unauthorized(new Problem("The specified credentials are invalid."));

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity.AddUserClaims(user));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

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
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity.AddUserClaims(authenticationResult.User!));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return returnUri is not null
                    ? Redirect(returnUri.ToString())
                    : Ok(authenticationResult.User.Adapt<UserInfoResponse>());
            }
            case GoogleAuthenticationStatus.GoogleApiUnavailable:
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, new Problem("Google API is unavailable at the moment."));
            case GoogleAuthenticationStatus.InvalidAuthorizationCode:
                return Unauthorized(new Problem("The provided Google OAuth authorization code is invalid."));
            case GoogleAuthenticationStatus.NoUserAssociatedWithGoogleIdentity:
                return Unauthorized(new Problem("There is no user associated with the provided Google identity."));
            default:
                return Unauthorized(new Problem("Google authentication service returned an unexpected authentication status."));
        }
    }
}