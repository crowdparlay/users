using System.Net;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

namespace CrowdParlay.Users.Api;

public class Program
{
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args).UseSerilog();
        return builder.ConfigureWebHostDefaults(webHostBuilder =>
        {
            webHostBuilder.UseStartup<Startup>();
            webHostBuilder.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Any, 8080, listenOptions => listenOptions.Protocols = HttpProtocols.Http1AndHttp2);
                options.Listen(IPAddress.Any, 8443, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
            });
        });
    }
}