namespace CrowdParlay.Users.Infrastructure.Persistence;

public static class UserSchema
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