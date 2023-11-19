using CrowdParlay.Users.Api.Extensions;
using CrowdParlay.Users.Api.Middlewares;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Infrastructure.Persistence.Extensions;
using Serilog;

namespace CrowdParlay.Users.Api;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment) => app
        .UseMiddleware<ExceptionHandlingMiddleware>()
        .UseMiddleware<TraceIdMiddleware>()
        .UseSerilogRequestLogging()
        .UseHealthChecks("/healthz")
        .UseCors()
        .UseHttpsRedirection()
        .UseAuthentication()
        .UseRouting()
        .UseAuthorization()
        .UseEndpoints(builder => builder.MapControllers());

    public void ConfigureServices(IServiceCollection services) => services
        .ConfigureApplicationServices()
        .ConfigurePersistenceServices(_configuration)
        .ConfigureApiServices(_configuration, _environment);
}