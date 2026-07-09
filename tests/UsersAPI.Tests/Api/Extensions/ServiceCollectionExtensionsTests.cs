using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using UsersAPI.Api.Common;
using UsersAPI.Api.Extensions;
using UsersAPI.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace UsersAPI.Tests.Api.Extensions;

[Trait("Category", "Unit")]
public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public async Task AddApiPresentation_DeveConfigurarJwtBearerComoEsquemaPadrao()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddApiPresentation(configuration);

        // Assert
        await using var serviceProvider = services.BuildServiceProvider();
        var schemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        var authenticateScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();
        var challengeScheme = await schemeProvider.GetDefaultChallengeSchemeAsync();

        authenticateScheme.ShouldNotBeNull();
        challengeScheme.ShouldNotBeNull();
        authenticateScheme!.Name.ShouldBe(JwtBearerDefaults.AuthenticationScheme);
        challengeScheme!.Name.ShouldBe(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public void AddApiPresentation_DeveConfigurarValidacaoJwtParaRoles()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        // Act
        services.AddApiPresentation(configuration);

        // Assert
        using var serviceProvider = services.BuildServiceProvider();
        var jwtBearerOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        jwtBearerOptions.MapInboundClaims.ShouldBeFalse();
        jwtBearerOptions.TokenValidationParameters.ValidateIssuer.ShouldBeTrue();
        jwtBearerOptions.TokenValidationParameters.ValidateAudience.ShouldBeTrue();
        jwtBearerOptions.TokenValidationParameters.ValidateIssuerSigningKey.ShouldBeTrue();
        jwtBearerOptions.TokenValidationParameters.ValidateLifetime.ShouldBeTrue();
        jwtBearerOptions.TokenValidationParameters.NameClaimType.ShouldBe(JwtRegisteredClaimNames.Sub);
        jwtBearerOptions.TokenValidationParameters.RoleClaimType.ShouldBe(JwtClaimNames.Role);
        jwtBearerOptions.TokenValidationParameters.ClockSkew.ShouldBe(TimeSpan.Zero);
    }

    [Fact]
    public async Task AddApiPresentation_QuandoJwtChallenge_DeveRetornarProblemDetailsUnauthorized()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddApiPresentation(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var jwtBearerOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = CreateHttpContext("/api/games");
        var authenticationScheme = CreateAuthenticationScheme();
        var context = new JwtBearerChallengeContext(
            httpContext,
            authenticationScheme,
            jwtBearerOptions,
            new AuthenticationProperties());

        // Act
        await jwtBearerOptions.Events.OnChallenge(context);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);
        httpContext.Response.ContentType.ShouldStartWith("application/problem+json");
        problemDetails.Status.ShouldBe(StatusCodes.Status401Unauthorized);
        problemDetails.Title.ShouldBe(ApiMessages.Unauthorized.Title);
        problemDetails.Detail.ShouldBe(ApiMessages.Unauthorized.Detail);
        problemDetails.Instance.ShouldBe("/api/games");
    }

    [Fact]
    public async Task AddApiPresentation_QuandoJwtForbidden_DeveRetornarProblemDetailsForbidden()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration();

        services.AddApiPresentation(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var jwtBearerOptions = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        var httpContext = CreateHttpContext("/api/games");
        var authenticationScheme = CreateAuthenticationScheme();
        var context = new ForbiddenContext(
            httpContext,
            authenticationScheme,
            jwtBearerOptions);

        // Act
        await jwtBearerOptions.Events.OnForbidden(context);

        // Assert
        var problemDetails = await ReadProblemDetailsAsync(httpContext);

        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status403Forbidden);
        httpContext.Response.ContentType.ShouldStartWith("application/problem+json");
        problemDetails.Status.ShouldBe(StatusCodes.Status403Forbidden);
        problemDetails.Title.ShouldBe(ApiMessages.Forbidden.Title);
        problemDetails.Detail.ShouldBe(ApiMessages.Forbidden.Detail);
        problemDetails.Instance.ShouldBe("/api/games");
    }

    private static IConfiguration CreateConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "FiapCloudGames",
                ["Jwt:Audience"] = "FiapCloudGames",
                ["Jwt:Secret"] = "maicon-guedes-dotnet-architect-level-99-cloud-games-key",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();
    }

    private static DefaultHttpContext CreateHttpContext(string path)
    {
        return new DefaultHttpContext
        {
            Request =
            {
                Path = path
            },
            Response =
            {
                Body = new MemoryStream()
            }
        };
    }

    private static AuthenticationScheme CreateAuthenticationScheme()
    {
        return new AuthenticationScheme(
            JwtBearerDefaults.AuthenticationScheme,
            JwtBearerDefaults.AuthenticationScheme,
            typeof(JwtBearerHandler));
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

