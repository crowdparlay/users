using System.Diagnostics;
using CrowdParlay.Users.Api.Extensions;

namespace CrowdParlay.Users.Api.Middlewares;

public class TraceIdMiddleware
{
    private readonly RequestDelegate _next;

    public TraceIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = Activity.Current?.GetTraceId() ?? context.TraceIdentifier;
        context.Response.Headers.Add("X-TraceId", traceId);
        
        await _next(context);
    }
}