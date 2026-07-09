using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Abstractions.Security;
using UsersAPI.Application.Common;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Application.Users.ForgotPassword;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;
using NSubstitute;

namespace UsersAPI.Tests.Application.Users;

[Trait("Category", "Unit")]
public sealed class ForgotPasswordUseCaseTests
{
    [Fact]
    public async Task Deve_Redefinir_Senha_Quando_Dados_De_Recuperacao_Forem_Validos()
    {
        // Arrange
        var user = CreateUser();
        var newPasswordHash = PasswordHash.Create("$2a$11$novohashfakeparatestes");
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByEmailAsync(user.Email, Arg.Any<CancellationToken>())
            .Returns(user);
        passwordHasher
            .Hash(Arg.Any<Password>())
            .Returns(newPasswordHash);

        var useCase = new ForgotPasswordUseCase(userRepository, passwordHasher);
        var command = new ForgotPasswordCommand(
            user.Email.Value,
            user.Cpf!.Value,
            user.BirthDate,
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
    public async Task Deve_Lancar_Excecao_Quando_Email_Nao_Estiver_Cadastrado()
    {
        // Arrange
        var email = Email.Create("maicon@email.com");
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByEmailAsync(email, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var useCase = new ForgotPasswordUseCase(userRepository, passwordHasher);
        var command = new ForgotPasswordCommand(
            email.Value,
            "529.982.247-25",
            new DateOnly(1993, 6, 17),
            "NovaSenha@123",
            "NovaSenha@123");

        // Act
        var exception = await Should.ThrowAsync<InvalidPasswordRecoveryDataException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.PasswordRecovery.InvalidRecoveryData);
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Cpf_Nao_Corresponder()
    {
        // Arrange
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByEmailAsync(user.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new ForgotPasswordUseCase(userRepository, passwordHasher);
        var command = new ForgotPasswordCommand(
            user.Email.Value,
            "286.255.878-87",
            user.BirthDate,
            "NovaSenha@123",
            "NovaSenha@123");

        // Act
        var exception = await Should.ThrowAsync<InvalidPasswordRecoveryDataException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.PasswordRecovery.InvalidRecoveryData);
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Data_De_Nascimento_Nao_Corresponder()
    {
        // Arrange
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByEmailAsync(user.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new ForgotPasswordUseCase(userRepository, passwordHasher);
        var command = new ForgotPasswordCommand(
            user.Email.Value,
            user.Cpf!.Value,
            new DateOnly(1990, 1, 1),
            "NovaSenha@123",
            "NovaSenha@123");

        // Act
        var exception = await Should.ThrowAsync<InvalidPasswordRecoveryDataException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.PasswordRecovery.InvalidRecoveryData);
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
            .GetByEmailAsync(user.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new ForgotPasswordUseCase(userRepository, passwordHasher);
        var command = new ForgotPasswordCommand(
            user.Email.Value,
            user.Cpf!.Value,
            user.BirthDate,
            "NovaSenha@123",
            "NovaSenha@123");

        // Act
        var exception = await Should.ThrowAsync<InvalidPasswordRecoveryDataException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.PasswordRecovery.InvalidRecoveryData);
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Confirmacao_Da_Nova_Senha_Nao_Conferir()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var useCase = new ForgotPasswordUseCase(userRepository, passwordHasher);
        var command = new ForgotPasswordCommand(
            "maicon@email.com",
            "529.982.247-25",
            new DateOnly(1993, 6, 17),
            "NovaSenha@123",
            "OutraSenha@123");

        // Act
        var exception = await Should.ThrowAsync<ArgumentException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.User.PasswordConfirmationDoesNotMatch);
        passwordHasher.DidNotReceive().Hash(Arg.Any<Password>());
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

