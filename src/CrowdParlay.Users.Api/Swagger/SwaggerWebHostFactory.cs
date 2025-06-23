using System.Reflection;
using CrowdParlay.Users.Api.Extensions;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CrowdParlay.Users.Api.Swagger;

public class SwaggerWebHostFactory
{
    ///<summary>
    /// Ignored parts of namespaces, generally CQRS-conventional names,
    /// such as 'Queries' and 'Commands'. These are skipped when generating
    /// Swagger names for the public DTOs.
    /// </summary>
    private static readonly IEnumerable<string> IgnoredNamespaceIdentifiers = new[]
    {
        "Commands",
        "Queries"
    };

    public static IWebHost CreateWebHost() => WebHost.CreateDefaultBuilder()
        .Configure(builder => builder.New())
        .ConfigureServices(services =>
        {
            services.ConfigureEndpoints();
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options =>
            {
                var xmlDocsFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlDocsFileName));

                options.SupportNonNullableReferenceTypes();
                options.MapType<Guid>(() => new OpenApiSchema
                {
                    Type = "string",
                    Format = "uuid"
                });

                options.CustomSchemaIds(type =>
                {
                    if (!type.Namespace!.StartsWith("CrowdParlay.Users.Application.Features"))
                        return type.Name;

                    // Generates unique and user-friendly names for CQRS entities.
                    // For example, 'Features.Accounts.Commands.Create.Command' gets turned into 'AccountsCreateCommand'.
                    var lastNames = type.FullName!.Split('.', '+')
                        .Except(IgnoredNamespaceIdentifiers)
                        .TakeLast(3);

                    return string.Join(string.Empty, lastNames);
                });
            });
        })
        .Build();
}