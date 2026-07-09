using UsersAPI.Api.Contracts.Authentication.ForgotPassword;
using UsersAPI.Api.Contracts.Authentication.Login;
using UsersAPI.Api.Controllers;
using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Abstractions.Security;
using UsersAPI.Application.Users.Authenticate;
using UsersAPI.Application.Users.ForgotPassword;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace UsersAPI.Tests.Api.Controllers;

[Trait("Category", "Unit")]
public sealed class AuthenticationControllerTests
{
    [Fact]
    public async Task LoginAsync_QuandoCredenciaisForemValidas_DeveRetornarOkComAccessToken()
    {
        // Arrange
        var user = CreateUser();
        var email = user.Email;
        var passwordHash = user.PasswordHash;
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var accessTokenGenerator = Substitute.For<IAccessTokenGenerator>();

        userRepository
            .GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);

        passwordHasher
            .Verify(Arg.Any<string?>(), passwordHash)
            .Returns(true);

        accessTokenGenerator
            .Generate(user)
            .Returns("access-token");

        var authenticateUserUseCase = new AuthenticateUserUseCase(
            userRepository,
            passwordHasher,
            accessTokenGenerator);
        var forgotPasswordUseCase = new ForgotPasswordUseCase(userRepository, passwordHasher);

        var controller = new AuthenticationController(authenticateUserUseCase, forgotPasswordUseCase);
        var request = new LoginRequest(
            "maicon@email.com",
            "Senha@123");

        // Act
        var actionResult = await controller.LoginAsync(request, CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<LoginResponse>();

        response.AccessToken.ShouldBe("access-token");
        await userRepository.Received(1).GetByEmailAsync(email, Arg.Any<CancellationToken>());
        passwordHasher.Received(1).Verify("Senha@123", passwordHash);
        accessTokenGenerator.Received(1).Generate(user);
    }

    [Fact]
    public async Task ForgotPasswordAsync_QuandoDadosForemValidos_DeveRetornarNoContent()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var cpf = Cpf.Create("529.982.247-25");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var newPasswordHash = PasswordHash.Create("$2a$11$novohashfakeparatestes");
        var user = User.Create("Maicon Guedes", email, cpf, new DateOnly(1993, 6, 17), passwordHash);
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var accessTokenGenerator = Substitute.For<IAccessTokenGenerator>();

        userRepository
            .GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns(user);
        passwordHasher
            .Hash(Arg.Any<Password>())
            .Returns(newPasswordHash);

        var authenticateUserUseCase = new AuthenticateUserUseCase(
            userRepository,
            passwordHasher,
            accessTokenGenerator);
        var forgotPasswordUseCase = new ForgotPasswordUseCase(userRepository, passwordHasher);

        var controller = new AuthenticationController(authenticateUserUseCase, forgotPasswordUseCase);
        var request = new ForgotPasswordRequest(
            email.Value,
            cpf.Value,
            new DateOnly(1993, 6, 17),
            "NovaSenha@123",
            "NovaSenha@123");

        // Act
        var actionResult = await controller.ForgotPasswordAsync(request, CancellationToken.None);

        // Assert
        actionResult.ShouldBeOfType<NoContentResult>();
        user.PasswordHash.ShouldBe(newPasswordHash);
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void LoginAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(AuthenticationController)
            .GetMethod(nameof(AuthenticationController.LoginAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status200OK].Type.ShouldBe(typeof(LoginResponse));
        responseTypes[StatusCodes.Status400BadRequest].Type.ShouldBe(typeof(ValidationProblemDetails));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    [Fact]
    public void ForgotPasswordAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(AuthenticationController)
            .GetMethod(nameof(AuthenticationController.ForgotPasswordAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status204NoContent].Type.ShouldBe(typeof(void));
        responseTypes[StatusCodes.Status400BadRequest].Type.ShouldBe(typeof(ValidationProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    private static User CreateUser()
    {
        var email = Email.Create("maicon@email.com");
        var cpf = Cpf.Create("529.982.247-25");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        return User.Create("Maicon Guedes", email, cpf, new DateOnly(1993, 6, 17), passwordHash);
    }
}

