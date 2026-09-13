using UsersAPI.Domain.Shared;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Tests.Domain.ValueObjects;

[Trait("Category", "Unit")]
public sealed class PasswordHashTests
{
    [Fact]
    public void Deve_Criar_PasswordHash_Quando_Valor_For_Informado()
    {
        // Preparação
        const string valor = "$2a$11$hashfakeparatestes";

        // Execução
        var passwordHash = PasswordHash.Create(valor);

        // Verificação
        passwordHash.Value.ShouldBe(valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_PasswordHash_For_Obrigatorio_E_Nao_For_Informado(string? valor)
    {
        // Preparação
        Action acao = () => PasswordHash.Create(valor!);

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.PasswordHash.Required);
    }

    [Fact]
    public void Deve_Considerar_PasswordHashes_Iguais_Quando_Valores_Forem_Iguais()
    {
        // Preparação
        var primeiroPasswordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var segundoPasswordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        // Execução
        var saoIguais = primeiroPasswordHash.Equals(segundoPasswordHash);

        // Verificação
        saoIguais.ShouldBeTrue();
    }

    [Fact]
    public void Deve_Considerar_PasswordHashes_Diferentes_Quando_Valores_Forem_Diferentes()
    {
        // Preparação
        var primeiroPasswordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var segundoPasswordHash = PasswordHash.Create("$2a$11$outrohashfake");

        // Execução
        var saoIguais = primeiroPasswordHash.Equals(segundoPasswordHash);

        // Verificação
        saoIguais.ShouldBeFalse();
    }
}
