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
        // Preparação
        var userId = Guid.NewGuid();
        var user = CreateClaimsPrincipal(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()));

        // Execução
        var result = user.GetRequiredUserId();

        // Verificação
        result.ShouldBe(userId);
    }

    [Fact]
    public void GetRequiredUserId_QuandoClaimSubNaoExistir_DeveLancarInvalidCredentialsException()
    {
        // Preparação
        var user = CreateClaimsPrincipal();

        // Execução
        var exception = Should.Throw<InvalidCredentialsException>(() => user.GetRequiredUserId());

        // Verificação
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
    }

    [Fact]
    public void GetRequiredUserId_QuandoClaimSubForInvalida_DeveLancarInvalidCredentialsException()
    {
        // Preparação
        var user = CreateClaimsPrincipal(new Claim(JwtRegisteredClaimNames.Sub, "invalid-user-id"));

        // Execução
        var exception = Should.Throw<InvalidCredentialsException>(() => user.GetRequiredUserId());

        // Verificação
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(params Claim[] claims)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }
}

