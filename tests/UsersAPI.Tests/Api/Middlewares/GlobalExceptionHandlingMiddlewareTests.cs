using System.Text.Json;
using UsersAPI.Api.Common;
using UsersAPI.Api.Middlewares;
using UsersAPI.Application.Common;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Domain.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace UsersAPI.Tests.Api.Middlewares;

[Trait("Category", "Unit")]
public sealed class GlobalExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_QuandoOcorrerArgumentException_DeveRetornarBadRequest()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var middleware = CreateMiddleware(_ =>
            throw new ArgumentException(DomainMessages.Email.InvalidFormat));

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        httpContext.Response.ContentType.ShouldStartWith("application/problem+json");
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Title.ShouldBe(ApiMessages.Validation.Title);
        problemDetails.Detail.ShouldBe(DomainMessages.Email.InvalidFormat);
    }

    [Fact]
    public async Task InvokeAsync_QuandoOcorrerBadHttpRequestException_DeveRetornarBadRequest()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var middleware = CreateMiddleware(_ =>
            throw new BadHttpRequestException("Required request body is missing."));

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Title.ShouldBe(ApiMessages.Validation.Title);
        problemDetails.Detail.ShouldBe(ApiMessages.Validation.RequestBodyRequired);
    }

    [Fact]
    public async Task InvokeAsync_QuandoOcorrerEmailAlreadyRegisteredException_DeveRetornarConflict()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var middleware = CreateMiddleware(_ =>
            throw new EmailAlreadyRegisteredException());

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
        problemDetails.Status.ShouldBe(StatusCodes.Status409Conflict);
        problemDetails.Title.ShouldBe(ApiMessages.Conflict.Title);
        problemDetails.Detail.ShouldBe(ApplicationMessages.User.EmailAlreadyRegistered);
    }

    [Fact]
    public async Task InvokeAsync_QuandoOcorrerCpfAlreadyRegisteredException_DeveRetornarConflict()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var middleware = CreateMiddleware(_ =>
            throw new CpfAlreadyRegisteredException());

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
        problemDetails.Status.ShouldBe(StatusCodes.Status409Conflict);
        problemDetails.Title.ShouldBe(ApiMessages.Conflict.Title);
        problemDetails.Detail.ShouldBe(ApplicationMessages.User.CpfAlreadyRegistered);
    }

    [Fact]
    public async Task InvokeAsync_QuandoOcorrerUserNotFoundException_DeveRetornarNotFound()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var middleware = CreateMiddleware(_ =>
            throw new UserNotFoundException());

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
        problemDetails.Title.ShouldBe(ApiMessages.NotFound.Title);
        problemDetails.Detail.ShouldBe(ApplicationMessages.User.NotFound);
    }

    [Fact]
    public async Task InvokeAsync_QuandoOcorrerInvalidPasswordRecoveryDataException_DeveRetornarBadRequest()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var middleware = CreateMiddleware(_ =>
            throw new InvalidPasswordRecoveryDataException());

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Title.ShouldBe(ApiMessages.Validation.Title);
        problemDetails.Detail.ShouldBe(ApplicationMessages.PasswordRecovery.InvalidRecoveryData);
    }

    [Fact]
    public async Task InvokeAsync_QuandoOcorrerInvalidCredentialsException_DeveRetornarUnauthorized()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var middleware = CreateMiddleware(_ =>
            throw new InvalidCredentialsException());

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        problemDetails.Status.ShouldBe(StatusCodes.Status401Unauthorized);
        problemDetails.Title.ShouldBe(ApiMessages.Unauthorized.Title);
        problemDetails.Detail.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
    }

    [Fact]
    public async Task InvokeAsync_QuandoOcorrerInactiveUserException_DeveRetornarForbidden()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var middleware = CreateMiddleware(_ =>
            throw new InactiveUserException());

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status403Forbidden);
        problemDetails.Status.ShouldBe(StatusCodes.Status403Forbidden);
        problemDetails.Title.ShouldBe(ApiMessages.Forbidden.Title);
        problemDetails.Detail.ShouldBe(ApplicationMessages.Authentication.InactiveUser);
    }

    [Fact]
    public async Task InvokeAsync_QuandoOcorrerDbUpdateExceptionDeUnicidade_DeveRetornarConflict()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var middleware = CreateMiddleware(_ =>
            throw new DbUpdateException(
                "Falha ao persistir.",
                new Exception("Violation of UNIQUE KEY constraint 'IX_Users_Email'.")));

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status409Conflict);
        problemDetails.Status.ShouldBe(StatusCodes.Status409Conflict);
        problemDetails.Title.ShouldBe(ApiMessages.Conflict.Title);
        problemDetails.Detail.ShouldBe(ApplicationMessages.Conflict.UniqueConstraintViolation);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            },
            Request =
            {
                Path = "/api/users"
            }
        };
    }

    private static GlobalExceptionHandlingMiddleware CreateMiddleware(RequestDelegate next)
    {
        var logger = Substitute.For<ILogger<GlobalExceptionHandlingMiddleware>>();

        return new GlobalExceptionHandlingMiddleware(next, logger);
    }

    private static async Task<ProblemDetails> ReadProblemDetailsAsync(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;

        var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            httpContext.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return problemDetails!;
    }
}


