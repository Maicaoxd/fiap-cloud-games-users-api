using UsersAPI.Api.Extensions;
using UsersAPI.Application;
using UsersAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiPresentation(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseApiPresentation();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "UsersAPI" }))
    .WithName("HealthCheck");

app.Run();
