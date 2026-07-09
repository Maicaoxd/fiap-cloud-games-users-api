using System.IdentityModel.Tokens.Jwt;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Tests.Infrastructure.Security;

[Trait("Category", "Unit")]
public sealed class JwtAccessTokenGeneratorTests
{
    [Fact]
    public void Generate_QuandoUsuarioForValido_DeveGerarAccessTokenComClaimsDoUsuario()
    {
        // Arrange
        var jwtOptions = new JwtOptions(
            "FiapCloudGames",
            "FiapCloudGames",
            "maicon-guedes-dotnet-architect-level-99-cloud-games-key",
            60);

        var email = Email.Create("maicon@email.com");
        var cpf = Cpf.Create("529.982.247-25");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var user = User.Create("Maicon Guedes", email, cpf, new DateOnly(1993, 6, 17), passwordHash);
        var accessTokenGenerator = new JwtAccessTokenGenerator(jwtOptions);

        // Act
        var accessToken = accessTokenGenerator.Generate(user);

        // Assert
        accessToken.ShouldNotBeNullOrWhiteSpace();

        var token = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

        token.Issuer.ShouldBe(jwtOptions.Issuer);
        token.Audiences.ShouldContain(jwtOptions.Audience);
        token.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value.ShouldBe(user.Id.ToString());
        token.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Email).Value.ShouldBe(user.Email.Value);
        token.Claims.Single(claim => claim.Type == JwtClaimNames.Role).Value.ShouldBe(UserRole.User.ToString());
        token.ValidTo.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(55));
        token.ValidTo.ShouldBeLessThan(DateTime.UtcNow.AddMinutes(65));
    }
}

