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

    public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<TraceIdMiddleware>();
        app.UseSerilogRequestLogging();
        app.UseHealthChecks("/healthz");

        app.UseCors(builder => builder
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(builder => builder.MapControllers());
    }

    public void ConfigureServices(IServiceCollection services) => services
        .ConfigureApplicationServices()
        .ConfigurePersistenceServices(_configuration)
        .ConfigureApiServices(_configuration, _environment);
}