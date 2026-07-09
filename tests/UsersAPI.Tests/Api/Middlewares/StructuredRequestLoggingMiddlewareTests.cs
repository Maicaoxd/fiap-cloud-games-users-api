using UsersAPI.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UsersAPI.Tests.Api.Middlewares;

[Trait("Category", "Unit")]
public sealed class StructuredRequestLoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_QuandoRequestForProcessado_DeveRegistrarLogEstruturado()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var logger = new TestLogger<StructuredRequestLoggingMiddleware>();
        var middleware = new StructuredRequestLoggingMiddleware(
            context =>
            {
                context.Response.StatusCode = StatusCodes.Status201Created;

                return Task.CompletedTask;
            },
            logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var logEntry = logger.LogEntries.Single();

        logEntry.LogLevel.ShouldBe(LogLevel.Information);
        logEntry.Properties["Method"].ShouldBe("POST");
        logEntry.Properties["Path"].ShouldBe("/api/users");
        logEntry.Properties["StatusCode"].ShouldBe(StatusCodes.Status201Created);
        logEntry.Properties["TraceId"].ShouldBe(httpContext.TraceIdentifier);
        logEntry.Properties.ContainsKey("ElapsedMilliseconds").ShouldBeTrue();
    }

    [Fact]
    public async Task InvokeAsync_QuandoResponseForErroServidor_DeveRegistrarWarning()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var logger = new TestLogger<StructuredRequestLoggingMiddleware>();
        var middleware = new StructuredRequestLoggingMiddleware(
            context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                return Task.CompletedTask;
            },
            logger);

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var logEntry = logger.LogEntries.Single();

        logEntry.LogLevel.ShouldBe(LogLevel.Warning);
        logEntry.Properties["StatusCode"].ShouldBe(StatusCodes.Status500InternalServerError);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            TraceIdentifier = "trace-id",
            Request =
            {
                Method = HttpMethods.Post,
                Path = "/api/users"
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public List<LogEntry> LogEntries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var properties = state as IEnumerable<KeyValuePair<string, object?>>
                ?? [];

            LogEntries.Add(new LogEntry(
                logLevel,
                properties.ToDictionary(
                    property => property.Key,
                    property => property.Value)));
        }
    }

    private sealed record LogEntry(
        LogLevel LogLevel,
        IReadOnlyDictionary<string, object?> Properties);
}

