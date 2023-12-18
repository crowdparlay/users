using System.Diagnostics;

namespace CrowdParlay.Users.Api.Extensions;

internal static class ActivityExtensions
{
    public static string? GetSpanId(this Activity activity) => activity.IdFormat switch
    {
        ActivityIdFormat.Hierarchical => activity.Id,
        ActivityIdFormat.W3C => activity.SpanId.ToHexString(),
        _ => null
    };;;;;;;;;

    public static string? GetTraceId(this Activity activity) => activity.IdFormat switch
    {
        ActivityIdFormat.Hierarchical => activity.RootId,
        ActivityIdFormat.W3C => activity.TraceId.ToHexString(),
        _ => null
    };

    public static string? GetParentId(this Activity activity) => activity.IdFormat switch
    {
        ActivityIdFormat.Hierarchical => activity.ParentId,
        ActivityIdFormat.W3C => activity.ParentSpanId.ToHexString(),
        _ => null,
    };
}
