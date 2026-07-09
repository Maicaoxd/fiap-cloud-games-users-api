using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Abstractions.Security;
using UsersAPI.Application.Common;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Application.Users.ChangePassword;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;
using NSubstitute;

namespace UsersAPI.Tests.Application.Users;

[Trait("Category", "Unit")]
public sealed class ChangePasswordUseCaseTests
{
    [Fact]
    public async Task Deve_Alterar_Senha_Quando_Dados_Forem_Validos()
    {
        // Arrange
        var user = CreateUser();
        var currentPasswordHash = user.PasswordHash;
        var newPasswordHash = PasswordHash.Create("$2a$11$novohashfakeparatestes");
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        passwordHasher
            .Verify("Senha@123", currentPasswordHash)
            .Returns(true);
        passwordHasher
            .Hash(Arg.Any<Password>())
            .Returns(newPasswordHash);

        var useCase = new ChangePasswordUseCase(userRepository, passwordHasher);
        var command = new ChangePasswordCommand(
            user.Id,
            "Senha@123",
            "NovaSenha@123",
            "NovaSenha@123");

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        user.PasswordHash.ShouldBe(newPasswordHash);
        user.UpdatedBy.ShouldBe(user.Id);
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Nao_Existir()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var userId = Guid.NewGuid();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var useCase = new ChangePasswordUseCase(userRepository, passwordHasher);
        var command = new ChangePasswordCommand(
            userId,
            "Senha@123",
            "NovaSenha@123",
            "NovaSenha@123");

        // Act
        var exception = await Should.ThrowAsync<InvalidCredentialsException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Estiver_Inativo()
    {
        // Arrange
        var user = CreateUser();
        user.Deactivate(Guid.NewGuid());
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new ChangePasswordUseCase(userRepository, passwordHasher);
        var command = new ChangePasswordCommand(
            user.Id,
            "Senha@123",
            "NovaSenha@123",
            "NovaSenha@123");

        // Act
        var exception = await Should.ThrowAsync<InactiveUserException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InactiveUser);
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Senha_Atual_For_Invalida()
    {
        // Arrange
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        passwordHasher
            .Verify("SenhaErrada@123", user.PasswordHash)
            .Returns(false);

        var useCase = new ChangePasswordUseCase(userRepository, passwordHasher);
        var command = new ChangePasswordCommand(
            user.Id,
            "SenhaErrada@123",
            "NovaSenha@123",
            "NovaSenha@123");

        // Act
        var exception = await Should.ThrowAsync<InvalidCredentialsException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InvalidCredentials);
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Confirmacao_Da_Nova_Senha_Nao_Conferir()
    {
        // Arrange
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new ChangePasswordUseCase(userRepository, passwordHasher);
        var command = new ChangePasswordCommand(
            user.Id,
            "Senha@123",
            "NovaSenha@123",
            "OutraSenha@123");

        // Act
        var exception = await Should.ThrowAsync<ArgumentException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.User.PasswordConfirmationDoesNotMatch);
        passwordHasher.DidNotReceive().Verify(Arg.Any<string?>(), Arg.Any<PasswordHash>());
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    private static User CreateUser()
    {
        var email = Email.Create("maicon@email.com");
        var cpf = Cpf.Create("529.982.247-25");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        return User.Create("Maicon Guedes", email, cpf, new DateOnly(1993, 6, 17), passwordHash);
    }
}

