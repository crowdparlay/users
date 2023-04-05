using System.Net;
using CrowdParlay.Auth.Api.Extensions;
using CrowdParlay.Auth.Application.Extensions;
using CrowdParlay.Auth.Infrastructure.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .ConfigureApplicationServices()
    .ConfigurePersistenceServices()
    .ConfigureApiServices(builder.Configuration, builder.Environment);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog();

var app = builder.Build();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
    context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
    context.Response.Headers.Add("Access-Control-Allow-Headers",
        "Content-Type, X-CSRF-Token, X-Requested-With, Accept, Accept-Version, Accept-Encoding, Content-Length, Content-MD5, Date, X-Api-Version, X-File-Name");
    context.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,PUT,PATCH,DELETE,OPTIONS");

    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        await context.Response.WriteAsync(string.Empty);
    }

    await next(context);
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHealthChecks("/health");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();