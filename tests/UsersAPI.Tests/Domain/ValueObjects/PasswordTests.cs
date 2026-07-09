using UsersAPI.Domain.Shared;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Tests.Domain.ValueObjects;

[Trait("Category", "Unit")]
public sealed class PasswordTests
{
    [Fact]
    public void Deve_Criar_Senha_Quando_Valor_For_Forte()
    {
        // Arrange
        const string valor = "Senha@123";

        // Act
        var senha = Password.Create(valor);

        // Assert
        senha.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Senha_For_Obrigatoria_E_Nao_For_Informada(string? valor)
    {
        // Arrange
        Action acao = () => Password.Create(valor!);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Password.Required);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Tiver_Menos_De_Oito_Caracteres()
    {
        // Arrange
        const string valor = "Senha@1";

        // Act
        Action acao = () => Password.Create(valor);

        // Assert
        var excecao = Should.Throw<ArgumentException>(acao);
        excecao.Message.ShouldBe(DomainMessages.Password.MinimumLength);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Nao_Tiver_Letras()
    {
        // Arrange
        const string valor = "12345678@";

        // Act
        Action acao = () => Password.Create(valor);

        // Assert
        var excecao = Should.Throw<ArgumentException>(acao);
        excecao.Message.ShouldBe(DomainMessages.Password.LetterRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Nao_Tiver_Numeros()
    {
        // Arrange
        const string valor = "Senha@@@";

        // Act
        Action acao = () => Password.Create(valor);

        // Assert
        var excecao = Should.Throw<ArgumentException>(acao);
        excecao.Message.ShouldBe(DomainMessages.Password.NumberRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Nao_Tiver_Caractere_Especial()
    {
        // Arrange
        const string valor = "Senha123";

        // Act
        Action acao = () => Password.Create(valor);

        // Assert
        var excecao = Should.Throw<ArgumentException>(acao);
        excecao.Message.ShouldBe(DomainMessages.Password.SpecialCharacterRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Senha_Tiver_Espaco_Em_Branco()
    {
        // Arrange
        const string valor = "Senha 123";

        // Act
        Action acao = () => Password.Create(valor);

        // Assert
        var excecao = Should.Throw<ArgumentException>(acao);
        excecao.Message.ShouldBe(DomainMessages.Password.WhiteSpaceNotAllowed);
    }

    [Fact]
    public void Deve_Considerar_Senhas_Iguais_Quando_Valores_Forem_Iguais()
    {
        // Arrange
        var primeiraSenha = Password.Create("Senha@123");
        var segundaSenha = Password.Create("Senha@123");

        // Act
        var saoIguais = primeiraSenha.Equals(segundaSenha);

        // Assert
        saoIguais.ShouldBeTrue();
    }

    [Fact]
    public void Deve_Considerar_Senhas_Diferentes_Quando_Valores_Forem_Diferentes()
    {
        // Arrange
        var primeiraSenha = Password.Create("Senha@123");
        var segundaSenha = Password.Create("Senha@456");

        // Act
        var saoIguais = primeiraSenha.Equals(segundaSenha);

        // Assert
        saoIguais.ShouldBeFalse();
    }
}

