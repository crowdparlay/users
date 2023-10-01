namespace CrowdParlay.Users.Api.v1.DTOs;

public record UsersRegisterRequest(
    string Username,
    string DisplayName,
    string Password,
    string? AvatarUrl);

public record UsersUpdateRequest(
    string? Username,
    string? DisplayName,
    string? AvatarUrl,
    string? OldPassword,
    string? NewPassword);