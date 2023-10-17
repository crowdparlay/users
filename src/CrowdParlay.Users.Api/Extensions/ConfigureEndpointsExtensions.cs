using CrowdParlay.Users.Api.Services;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CrowdParlay.Users.Api.Extensions;

public static class ConfigureEndpointsExtensions
{
    public static IServiceCollection ConfigureEndpoints(this IServiceCollection services)
    {
        var mvcBuilder = services.AddControllers(options =>
        {
            var transformer = new KebabCaseParameterPolicy();
            options.Conventions.Add(new RouteTokenTransformerConvention(transformer));
        });

        mvcBuilder.AddJsonOptions(options =>
            options.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy());

        services.AddApiVersioning(options => options.ReportApiVersions = true);

        return services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }
}