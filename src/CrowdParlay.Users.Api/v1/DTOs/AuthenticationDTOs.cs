using Microsoft.AspNetCore.Mvc;

namespace CrowdParlay.Users.Api.v1.DTOs;

public record OAuth2ExchangeRequest(
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