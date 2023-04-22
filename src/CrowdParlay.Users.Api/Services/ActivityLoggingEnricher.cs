using System.Diagnostics;
using CrowdParlay.Users.Api.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace CrowdParlay.Users.Api.Services;

/// <summary>
/// Serilog enricher that makes it possible to inject span, trace and parent IDs into the logger template.
/// https://github.com/serilog/serilog/wiki/Enrichment
/// </summary>
public class ActivityLoggingEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var activity = Activity.Current;

        logEvent.AddPropertyIfAbsent(new LogEventProperty("SpanIdOptional", AsValue("SPAN", activity?.GetSpanId())));
        logEvent.AddPropertyIfAbsent(new LogEventProperty("TraceIdOptional", AsValue("TRACE", activity?.GetTraceId())));
        logEvent.AddPropertyIfAbsent(new LogEventProperty("ParentIdOptional", AsValue("PARENT", activity?.GetParentId())));

        // If value exists, then return ' PREFIX:VALUE '
        // Otherwise, return ' '
        LogEventPropertyValue AsValue(string prefix, string? value) => new ScalarValue(
            value is not null
                ? $" {prefix}:{activity?.GetSpanId()} "
                : " ");
    }
}