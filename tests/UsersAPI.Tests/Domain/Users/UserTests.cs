using UsersAPI.Domain.Shared;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Tests.Domain.Users;

[Trait("Category", "Unit")]
public sealed class UserTests
{
    [Fact]
    public void Deve_Criar_Usuario_Quando_Dados_Forem_Validos()
    {
        // Arrange
        const string nome = "Maicon Guedes";
        var email = Email.Create("maicon@email.com");
        var cpf = Cpf.Create("529.982.247-25");
        var birthDate = new DateOnly(1993, 6, 17);
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        // Act
        var usuario = User.Create(nome, email, cpf, birthDate, passwordHash);

        // Assert
        usuario.Id.ShouldNotBe(Guid.Empty);
        usuario.Name.ShouldBe(nome);
        usuario.Email.ShouldBe(email);
        usuario.Cpf.ShouldBe(cpf);
        usuario.BirthDate.ShouldBe(birthDate);
        usuario.PasswordHash.ShouldBe(passwordHash);
        usuario.Role.ShouldBe(UserRole.User);
        usuario.IsActive.ShouldBeTrue();
        usuario.CreatedAt.ShouldNotBe(default);
        usuario.CreatedBy.ShouldBe(usuario.Id);
        usuario.UpdatedAt.ShouldBeNull();
        usuario.UpdatedBy.ShouldBeNull();
    }

    [Fact]
    public void Deve_Criar_Usuario_Com_Auditoria_Quando_Criado_Por_Outro_Usuario()
    {
        // Arrange
        var criadoPor = Guid.NewGuid();

        // Act
        var usuario = CreateUser(createdBy: criadoPor);

        // Assert
        usuario.CreatedBy.ShouldBe(criadoPor);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Criar_Usuario_Com_Responsavel_Invalido()
    {
        // Arrange
        Action acao = () => CreateUser(createdBy: Guid.Empty);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Nome_For_Obrigatorio_E_Nao_For_Informado(string? nome)
    {
        // Arrange
        Action acao = () => CreateUser(name: nome!);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.User.NameRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Email_For_Obrigatorio_E_Nao_For_Informado()
    {
        // Arrange
        Action acao = () => User.Create(
            "Maicon Guedes",
            null!,
            Cpf.Create("529.982.247-25"),
            new DateOnly(1993, 6, 17),
            PasswordHash.Create("$2a$11$hashfakeparatestes"));

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Email.Required);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Cpf_For_Obrigatorio_E_Nao_For_Informado()
    {
        // Arrange
        Action acao = () => User.Create(
            "Maicon Guedes",
            Email.Create("maicon@email.com"),
            null!,
            new DateOnly(1993, 6, 17),
            PasswordHash.Create("$2a$11$hashfakeparatestes"));

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Cpf.Required);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Data_De_Nascimento_For_Obrigatoria_E_Nao_For_Informada()
    {
        // Arrange
        Action acao = () => User.Create(
            "Maicon Guedes",
            Email.Create("maicon@email.com"),
            Cpf.Create("529.982.247-25"),
            default,
            PasswordHash.Create("$2a$11$hashfakeparatestes"));

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.User.BirthDateRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_PasswordHash_For_Obrigatorio_E_Nao_For_Informado()
    {
        // Arrange
        Action acao = () => User.Create(
            "Maicon Guedes",
            Email.Create("maicon@email.com"),
            Cpf.Create("529.982.247-25"),
            new DateOnly(1993, 6, 17),
            null!);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.PasswordHash.Required);
    }

    [Fact]
    public void Deve_Desativar_Usuario_Quando_Usuario_Estiver_Ativo()
    {
        // Arrange
        var desativadoPor = Guid.NewGuid();
        var usuario = CreateUser();

        // Act
        usuario.Deactivate(desativadoPor);

        // Assert
        usuario.IsActive.ShouldBeFalse();
        usuario.UpdatedAt.ShouldNotBeNull();
        usuario.UpdatedBy.ShouldBe(desativadoPor);
    }

    [Fact]
    public void Deve_Reativar_Usuario_Quando_Usuario_Estiver_Inativo()
    {
        // Arrange
        var desativadoPor = Guid.NewGuid();
        var ativadoPor = Guid.NewGuid();
        var usuario = CreateUser();
        usuario.Deactivate(desativadoPor);

        // Act
        usuario.Activate(ativadoPor);

        // Assert
        usuario.IsActive.ShouldBeTrue();
        usuario.UpdatedAt.ShouldNotBeNull();
        usuario.UpdatedBy.ShouldBe(ativadoPor);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Desativar_Usuario_Sem_Responsavel_Valido()
    {
        // Arrange
        var usuario = CreateUser();
        Action acao = () => usuario.Deactivate(Guid.Empty);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Reativar_Usuario_Sem_Responsavel_Valido()
    {
        // Arrange
        var usuario = CreateUser();
        usuario.Deactivate(Guid.NewGuid());
        Action acao = () => usuario.Activate(Guid.Empty);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Fact]
    public void Deve_Alterar_Nome_Quando_Nome_For_Valido()
    {
        // Arrange
        var atualizadoPor = Guid.NewGuid();
        var usuario = CreateUser(name: "Maicon Alves");

        // Act
        usuario.ChangeName("Maicon Guedes", atualizadoPor);

        // Assert
        usuario.Name.ShouldBe("Maicon Guedes");
        usuario.UpdatedAt.ShouldNotBeNull();
        usuario.UpdatedBy.ShouldBe(atualizadoPor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Alterar_Nome_Para_Valor_Obrigatorio_E_Nao_Informado(string? nome)
    {
        // Arrange
        var usuario = CreateUser();
        Action acao = () => usuario.ChangeName(nome!, Guid.NewGuid());

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.User.NameRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Nome_Sem_Responsavel_Valido()
    {
        // Arrange
        var usuario = CreateUser();
        Action acao = () => usuario.ChangeName("Maicon Guedes", Guid.Empty);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Fact]
    public void Deve_Alterar_Email_Quando_Email_For_Valido()
    {
        // Arrange
        var atualizadoPor = Guid.NewGuid();
        var usuario = CreateUser();
        var novoEmail = Email.Create("novo@email.com");

        // Act
        usuario.ChangeEmail(novoEmail, atualizadoPor);

        // Assert
        usuario.Email.ShouldBe(novoEmail);
        usuario.UpdatedAt.ShouldNotBeNull();
        usuario.UpdatedBy.ShouldBe(atualizadoPor);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Email_Para_Valor_Obrigatorio_E_Nao_Informado()
    {
        // Arrange
        var usuario = CreateUser();
        Action acao = () => usuario.ChangeEmail(null!, Guid.NewGuid());

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Email.Required);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Email_Sem_Responsavel_Valido()
    {
        // Arrange
        var usuario = CreateUser();
        var novoEmail = Email.Create("novo@email.com");
        Action acao = () => usuario.ChangeEmail(novoEmail, Guid.Empty);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Fact]
    public void Deve_Alterar_Senha_Quando_PasswordHash_For_Valido()
    {
        // Arrange
        var atualizadoPor = Guid.NewGuid();
        var usuario = CreateUser();
        var novoPasswordHash = PasswordHash.Create("$2a$11$novohashfakeparatestes");

        // Act
        usuario.ChangePassword(novoPasswordHash, atualizadoPor);

        // Assert
        usuario.PasswordHash.ShouldBe(novoPasswordHash);
        usuario.UpdatedAt.ShouldNotBeNull();
        usuario.UpdatedBy.ShouldBe(atualizadoPor);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Senha_Para_PasswordHash_Obrigatorio_E_Nao_Informado()
    {
        // Arrange
        var usuario = CreateUser();
        Action acao = () => usuario.ChangePassword(null!, Guid.NewGuid());

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.PasswordHash.Required);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Senha_Sem_Responsavel_Valido()
    {
        // Arrange
        var usuario = CreateUser();
        var novoPasswordHash = PasswordHash.Create("$2a$11$novohashfakeparatestes");
        Action acao = () => usuario.ChangePassword(novoPasswordHash, Guid.Empty);

        // Act
        var excecao = Should.Throw<ArgumentException>(acao);

        // Assert
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Fact]
    public void Deve_Atualizar_Perfil_Quando_Nome_E_Email_Forem_Validos()
    {
        // Arrange
        var atualizadoPor = Guid.NewGuid();
        var usuario = CreateUser(name: "Maicon Alves");
        var novoEmail = Email.Create("maicon.guedes@email.com");

        // Act
        usuario.UpdateProfile("Maicon Guedes", novoEmail, atualizadoPor);

        // Assert
        usuario.Name.ShouldBe("Maicon Guedes");
        usuario.Email.ShouldBe(novoEmail);
        usuario.Cpf.ShouldBe(Cpf.Create("529.982.247-25"));
        usuario.BirthDate.ShouldBe(new DateOnly(1993, 6, 17));
        usuario.UpdatedAt.ShouldNotBeNull();
        usuario.UpdatedBy.ShouldBe(atualizadoPor);
    }

    [Fact]
    public void Deve_Atualizar_Perfil_Com_Cpf_E_Data_De_Nascimento_Quando_Dados_Forem_Validos()
    {
        // Arrange
        var atualizadoPor = Guid.NewGuid();
        var usuario = CreateUser(name: "Maicon Alves");
        var novoEmail = Email.Create("maicon.guedes@email.com");
        var cpf = Cpf.Create("286.255.878-87");

        // Act
        usuario.UpdateProfile("Maicon Guedes", novoEmail, cpf, new DateOnly(1991, 2, 3), atualizadoPor);

        // Assert
        usuario.Name.ShouldBe("Maicon Guedes");
        usuario.Email.ShouldBe(novoEmail);
        usuario.Cpf.ShouldBe(cpf);
        usuario.BirthDate.ShouldBe(new DateOnly(1991, 2, 3));
        usuario.UpdatedAt.ShouldNotBeNull();
        usuario.UpdatedBy.ShouldBe(atualizadoPor);
    }

    [Fact]
    public void Deve_Validar_Dados_De_Recuperacao_Quando_Cpf_E_Data_Corresponderem()
    {
        // Arrange
        var usuario = CreateUser();

        // Act
        var matches = usuario.MatchesRecoveryData(Cpf.Create("52998224725"), new DateOnly(1993, 6, 17));

        // Assert
        matches.ShouldBeTrue();
    }

    private static User CreateUser(
        string name = "Maicon Guedes",
        string emailValue = "maicon@email.com",
        string cpfValue = "529.982.247-25",
        DateOnly? birthDate = null,
        Guid? createdBy = null)
    {
        var email = Email.Create(emailValue);
        var cpf = Cpf.Create(cpfValue);
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        return User.Create(
            name,
            email,
            cpf,
            birthDate ?? new DateOnly(1993, 6, 17),
            passwordHash,
            createdBy);
    }
}

