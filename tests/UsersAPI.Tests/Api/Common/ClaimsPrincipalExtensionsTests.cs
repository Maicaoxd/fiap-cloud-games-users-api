using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UsersAPI.Api.Common;
using UsersAPI.Application.Common;
using UsersAPI.Application.Common.Exceptions;

namespace UsersAPI.Tests.Api.Common;

[Trait("Category", "Unit")]
public sealed class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetRequiredUserId_QuandoClaimSubForGuid_DeveRetornarUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateClaimsPrincipal(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()));

        // Act
        var result = user.GetRequiredUserId();

        // Assert
        result.ShouldBe(userId);
    }

    [Fact]
    public void GetRequiredUserId_QuandoClaimSubNaoExistir_DeveLancarInvalidCredentialsException()
    {
        // Arrange
        var user = CreateClaimsPrincipal();

        // Act
        var exception = Should.Throw<InvalidCredentialsException>(() => user.GetRequiredUserId());

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
    }

    [Fact]
    public void GetRequiredUserId_QuandoClaimSubForInvalida_DeveLancarInvalidCredentialsException()
    {
        // Arrange
        var user = CreateClaimsPrincipal(new Claim(JwtRegisteredClaimNames.Sub, "invalid-user-id"));

        // Act
        var exception = Should.Throw<InvalidCredentialsException>(() => user.GetRequiredUserId());

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(params Claim[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
}

