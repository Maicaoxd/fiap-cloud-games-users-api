using UsersAPI.Domain.Shared;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Tests.Domain.ValueObjects;

[Trait("Category", "Unit")]
public sealed class EmailTests
{
    [Fact]
    public void Deve_Criar_Email_Quando_Valor_For_Valido()
    {
        // Preparação
        const string valor = "maicon@email.com";

        // Execução
        var email = Email.Create(valor);

        // Verificação
        email.Value.ShouldBe("maicon@email.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Email_For_Vazio(string? valor)
    {
        // Preparação
        Action acao = () => Email.Create(valor!);

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.Email.Required);
    }

    [Theory]
    [InlineData("maicon")]
    [InlineData("maicon.email.com")]
    [InlineData("maiconemail.com")]
    [InlineData("@email.com")]
    [InlineData("maicon@")]
    [InlineData("maicon@@email.com")]
    [InlineData("maicon@email")]
    [InlineData("maicon @email.com")]
    public void Deve_Lancar_Excecao_Quando_Formato_Do_Email_For_Invalido(string valor)
    {
        // Preparação
        Action acao = () => Email.Create(valor);

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.Email.InvalidFormat);
    }

    [Fact]
    public void Deve_Normalizar_Email_Removendo_Espacos_E_Convertendo_Para_Minusculo()
    {
        // Preparação
        const string valor = "  MAICON@EMAIL.COM  ";

        // Execução
        var email = Email.Create(valor);

        // Verificação
        email.Value.ShouldBe("maicon@email.com");
    }

    [Fact]
    public void Deve_Considerar_Emails_Iguais_Quando_Valores_Normalizados_Forem_Iguais()
    {
        // Preparação
        var primeiroEmail = Email.Create("  MAICON@EMAIL.COM  ");
        var segundoEmail = Email.Create("maicon@email.com");

        // Execução
        var saoIguais = primeiroEmail.Equals(segundoEmail);

        // Verificação
        saoIguais.ShouldBeTrue();
    }

    [Fact]
    public void Deve_Considerar_Emails_Diferentes_Quando_Valores_Forem_Diferentes()
    {
        // Preparação
        var primeiroEmail = Email.Create("maicon@email.com");
        var segundoEmail = Email.Create("outro@email.com");

        // Execução
        var saoIguais = primeiroEmail.Equals(segundoEmail);

        // Verificação
        saoIguais.ShouldBeFalse();
    }

    [Fact]
    public void Deve_Gerar_Mesmo_HashCode_Quando_Emails_Forem_Iguais()
    {
        // Preparação
        var primeiroEmail = Email.Create("  MAICON@EMAIL.COM  ");
        var segundoEmail = Email.Create("maicon@email.com");

        // Execução
        var primeiroHashCode = primeiroEmail.GetHashCode();
        var segundoHashCode = segundoEmail.GetHashCode();

        // Verificação
        primeiroHashCode.ShouldBe(segundoHashCode);
    }
}
