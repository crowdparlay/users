using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Models;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using Dodo.Primitives;
using Microsoft.Extensions.Logging;
using static CrowdParlay.Users.Application.Services.GoogleAuthenticationStatus;

namespace CrowdParlay.Users.Application.Services;

public class GoogleAuthenticationService : IGoogleAuthenticationService
{
    private readonly IExternalLoginsRepository _externalLoginsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IGoogleOAuthService _googleOAuthService;
    private readonly ILogger<GoogleAuthenticationService> _logger;

    public GoogleAuthenticationService(
        IExternalLoginsRepository externalLoginsRepository,
        IUsersRepository usersRepository,
        IGoogleOAuthService googleOAuthService,
        ILogger<GoogleAuthenticationService> logger)
    {
        _externalLoginsRepository = externalLoginsRepository;
        _usersRepository = usersRepository;
        _googleOAuthService = googleOAuthService;
        _logger = logger;
    }

    public async Task<GoogleAuthenticationResult> AuthenticateUserByAuthorizationCodeAsync(string code, CancellationToken cancellationToken)
    {
        GoogleUserInfo googleUserInfo;

        try
        {
            var accessToken = await _googleOAuthService.GetAccessTokenAsync(code, new[] { "email", "profile" }, cancellationToken);
            if (accessToken is null)
                return new GoogleAuthenticationResult(InvalidAuthorizationCode);

            googleUserInfo = await _googleOAuthService.GetUserInfoAsync(accessToken, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogError(exception, "Network-related exception was thrown while trying to access Google OAuth API");
            return new GoogleAuthenticationResult(GoogleApiUnavailable);
        }

        var user = await _usersRepository.GetByExternalLoginAsync(
            GoogleAuthenticationDefaults.ExternalLoginProviderId,
            googleUserInfo.Email,
            cancellationToken);

        if (user is not null)
            return new GoogleAuthenticationResult(Success, googleUserInfo.Email, user);

        user = await _usersRepository.GetByEmailNormalizedAsync(googleUserInfo.Email, cancellationToken);
        if (user is null)
            return new GoogleAuthenticationResult(NoUserAssociatedWithGoogleIdentity, googleUserInfo.Email);

        var login = new ExternalLogin
        {
            Id = Uuid.NewTimeBased(),
            UserId = user.Id,
            ProviderId = GoogleAuthenticationDefaults.ExternalLoginProviderId,
            Identity = googleUserInfo.Email
        };

        await _externalLoginsRepository.AddAsync(login, cancellationToken);
        return new GoogleAuthenticationResult(Success, googleUserInfo.Email, user);
    }

    public string GetAuthorizationFlowUrl(string returnUrl) =>
        _googleOAuthService.GetAuthorizationFlowUrl(new[] { "email", "profile" }, returnUrl);
}

public class GoogleAuthenticationResult
{
    public readonly string? Identity;
    public readonly User? User;
    public readonly GoogleAuthenticationStatus Status;

    public GoogleAuthenticationResult(GoogleAuthenticationStatus status, string? identity = null, User? user = null)
    {
        if (status is not InvalidAuthorizationCode and not GoogleApiUnavailable && identity is null)
            throw new ArgumentNullException(nameof(identity));

        if (status is Success && user is null)
            throw new ArgumentNullException(nameof(user));

        if (status is not Success && user is not null)
            throw new ArgumentException("Google authentication failed, thus no user was expected to be authenticated.", nameof(user));

        Identity = identity;
        User = user;
        Status = status;
    }
}

public enum GoogleAuthenticationStatus
{
    Success,
    GoogleApiUnavailable,
    InvalidAuthorizationCode,
    NoUserAssociatedWithGoogleIdentity
}