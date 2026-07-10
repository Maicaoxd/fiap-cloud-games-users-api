using UsersAPI.Api.Extensions;
using UsersAPI.Application;
using UsersAPI.Health;
using UsersAPI.Infrastructure;
using UsersAPI.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiPresentation(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
{
    await app.ApplyDatabaseMigrationsAsync();
    return;
}

app.UseApiPresentation();

app.MapGet("/health/live", () => Results.Ok(new
{
    status = "Healthy",
    service = "UsersAPI"
}))
.WithName("LiveHealthCheck");

app.MapGet("/health", CheckReadinessAsync)
    .WithName("HealthCheck");

app.MapGet("/health/ready", CheckReadinessAsync)
    .WithName("ReadyHealthCheck");

app.Run();

static async Task<IResult> CheckReadinessAsync(
    IDatabaseHealthChecker databaseHealthChecker,
    IRabbitMqConnectionChecker rabbitMqConnectionChecker,
    CancellationToken cancellationToken)
{
    var databaseStatus = "Unhealthy";
    var rabbitMqStatus = "Unhealthy";
    string? databaseError = null;

    try
    {
        databaseStatus = await databaseHealthChecker.CanConnectAsync(cancellationToken)
            ? "Healthy"
            : "Unhealthy";
    }
    catch (Exception exception)
    {
        databaseError = exception.Message;
    }

    rabbitMqStatus = await rabbitMqConnectionChecker.CanConnectAsync(cancellationToken)
        ? "Healthy"
        : "Unhealthy";

    var isHealthy = databaseStatus == "Healthy" && rabbitMqStatus == "Healthy";
    var response = new
    {
        status = isHealthy ? "Healthy" : "Unhealthy",
        service = "UsersAPI",
        checks = new
        {
            database = databaseStatus,
            rabbitMq = rabbitMqStatus
        },
        error = databaseError
    };

    return isHealthy
        ? Results.Ok(response)
        : Results.Json(response, statusCode: StatusCodes.Status503ServiceUnavailable);
}
