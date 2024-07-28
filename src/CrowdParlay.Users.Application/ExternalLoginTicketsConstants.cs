using System.Text.RegularExpressions;

namespace CrowdParlay.Users.Application;

public static partial class ExternalLoginTicketsConstants
{
    public const string DataProtectionPurpose = "ExternalLoginTickets";
    public const string CookieKeyTemplate = ".CrowdParlay.ExternalLoginTickets.{0}";
    public static readonly Regex CookieKeyRegex = GeneratedCookieKeyRegex();

    [GeneratedRegex(@"\.CrowdParlay\.ExternalLoginTickets\..+", RegexOptions.Compiled)]
    private static partial Regex GeneratedCookieKeyRegex();
}