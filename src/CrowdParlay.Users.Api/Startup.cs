using CrowdParlay.Users.Api.Extensions;
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
        app.UseSerilogRequestLogging();
        app.UseHealthChecks("/health");

        app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());

        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRouting();
        app.UseEndpoints(builder => builder.MapControllers());
    }

    public void ConfigureServices(IServiceCollection services) => services
        .ConfigureApplicationServices()
        .ConfigurePersistenceServices(_configuration)
        .ConfigureApiServices(_configuration, _environment);
}