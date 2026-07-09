using UsersAPI.Application.Abstractions.Messaging;
using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Abstractions.Security;
using UsersAPI.Application.Common;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Application.Users.Register;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;
using NSubstitute;

namespace UsersAPI.Tests.Application.Users;

[Trait("Category", "Unit")]
public sealed class RegisterUserUseCaseTests
{
    [Fact]
    public async Task Deve_Cadastrar_Usuario_Quando_Dados_Forem_Validos()
    {
        // Arrange
        var birthDate = new DateOnly(1993, 6, 17);
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        User? addedUser = null;

        userRepository
            .ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);
        userRepository
            .ExistsByCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>())
            .Returns(false);
        userRepository
            .AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                addedUser = callInfo.ArgAt<User>(0);
                return Task.CompletedTask;
            });

        passwordHasher.Hash(Arg.Any<Password>()).Returns(passwordHash);

        var useCase = new RegisterUserUseCase(userRepository, passwordHasher, Substitute.For<IUserCreatedEventPublisher>());
        var command = new RegisterUserCommand(
            "Maicon Guedes",
            "maicon@email.com",
            "529.982.247-25",
            birthDate,
            "Senha@123",
            "Senha@123");

        // Act
        var result = await useCase.ExecuteAsync(command);

        // Assert
        result.UserId.ShouldNotBe(Guid.Empty);
        addedUser.ShouldNotBeNull();
        addedUser!.Id.ShouldBe(result.UserId);
        addedUser.Name.ShouldBe("Maicon Guedes");
        addedUser.Email.ShouldBe(Email.Create("maicon@email.com"));
        addedUser.Cpf.ShouldBe(Cpf.Create("529.982.247-25"));
        addedUser.BirthDate.ShouldBe(birthDate);
        addedUser.PasswordHash.ShouldBe(passwordHash);
        addedUser.Role.ShouldBe(UserRole.User);
        passwordHasher.Received(1).Hash(Arg.Any<Password>());
        await userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Senha_E_Confirmacao_Nao_Conferirem()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var useCase = new RegisterUserUseCase(userRepository, passwordHasher, Substitute.For<IUserCreatedEventPublisher>());
        var command = new RegisterUserCommand(
            "Maicon Guedes",
            "maicon@email.com",
            "529.982.247-25",
            new DateOnly(1993, 6, 17),
            "Senha@123",
            "Outra@123");

        // Act
        var excecao = await Should.ThrowAsync<ArgumentException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe(ApplicationMessages.User.PasswordConfirmationDoesNotMatch);
        passwordHasher.DidNotReceive().Hash(Arg.Any<Password>());
        await userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Email_Ja_Estiver_Cadastrado()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var useCase = new RegisterUserUseCase(userRepository, passwordHasher, Substitute.For<IUserCreatedEventPublisher>());
        var command = new RegisterUserCommand(
            "Maicon Guedes",
            "maicon@email.com",
            "529.982.247-25",
            new DateOnly(1993, 6, 17),
            "Senha@123",
            "Senha@123");

        // Act
        var excecao = await Should.ThrowAsync<EmailAlreadyRegisteredException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe(ApplicationMessages.User.EmailAlreadyRegistered);
        passwordHasher.DidNotReceive().Hash(Arg.Any<Password>());
        await userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Cpf_Ja_Estiver_Cadastrado()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);
        userRepository
            .ExistsByCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var useCase = new RegisterUserUseCase(userRepository, passwordHasher, Substitute.For<IUserCreatedEventPublisher>());
        var command = new RegisterUserCommand(
            "Maicon Guedes",
            "maicon@email.com",
            "529.982.247-25",
            new DateOnly(1993, 6, 17),
            "Senha@123",
            "Senha@123");

        // Act
        var excecao = await Should.ThrowAsync<CpfAlreadyRegisteredException>(() => useCase.ExecuteAsync(command));

        // Assert
        excecao.Message.ShouldBe(ApplicationMessages.User.CpfAlreadyRegistered);
        passwordHasher.DidNotReceive().Hash(Arg.Any<Password>());
        await userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }
}


