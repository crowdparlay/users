using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CrowdParlay.Users.Api.Swagger;

/// <summary>
/// Configures the Swagger generation options.
/// </summary>
/// <remarks>This allows API versioning to define a Swagger document per API version after the
/// <see cref="IApiVersionDescriptionProvider"/> service has been resolved from the service container.</remarks>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

    /// <inheritdoc />
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        
        options.SupportNonNullableReferenceTypes();
        options.UseAllOfToExtendReferenceSchemas();
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription versionDescription)
    {
        var info = new OpenApiInfo()
        {
            Title = "Crowd Parlay Users API",
            Description = "RESTful API of the Crowd Parlay's Users service.",
            Version = versionDescription.ApiVersion.ToString()
        };

        if (versionDescription.IsDeprecated)
            info.Description += " This API version has been deprecated.";

        return info;
    }
}