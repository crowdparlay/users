using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Users.Api.DTOs;

public record OAuth2ExchangeDto(
    [property: ModelBinder(Name = "grant_type")]
    string GrantType,
    [property: ModelBinder(Name = "scope")]
    string Scope,
    [property: ModelBinder(Name = "username")]
    string? Username,
    [property: ModelBinder(Name = "password")]
    string? Password,
    [property: ModelBinder(Name = "refresh_token")]
    string? RefreshToken);