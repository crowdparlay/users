using CrowdParlay.Users.Api.Extensions;
using CrowdParlay.Users.Api.Middlewares;
using CrowdParlay.Users.Api.Services.gRPC;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Infrastructure.Extensions;
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

        app.UseRouting();
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHealthChecks("/healthz");
        app.UseEndpoints(builder =>
        {
            builder.MapControllers();
            builder.MapGrpcService<UsersGrpcService>();
        });
    }

    public void ConfigureServices(IServiceCollection services) => services
        .ConfigureApplicationServices()
        .ConfigureInfrastructureServices(_configuration)
        .ConfigureApiServices(_configuration, _environment);
}