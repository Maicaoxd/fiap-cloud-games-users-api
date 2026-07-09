using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Users.List;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;
using NSubstitute;

namespace UsersAPI.Tests.Application.Users;

[Trait("Category", "Unit")]
public sealed class ListUsersUseCaseTests
{
    [Fact]
    public async Task Deve_Listar_Usuarios()
    {
        // Arrange
        var firstUser = CreateUser("Ana Guedes", "ana@email.com", "529.982.247-25");
        var secondUser = CreateUser("Maicon Guedes", "maicon@email.com", "168.995.350-09");
        secondUser.Deactivate(Guid.NewGuid());

        var userRepository = Substitute.For<IUserRepository>();
        userRepository
            .ListAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { firstUser, secondUser });

        var useCase = new ListUsersUseCase(userRepository);

        // Act
        var result = await useCase.ExecuteAsync();

        // Assert
        result.Count.ShouldBe(2);
        result.ShouldContain(user =>
            user.UserId == firstUser.Id &&
            user.Name == "Ana Guedes" &&
            user.Email == "ana@email.com" &&
            user.Cpf == "52998224725" &&
            user.BirthDate == new DateOnly(1993, 6, 17) &&
            user.Role == nameof(UserRole.User) &&
            user.IsActive);
        result.ShouldContain(user =>
            user.UserId == secondUser.Id &&
            user.Name == "Maicon Guedes" &&
            user.Email == "maicon@email.com" &&
            user.Cpf == "16899535009" &&
            user.BirthDate == new DateOnly(1993, 6, 17) &&
            user.Role == nameof(UserRole.User) &&
            !user.IsActive);
        await userRepository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    private static User CreateUser(string name, string email, string cpf)
    {
        return User.Create(
            name,
            Email.Create(email),
            Cpf.Create(cpf),
            new DateOnly(1993, 6, 17),
            PasswordHash.Create("$2a$11$hashfakeparatestes"));
    }
}

