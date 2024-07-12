namespace CrowdParlay.Users.Infrastructure.Persistence;

public static class UsersSchema
{
    public const string Table = "users";
    public const string Id = "id";
    public const string Username = "username";
    public const string DisplayName = "display_name";
    public const string Email = "email";
    public const string AvatarUrl = "avatar_url";
    public const string PasswordHash = "password_hash";
    public const string CreatedAt = "created_at";
    public const string UsernameNormalized = "username_normalized";
    public const string EmailNormalized = "email_normalized";
}

public static class ExternalLoginProvidersSchema
{
    public const string Table = "external_login_providers";
    public const string Id = "id";
    public const string DisplayName = "display_name";
    public const string LogoUrl = "logo_url";
}

public static class ExternalLoginsSchema
{
    public const string Table = "external_logins";
    public const string Id = "id";
    public const string UserId = "user_id";
    public const string ProviderId = "provider_id";
    public const string Identity = "identity";
}