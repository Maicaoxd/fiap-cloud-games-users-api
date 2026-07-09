using UsersAPI.Domain.Shared;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Tests.Domain.Users;

[Trait("Category", "Unit")]
public sealed class CpfTests
{
    [Fact]
    public void Deve_Criar_Cpf_Quando_Valor_For_Valido()
    {
        // Arrange
        const string value = "529.982.247-25";

        // Act
        var cpf = Cpf.Create(value);

        // Assert
        cpf.Value.ShouldBe("52998224725");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Lancar_Excecao_Quando_Cpf_For_Obrigatorio(string? value)
    {
        // Arrange
        Action action = () => Cpf.Create(value!);

        // Act
        var exception = Should.Throw<ArgumentException>(action);

        // Assert
        exception.Message.ShouldBe(DomainMessages.Cpf.Required);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("111.111.111-11")]
    [InlineData("529.982.247-24")]
    public void Deve_Lancar_Excecao_Quando_Cpf_For_Invalido(string value)
    {
        // Arrange
        Action action = () => Cpf.Create(value);

        // Act
        var exception = Should.Throw<ArgumentException>(action);

        // Assert
        exception.Message.ShouldBe(DomainMessages.Cpf.InvalidFormat);
    }

    [Fact]
    public void Deve_Comparar_Cpf_Por_Valor()
    {
        // Arrange
        var firstCpf = Cpf.Create("529.982.247-25");
        var secondCpf = Cpf.Create("52998224725");

        // Act & Assert
        firstCpf.ShouldBe(secondCpf);
        firstCpf.GetHashCode().ShouldBe(secondCpf.GetHashCode());
    }
}

