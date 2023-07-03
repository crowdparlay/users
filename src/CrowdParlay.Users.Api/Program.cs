using CrowdParlay.Communication.RabbitMq.DependencyInjection;
using CrowdParlay.Users.Api.Extensions;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Infrastructure.Persistence.Extensions;

namespace CrowdParlay.Users.Api;

public class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    private static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>());
}

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

    public void ConfigureServices(IServiceCollection services)
    {
        var rabbitMqAmqpServerUrl =
            _configuration["RABBITMQ_AMQP_SERVER_URL"]
            ?? throw new InvalidOperationException("Missing required configuration 'RABBITMQ_AMQP_SERVER_URL'.");

        services
            .ConfigureApplicationServices()
            .ConfigurePersistenceServices(_configuration)
            .ConfigureApiServices(_configuration, _environment)
            .AddRabbitMqCommunication(options => options
                .UseAmqpServer(rabbitMqAmqpServerUrl));

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
}