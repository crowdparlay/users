using CrowdParlay.Users.Api.Extensions;
using CrowdParlay.Users.Api.Routing;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace CrowdParlay.Users.Api;

public class SwaggerWebHostFactory
{
    private const string SwaggerVersion = "v1";

    public static IWebHost CreateWebHost() => WebHost.CreateDefaultBuilder()
        .Configure(builder => builder.New())
        .ConfigureServices((context, services) =>
        {
            services.ConfigureSwagger(context.Configuration);
            services.AddEndpointsApiExplorer();
            services.AddControllers(options =>
            {
                var transformer = new KebabCaseParameterPolicy();
                options.Conventions.Add(new RouteTokenTransformerConvention(transformer));
            });
        })
        .Build();
}