using UsersAPI.Domain.Users.ValueObjects;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Tests.Infrastructure.Security;

[Trait("Category", "Unit")]
public sealed class BCryptPasswordHasherTests
{
    [Fact]
    public void Deve_Gerar_PasswordHash_Quando_Senha_For_Valida()
    {
        // Preparação
        var password = Password.Create("Senha@123");
        var passwordHasher = new BCryptPasswordHasher();

        // Execução
        var passwordHash = passwordHasher.Hash(password);

        // Verificação
        string.IsNullOrWhiteSpace(passwordHash.Value).ShouldBeFalse();
        passwordHash.Value.ShouldNotBe(password.Value);
    }

    [Fact]
    public void Deve_Verificar_Senha_Quando_Valor_Original_For_Correto()
    {
        // Preparação
        var password = Password.Create("Senha@123");
        var passwordHasher = new BCryptPasswordHasher();
        var passwordHash = passwordHasher.Hash(password);

        // Execução
        var senhaCorreta = passwordHasher.Verify("Senha@123", passwordHash);

        // Verificação
        senhaCorreta.ShouldBeTrue();
    }

    [Fact]
    public void Deve_Nao_Verificar_Senha_Quando_Valor_Original_For_Incorreto()
    {
        // Preparação
        var password = Password.Create("Senha@123");
        var wrongPassword = Password.Create("Outra@123");
        var passwordHasher = new BCryptPasswordHasher();
        var passwordHash = passwordHasher.Hash(password);

        // Execução
        var senhaCorreta = passwordHasher.Verify(wrongPassword.Value, passwordHash);

        // Verificação
        senhaCorreta.ShouldBeFalse();
    }

    [Fact]
    public void Deve_Nao_Verificar_Senha_Quando_Valor_For_Nulo()
    {
        // Preparação
        var password = Password.Create("Senha@123");
        var passwordHasher = new BCryptPasswordHasher();
        var passwordHash = passwordHasher.Hash(password);

        // Execução
        var senhaCorreta = passwordHasher.Verify((string?)null, passwordHash);

        // Verificação
        senhaCorreta.ShouldBeFalse();
    }
}
