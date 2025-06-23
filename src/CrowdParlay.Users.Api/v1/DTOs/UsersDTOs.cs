using Dodo.Primitives;

namespace CrowdParlay.Users.Api.v1.DTOs;

public sealed record UserInfoResponse(
    Uuid Id,
    string Username,
    string DisplayName,
    string? AvatarUrl);

public sealed record UsersRegisterRequest(
    string Username,
    string DisplayName,
    string? Email,
    string? Password,
    string? AvatarUrl,
    string? ExternalLoginTicketId);

public sealed record UsersUpdateRequest(
    string? Username,
    string? DisplayName,
    string? Email,
    string? AvatarUrl,
    string? OldPassword,
    string? NewPassword);