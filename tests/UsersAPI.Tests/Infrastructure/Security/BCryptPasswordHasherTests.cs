using UsersAPI.Domain.Users.ValueObjects;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Tests.Infrastructure.Security;

[Trait("Category", "Unit")]
public sealed class BCryptPasswordHasherTests
{
    [Fact]
    public void Deve_Gerar_PasswordHash_Quando_Senha_For_Valida()
    {
        // Arrange
        var password = Password.Create("Senha@123");
        var passwordHasher = new BCryptPasswordHasher();

        // Act
        var passwordHash = passwordHasher.Hash(password);

        // Assert
        string.IsNullOrWhiteSpace(passwordHash.Value).ShouldBeFalse();
        passwordHash.Value.ShouldNotBe(password.Value);
    }

    [Fact]
    public void Deve_Verificar_Senha_Quando_Valor_Original_For_Correto()
    {
        // Arrange
        var password = Password.Create("Senha@123");
        var passwordHasher = new BCryptPasswordHasher();
        var passwordHash = passwordHasher.Hash(password);

        // Act
        var senhaCorreta = passwordHasher.Verify("Senha@123", passwordHash);

        // Assert
        senhaCorreta.ShouldBeTrue();
    }

    [Fact]
    public void Deve_Nao_Verificar_Senha_Quando_Valor_Original_For_Incorreto()
    {
        // Arrange
        var password = Password.Create("Senha@123");
        var wrongPassword = Password.Create("Outra@123");
        var passwordHasher = new BCryptPasswordHasher();
        var passwordHash = passwordHasher.Hash(password);

        // Act
        var senhaCorreta = passwordHasher.Verify(wrongPassword.Value, passwordHash);

        // Assert
        senhaCorreta.ShouldBeFalse();
    }

    [Fact]
    public void Deve_Nao_Verificar_Senha_Quando_Valor_For_Nulo()
    {
        // Arrange
        var password = Password.Create("Senha@123");
        var passwordHasher = new BCryptPasswordHasher();
        var passwordHash = passwordHasher.Hash(password);

        // Act
        var senhaCorreta = passwordHasher.Verify((string?)null, passwordHash);

        // Assert
        senhaCorreta.ShouldBeFalse();
    }
}

