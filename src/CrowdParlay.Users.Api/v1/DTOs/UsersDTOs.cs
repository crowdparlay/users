namespace CrowdParlay.Users.Api.v1.DTOs;

public record UsersRegisterRequest(
    string Username,
    string DisplayName,
    string Email,
    string Password,
    string? AvatarUrl);

public record UsersUpdateRequest(
    string? Username,
    string? DisplayName,
    string? Email,
    string? AvatarUrl,
    string? OldPassword,
    string? NewPassword);