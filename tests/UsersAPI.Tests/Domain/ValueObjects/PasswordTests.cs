using UsersAPI.Domain.Shared;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Tests.Domain.ValueObjects;

[Trait("Category", "Unit")]
public sealed class PasswordTests
{
    [Fact]
    public void Deve_Criar_Senha_Quando_Valor_For_Forte()
    {
        // Preparação
        const string valor = "Senha@123";

        // Execução
        var senha = Password.Create(valor);

        // Verificação
        senha.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Senha_For_Obrigatoria_E_Nao_For_Informada(string? valor)
    {
        // Preparação
        Action acao = () => Password.Create(valor!);

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.Password.Required);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Tiver_Menos_De_Oito_Caracteres()
    {
        // Preparação
        const string valor = "Senha@1";

        // Execução
        Action acao = () => Password.Create(valor);

        // Verificação
        var excecao = Should.Throw<ArgumentException>(acao);
        excecao.Message.ShouldBe(DomainMessages.Password.MinimumLength);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Nao_Tiver_Letras()
    {
        // Preparação
        const string valor = "12345678@";

        // Execução
        Action acao = () => Password.Create(valor);

        // Verificação
        var excecao = Should.Throw<ArgumentException>(acao);
        excecao.Message.ShouldBe(DomainMessages.Password.LetterRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Nao_Tiver_Numeros()
    {
        // Preparação
        const string valor = "Senha@@@";

        // Execução
        Action acao = () => Password.Create(valor);

        // Verificação
        var excecao = Should.Throw<ArgumentException>(acao);
        excecao.Message.ShouldBe(DomainMessages.Password.NumberRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Nao_Tiver_Caractere_Especial()
    {
        // Preparação
        const string valor = "Senha123";

        // Execução
        Action acao = () => Password.Create(valor);

        // Verificação
        var excecao = Should.Throw<ArgumentException>(acao);
        excecao.Message.ShouldBe(DomainMessages.Password.SpecialCharacterRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Tiver_Espaco_Em_Branco()
    {
        // Preparação
        const string valor = "Senha 123";

        // Execução
        Action acao = () => Password.Create(valor);

        // Verificação
        var excecao = Should.Throw<ArgumentException>(acao);
        excecao.Message.ShouldBe(DomainMessages.Password.WhiteSpaceNotAllowed);
    }

    [Fact]
    public void Deve_Considerar_Senhas_Iguais_Quando_Valores_Forem_Iguais()
    {
        // Preparação
        var primeiraSenha = Password.Create("Senha@123");
        var segundaSenha = Password.Create("Senha@123");

        // Execução
        var saoIguais = primeiraSenha.Equals(segundaSenha);

        // Verificação
        saoIguais.ShouldBeTrue();
    }

    [Fact]
    public void Deve_Considerar_Senhas_Diferentes_Quando_Valores_Forem_Diferentes()
    {
        // Preparação
        var primeiraSenha = Password.Create("Senha@123");
        var segundaSenha = Password.Create("Senha@456");

        // Execução
        var saoIguais = primeiraSenha.Equals(segundaSenha);

        // Verificação
        saoIguais.ShouldBeFalse();
    }
}
