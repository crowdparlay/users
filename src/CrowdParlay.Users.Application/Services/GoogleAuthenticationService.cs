using System.IdentityModel.Tokens.Jwt;
using CrowdParlay.Users.Application.Abstractions;
using CrowdParlay.Users.Application.Models;
using CrowdParlay.Users.Domain.Abstractions;
using CrowdParlay.Users.Domain.Entities;
using Dodo.Primitives;
using static CrowdParlay.Users.Application.Services.GoogleAuthenticationStatus;

namespace CrowdParlay.Users.Application.Services;

public class GoogleAuthenticationService : IGoogleAuthenticationService
{
    private const string GoogleExternalLoginProviderId = "google";

    private readonly IExternalLoginsRepository _externalLoginsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IGoogleOidcService _googleOidcService;

    public GoogleAuthenticationService(
        IExternalLoginsRepository externalLoginsRepository,
        IUsersRepository usersRepository,
        IGoogleOidcService googleOidcService)
    {
        _externalLoginsRepository = externalLoginsRepository;
        _usersRepository = usersRepository;
        _googleOidcService = googleOidcService;
    }

    public async Task<GoogleAuthenticationResult> AuthenticateUserByIdTokenAsync(JwtSecurityToken idToken)
    {
        GoogleUserInfo? googleUserInfo;
        
        try
        {
            googleUserInfo = await _googleOidcService.GetUserInfoByIdTokenAsync(idToken);
            if (googleUserInfo is null)
                return new GoogleAuthenticationResult(InvalidGoogleIdToken);
        }
        catch (HttpRequestException)
        {
            return new GoogleAuthenticationResult(GoogleApiUnavailable);
        }

        var user = await _usersRepository.GetByExternalLoginAsync(GoogleExternalLoginProviderId, googleUserInfo.Email);
        if (user is not null)
            return new GoogleAuthenticationResult(Success, user);

        user = await _usersRepository.GetByEmailNormalizedAsync(googleUserInfo.Email);
        if (user is null)
            return new GoogleAuthenticationResult(NoUserAssociatedWithGoogleIdentity);

        var login = new ExternalLogin
        {
            Id = Uuid.NewTimeBased(),
            UserId = user.Id,
            ProviderId = GoogleExternalLoginProviderId,
            Identity = googleUserInfo.Email
        };

        await _externalLoginsRepository.AddAsync(login);
        return new GoogleAuthenticationResult(Success, user);
    }
}

public class GoogleAuthenticationResult
{
    public readonly User? User;
    public readonly GoogleAuthenticationStatus Status;

    public GoogleAuthenticationResult(GoogleAuthenticationStatus status, User? user = null)
    {
        if (status is Success && user is null)
            throw new ArgumentNullException(nameof(user));

        if (status is not Success && user is not null)
            throw new ArgumentException("Google authentication failed, thus no user is expected to be authenticated.", nameof(user));

        User = user;
        Status = status;
    }
}

public enum GoogleAuthenticationStatus
{
    Success,
    GoogleApiUnavailable,
    InvalidGoogleIdToken,
    NoUserAssociatedWithGoogleIdentity
}