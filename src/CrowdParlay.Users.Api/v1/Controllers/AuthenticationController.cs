using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mime;
using System.Security.Claims;
using CrowdParlay.Users.Api.v1.DTOs;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Application.Features.Users.Commands;
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
    private readonly JwtSecurityTokenHandler _jwtHandler = new();

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
    [Consumes("x-www-form-urlencoded"), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    public async Task<ActionResult<UserInfoResponse>> SignIn([FromForm] string usernameOrEmail, [FromForm] string password)
    {
        var user = await _usersRepository.GetByUsernameOrEmailNormalizedAsync(usernameOrEmail);
        if (user is null || !_passwordService.VerifyPassword(user.PasswordHash, password))
            return Unauthorized("The username or password is incorrect.");

        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity.AddUserClaims(user));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Ok();
    }

    [HttpPost("[action]"), Authorize]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    public new async Task<IActionResult> SignOut()
    {
        await HttpContext.SignOutAsync();
        return Ok();
    }

    [HttpPost("[action]")]
    [Consumes("x-www-form-urlencoded"), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(UserInfoResponse), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.InternalServerError)]
    [ProducesResponseType(typeof(Problem), (int)HttpStatusCode.ServiceUnavailable)]
    public async Task<ActionResult<UserInfoResponse>> SignInGoogleCallback([FromForm] string credential)
    {
        var googleIdToken = _jwtHandler.ReadJwtToken(credential);
        var authenticationResult = await _googleAuthenticationService.AuthenticateUserByIdTokenAsync(googleIdToken);

        switch (authenticationResult.Status)
        {
            case GoogleAuthenticationStatus.Success:
            {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity.AddUserClaims(authenticationResult.User!));
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                
                return Ok(authenticationResult.User.Adapt<UserInfoResponse>());
            }
            case GoogleAuthenticationStatus.GoogleApiUnavailable:
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Google API is unavailable at the moment. Please try again later.");
            case GoogleAuthenticationStatus.InvalidGoogleIdToken:
                return Unauthorized("The provided Google ID token is invalid.");
            case GoogleAuthenticationStatus.NoUserAssociatedWithGoogleIdentity:
                return Unauthorized("There is no user associated with the provided Google identity.");
            default:
                return Unauthorized("Google authentication service returned an unexpected authentication status.");
        }
    }
}