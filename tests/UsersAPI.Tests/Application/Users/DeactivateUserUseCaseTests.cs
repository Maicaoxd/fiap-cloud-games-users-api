using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Common;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Application.Users.Deactivate;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;
using NSubstitute;

namespace UsersAPI.Tests.Application.Users;

[Trait("Category", "Unit")]
public sealed class DeactivateUserUseCaseTests
{
    [Fact]
    public async Task Deve_Desativar_Usuario_Quando_Ele_Estiver_Ativo()
    {
        // Arrange
        var deactivatedBy = Guid.NewGuid();
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new DeactivateUserUseCase(userRepository);
        var command = new DeactivateUserCommand(user.Id, deactivatedBy);

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        user.IsActive.ShouldBeFalse();
        user.UpdatedBy.ShouldBe(deactivatedBy);
        user.UpdatedAt.ShouldNotBeNull();
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

        var useCase = new DeactivateUserUseCase(userRepository);
        var command = new DeactivateUserCommand(userId, Guid.NewGuid());

        // Act
        var exception = await Should.ThrowAsync<UserNotFoundException>(() => useCase.ExecuteAsync(command));

        // Assert
        exception.Message.ShouldBe(ApplicationMessages.User.NotFound);
        await userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Deve_Nao_Persistir_Quando_Usuario_Ja_Estiver_Inativo()
    {
        // Arrange
        var user = CreateUser();
        user.Deactivate(Guid.NewGuid());

        var updatedAt = user.UpdatedAt;
        var updatedBy = user.UpdatedBy;
        var userRepository = Substitute.For<IUserRepository>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var useCase = new DeactivateUserUseCase(userRepository);
        var command = new DeactivateUserCommand(user.Id, Guid.NewGuid());

        // Act
        await useCase.ExecuteAsync(command);

        // Assert
        user.IsActive.ShouldBeFalse();
        user.UpdatedAt.ShouldBe(updatedAt);
        user.UpdatedBy.ShouldBe(updatedBy);
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

