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
        // Preparação
        const string nome = "Maicon Guedes";
        var email = Email.Create("maicon@email.com");
        var cpf = Cpf.Create("529.982.247-25");
        var birthDate = new DateOnly(1993, 6, 17);
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        // Execução
        var usuario = User.Create(nome, email, cpf, birthDate, passwordHash);

        // Verificação
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
        // Preparação
        var criadoPor = Guid.NewGuid();

        // Execução
        var usuario = CreateUser(createdBy: criadoPor);

        // Verificação
        usuario.CreatedBy.ShouldBe(criadoPor);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Criar_Usuario_Com_Responsavel_Invalido()
    {
        // Preparação
        Action acao = () => CreateUser(createdBy: Guid.Empty);

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Deve_Lancar_Excecao_Quando_Nome_For_Obrigatorio_E_Nao_For_Informado(string? nome)
    {
        // Preparação
        Action acao = () => CreateUser(name: nome!);

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.User.NameRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Email_For_Obrigatorio_E_Nao_For_Informado()
    {
        // Preparação
        Action acao = () => User.Create(
            "Maicon Guedes",
            null!,
            Cpf.Create("529.982.247-25"),
            new DateOnly(1993, 6, 17),
            PasswordHash.Create("$2a$11$hashfakeparatestes"));

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.Email.Required);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Cpf_For_Obrigatorio_E_Nao_For_Informado()
    {
        // Preparação
        Action acao = () => User.Create(
            "Maicon Guedes",
            Email.Create("maicon@email.com"),
            null!,
            new DateOnly(1993, 6, 17),
            PasswordHash.Create("$2a$11$hashfakeparatestes"));

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.Cpf.Required);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Data_De_Nascimento_For_Obrigatoria_E_Nao_For_Informada()
    {
        // Preparação
        Action acao = () => User.Create(
            "Maicon Guedes",
            Email.Create("maicon@email.com"),
            Cpf.Create("529.982.247-25"),
            default,
            PasswordHash.Create("$2a$11$hashfakeparatestes"));

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.User.BirthDateRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_PasswordHash_For_Obrigatorio_E_Nao_For_Informado()
    {
        // Preparação
        Action acao = () => User.Create(
            "Maicon Guedes",
            Email.Create("maicon@email.com"),
            Cpf.Create("529.982.247-25"),
            new DateOnly(1993, 6, 17),
            null!);

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.PasswordHash.Required);
    }

    [Fact]
    public void Deve_Desativar_Usuario_Quando_Usuario_Estiver_Ativo()
    {
        // Preparação
        var desativadoPor = Guid.NewGuid();
        var usuario = CreateUser();

        // Execução
        usuario.Deactivate(desativadoPor);

        // Verificação
        usuario.IsActive.ShouldBeFalse();
        usuario.UpdatedAt.ShouldNotBeNull();
        usuario.UpdatedBy.ShouldBe(desativadoPor);
    }

    [Fact]
    public void Deve_Reativar_Usuario_Quando_Usuario_Estiver_Inativo()
    {
        // Preparação
        var desativadoPor = Guid.NewGuid();
        var ativadoPor = Guid.NewGuid();
        var usuario = CreateUser();
        usuario.Deactivate(desativadoPor);

        // Execução
        usuario.Activate(ativadoPor);

        // Verificação
        usuario.IsActive.ShouldBeTrue();
        usuario.UpdatedAt.ShouldNotBeNull();
        usuario.UpdatedBy.ShouldBe(ativadoPor);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Desativar_Usuario_Sem_Responsavel_Valido()
    {
        // Preparação
        var usuario = CreateUser();
        Action acao = () => usuario.Deactivate(Guid.Empty);

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Reativar_Usuario_Sem_Responsavel_Valido()
    {
        // Preparação
        var usuario = CreateUser();
        usuario.Deactivate(Guid.NewGuid());
        Action acao = () => usuario.Activate(Guid.Empty);

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Fact]
    public void Deve_Alterar_Nome_Quando_Nome_For_Valido()
    {
        // Preparação
        var atualizadoPor = Guid.NewGuid();
        var usuario = CreateUser(name: "Maicon Alves");

        // Execução
        usuario.ChangeName("Maicon Guedes", atualizadoPor);

        // Verificação
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
        // Preparação
        var usuario = CreateUser();
        Action acao = () => usuario.ChangeName(nome!, Guid.NewGuid());

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.User.NameRequired);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Nome_Sem_Responsavel_Valido()
    {
        // Preparação
        var usuario = CreateUser();
        Action acao = () => usuario.ChangeName("Maicon Guedes", Guid.Empty);

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Fact]
    public void Deve_Alterar_Email_Quando_Email_For_Valido()
    {
        // Preparação
        var atualizadoPor = Guid.NewGuid();
        var usuario = CreateUser();
        var novoEmail = Email.Create("novo@email.com");

        // Execução
        usuario.ChangeEmail(novoEmail, atualizadoPor);

        // Verificação
        usuario.Email.ShouldBe(novoEmail);
        usuario.UpdatedAt.ShouldNotBeNull();
        usuario.UpdatedBy.ShouldBe(atualizadoPor);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Email_Para_Valor_Obrigatorio_E_Nao_Informado()
    {
        // Preparação
        var usuario = CreateUser();
        Action acao = () => usuario.ChangeEmail(null!, Guid.NewGuid());

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.Email.Required);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Email_Sem_Responsavel_Valido()
    {
        // Preparação
        var usuario = CreateUser();
        var novoEmail = Email.Create("novo@email.com");
        Action acao = () => usuario.ChangeEmail(novoEmail, Guid.Empty);

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Fact]
    public void Deve_Alterar_Senha_Quando_PasswordHash_For_Valido()
    {
        // Preparação
        var atualizadoPor = Guid.NewGuid();
        var usuario = CreateUser();
        var novoPasswordHash = PasswordHash.Create("$2a$11$novohashfakeparatestes");

        // Execução
        usuario.ChangePassword(novoPasswordHash, atualizadoPor);

        // Verificação
        usuario.PasswordHash.ShouldBe(novoPasswordHash);
        usuario.UpdatedAt.ShouldNotBeNull();
        usuario.UpdatedBy.ShouldBe(atualizadoPor);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Senha_Para_PasswordHash_Obrigatorio_E_Nao_Informado()
    {
        // Preparação
        var usuario = CreateUser();
        Action acao = () => usuario.ChangePassword(null!, Guid.NewGuid());

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.PasswordHash.Required);
    }

    [Fact]
    public void Deve_Lancar_Excecao_Quando_Alterar_Senha_Sem_Responsavel_Valido()
    {
        // Preparação
        var usuario = CreateUser();
        var novoPasswordHash = PasswordHash.Create("$2a$11$novohashfakeparatestes");
        Action acao = () => usuario.ChangePassword(novoPasswordHash, Guid.Empty);

        // Execução
        var excecao = Should.Throw<ArgumentException>(acao);

        // Verificação
        excecao.Message.ShouldBe(DomainMessages.Entity.ResponsibleForChangeRequired);
    }

    [Fact]
    public void Deve_Atualizar_Perfil_Quando_Nome_E_Email_Forem_Validos()
    {
        // Preparação
        var atualizadoPor = Guid.NewGuid();
        var usuario = CreateUser(name: "Maicon Alves");
        var novoEmail = Email.Create("maicon.guedes@email.com");

        // Execução
        usuario.UpdateProfile("Maicon Guedes", novoEmail, atualizadoPor);

        // Verificação
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
        // Preparação
        var atualizadoPor = Guid.NewGuid();
        var usuario = CreateUser(name: "Maicon Alves");
        var novoEmail = Email.Create("maicon.guedes@email.com");
        var cpf = Cpf.Create("286.255.878-87");

        // Execução
        usuario.UpdateProfile("Maicon Guedes", novoEmail, cpf, new DateOnly(1991, 2, 3), atualizadoPor);

        // Verificação
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
        // Preparação
        var usuario = CreateUser();

        // Execução
        var matches = usuario.MatchesRecoveryData(Cpf.Create("52998224725"), new DateOnly(1993, 6, 17));

        // Verificação
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
