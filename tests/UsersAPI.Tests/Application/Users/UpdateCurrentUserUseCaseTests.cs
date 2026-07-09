using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Common;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Application.Users.UpdateCurrent;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;
using NSubstitute;

namespace UsersAPI.Tests.Application.Users;

[Trait("Category", "Unit")]
public sealed class UpdateCurrentUserUseCaseTests
{
    [Fact]
    public async Task Deve_Atualizar_Usuario_Quando_Dados_Forem_Validos()
    {
        // Arrange
        var birthDate = new DateOnly(1994, 7, 18);
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        userRepository
            .GetByEmailAsync(Email.Create("maicon.guedes@email.com"), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var useCase = new UpdateCurrentUserUseCase(userRepository);
        var command = new UpdateCurrentUserCommand(
            user.Id,
            "Maicon Guedes",
            "maicon.guedes@email.com",
            birthDate);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        user.Name.ShouldBe("Maicon Guedes");
        user.Email.ShouldBe(Email.Create("maicon.guedes@email.com"));
        user.Cpf.ShouldBe(Cpf.Create("529.982.247-25"));
        user.BirthDate.ShouldBe(birthDate);
        user.UpdatedBy.ShouldBe(user.Id);
        user.UpdatedAt.ShouldNotBeNull();
        await userRepository.DidNotReceive().GetByCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>());
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Permitir_Atualizar_Quando_Email_Pertencer_Ao_Proprio_Usuario()
    {
        // Arrange
        var user = CreateUserWithRecoveryData();
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        userRepository
            .GetByEmailAsync(user.Email, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new UpdateCurrentUserUseCase(userRepository);
        var command = new UpdateCurrentUserCommand(
            user.Id,
            "Maicon Guedes",
            user.Email.Value,
            user.BirthDate);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        user.Name.ShouldBe("Maicon Guedes");
        user.Email.ShouldBe(Email.Create("maicon@email.com"));
        user.Cpf.ShouldBe(Cpf.Create("52998224725"));
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Usuario_Nao_Existir()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var useCase = new UpdateCurrentUserUseCase(userRepository);
        var command = new UpdateCurrentUserCommand(
            userId,
            "Maicon Guedes",
            "maicon@email.com",
            new DateOnly(1993, 6, 17));

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

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new UpdateCurrentUserUseCase(userRepository);
        var command = new UpdateCurrentUserCommand(
            user.Id,
            "Maicon Guedes",
            "maicon@email.com",
            new DateOnly(1993, 6, 17));

        // Act
        var exception = await Should.ThrowAsync<InactiveUserException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.Authentication.InactiveUser);
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Email_Pertencer_A_Outro_Usuario()
    {
        // Arrange
        var user = CreateUser();
        var anotherUser = User.Create(
            "Outro Usuario",
            Email.Create("outro@email.com"),
            Cpf.Create("286.255.878-87"),
            new DateOnly(1988, 1, 20),
            PasswordHash.Create("$2a$11$outrohashfakeparatestes"));
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        userRepository
            .GetByEmailAsync(Email.Create("outro@email.com"), Arg.Any<CancellationToken>())
            .Returns(anotherUser);
        userRepository
            .GetByCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var useCase = new UpdateCurrentUserUseCase(userRepository);
        var command = new UpdateCurrentUserCommand(
            user.Id,
            "Maicon Guedes",
            "outro@email.com",
            new DateOnly(1993, 6, 17));

        // Act
        var exception = await Should.ThrowAsync<EmailAlreadyRegisteredException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.User.EmailAlreadyRegistered);
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    private static User CreateUser()
    {
        var email = Email.Create("maicon@email.com");
        var cpf = Cpf.Create("529.982.247-25");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        return User.Create("Maicon Alves", email, cpf, new DateOnly(1993, 6, 17), passwordHash);
    }

    private static User CreateUserWithRecoveryData()
    {
        var email = Email.Create("maicon@email.com");
        var cpf = Cpf.Create("529.982.247-25");
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        return User.Create("Maicon Alves", email, cpf, new DateOnly(1993, 6, 17), passwordHash);
    }
}

