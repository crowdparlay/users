using CrowdParlay.Users.Api.Extensions;
using CrowdParlay.Users.Application.Extensions;
using CrowdParlay.Users.Infrastructure.Persistence.Extensions;
using CrowdParlay.Users.Infrastructure.Communication.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .ConfigureApplicationServices()
    .ConfigurePersistenceServices()
    .ConfigureCommunicationServices(builder.Configuration)
    .ConfigureApiServices(builder.Configuration, builder.Environment);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog();

var app = builder.Build();

app.UseHealthChecks("/health");

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();