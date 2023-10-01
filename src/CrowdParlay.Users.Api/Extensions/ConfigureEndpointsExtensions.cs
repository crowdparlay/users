using CrowdParlay.Users.Api.Routing;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CrowdParlay.Users.Api.Extensions;

public static class ConfigureEndpointsExtensions
{
    public static IServiceCollection ConfigureEndpoints(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            var transformer = new KebabCaseParameterPolicy();
            options.Conventions.Add(new RouteTokenTransformerConvention(transformer));
        });

        services.AddApiVersioning(options => options.ReportApiVersions = true);

        return services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }
}