using CrowdParlay.Users.Api.Extensions;
using Dodo.Primitives;
using Microsoft.AspNetCore;
using Microsoft.OpenApi.Models;

namespace CrowdParlay.Users.Api;

public class SwaggerWebHostFactory
{
    private const string SwaggerVersion = "v1";

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
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(SwaggerVersion, new OpenApiInfo
                {
                    Title = "Crowd Parlay Users API",
                    Version = SwaggerVersion
                });

                options.SupportNonNullableReferenceTypes();

                options.MapType<Uuid>(() => new OpenApiSchema
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
                        .Where(identifier => !IgnoredNamespaceIdentifiers.Contains(identifier))
                        .TakeLast(3);

                    return string.Join(string.Empty, lastNames);
                });
            });
        })
        .Build();
}