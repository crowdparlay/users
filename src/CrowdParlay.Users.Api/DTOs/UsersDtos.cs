namespace CrowdParlay.Users.Api.DTOs;

public record UsersRegisterDto(
    string Username,
    string DisplayName,
    string Password,
    string? AvatarUrl);

public record UsersUpdateDto(
    string? Username,
    string? DisplayName,
    string? AvatarUrl,
    string? OldPassword,
    string? NewPassword);