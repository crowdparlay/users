using Dodo.Primitives;
using Microsoft.OpenApi.Models;

namespace CrowdParlay.Users.Api.Extensions;

public static class ConfigureSwaggerExtensions
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

    public static IServiceCollection ConfigureSwagger(
        this IServiceCollection services, IConfiguration configuration) => services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Crowd Parlay Users API", Version = "v1" });

        options.SupportNonNullableReferenceTypes();

        options.MapType<Uuid>(() => new OpenApiSchema
        {
            Format = "uuid",
            Type = "string",
            Pattern = "[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"
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
}