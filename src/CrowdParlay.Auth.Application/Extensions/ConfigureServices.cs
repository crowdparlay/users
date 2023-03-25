using System.Reflection;
using CrowdParlay.Auth.Application.Behaviors;
using FluentValidation;
using Mapster;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace CrowdParlay.Auth.Application.Extensions;

public static class ConfigureServices
{
    public static IServiceCollection ConfigureApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
        typeAdapterConfig.Scan(assembly);
        
        return services
            .AddValidatorsFromAssembly(assembly)
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
    }
}