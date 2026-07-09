using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UsersAPI.Api.Controllers;
using UsersAPI.Api.Common;
using UsersAPI.Api.Contracts.Users.ChangePassword;
using UsersAPI.Api.Contracts.Users.List;
using UsersAPI.Api.Contracts.Users.Register;
using UsersAPI.Api.Contracts.Users.Update;
using UsersAPI.Api.Contracts.Users.UpdateCurrent;
using UsersAPI.Application.Abstractions.Messaging;
using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Abstractions.Security;
using UsersAPI.Application.Users.ChangePassword;
using UsersAPI.Application.Users.Deactivate;
using UsersAPI.Application.Users.List;
using UsersAPI.Application.Users.Register;
using UsersAPI.Application.Users.Update;
using UsersAPI.Application.Users.UpdateCurrent;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;
using UsersAPI.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace UsersAPI.Tests.Api.Controllers;

[Trait("Category", "Unit")]
public sealed class UsersControllerTests
{
    [Fact]
    public async Task RegisterAsync_QuandoDadosForemValidos_DeveRetornarCreatedComUserId()
    {
        // Arrange
        var birthDate = new DateOnly(1993, 6, 17);
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var deactivateUseCase = new DeactivateUserUseCase(userRepository);
        var registerUseCase = new RegisterUserUseCase(userRepository, passwordHasher, Substitute.For<IUserCreatedEventPublisher>());
        var changePasswordUseCase = new ChangePasswordUseCase(userRepository, passwordHasher);
        var listUsersUseCase = new ListUsersUseCase(userRepository);
        var updateUserUseCase = new UpdateUserUseCase(userRepository);
        var updateCurrentUserUseCase = new UpdateCurrentUserUseCase(userRepository);

        userRepository
            .ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);
        userRepository
            .ExistsByCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>())
            .Returns(false);
        passwordHasher.Hash(Arg.Any<Password>()).Returns(passwordHash);

        var controller = new UsersController(
            deactivateUseCase,
            registerUseCase,
            changePasswordUseCase,
            listUsersUseCase,
            updateUserUseCase,
            updateCurrentUserUseCase);
        var request = new RegisterUserRequest(
            "Maicon Guedes",
            "maicon@email.com",
            "529.982.247-25",
            birthDate,
            "Senha@123",
            "Senha@123");

        // Act
        var actionResult = await controller.RegisterAsync(request, CancellationToken.None);

        // Assert
        var createdResult = actionResult.Result.ShouldBeOfType<CreatedResult>();
        var response = createdResult.Value.ShouldBeOfType<RegisterUserResponse>();

        response.UserId.ShouldNotBe(Guid.Empty);
        createdResult.Location.ShouldBe($"/api/users/{response.UserId}");
        await userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateMeAsync_QuandoUsuarioAutenticadoERequestValido_DeveRetornarNoContent()
    {
        // Arrange
        var birthDate = new DateOnly(1994, 7, 18);
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        userRepository
            .GetByEmailAsync(Email.Create("maicon.guedes@email.com"), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var controller = CreateController(userRepository, passwordHasher, user.Id, nameof(UserRole.User));
        var request = new UpdateCurrentUserRequest(
            "Maicon Guedes",
            "maicon.guedes@email.com",
            birthDate);

        // Act
        var actionResult = await controller.UpdateMeAsync(request, CancellationToken.None);

        // Assert
        actionResult.ShouldBeOfType<NoContentResult>();
        user.Name.ShouldBe("Maicon Guedes");
        user.Email.ShouldBe(Email.Create("maicon.guedes@email.com"));
        user.Cpf.ShouldBe(Cpf.Create("529.982.247-25"));
        user.BirthDate.ShouldBe(birthDate);
        await userRepository.DidNotReceive().GetByCpfAsync(Arg.Any<Cpf>(), Arg.Any<CancellationToken>());
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ChangePasswordAsync_QuandoUsuarioAutenticadoERequestValido_DeveRetornarNoContent()
    {
        // Arrange
        var user = CreateUser();
        var newPasswordHash = PasswordHash.Create("$2a$11$novohashfakeparatestes");
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        passwordHasher
            .Verify("Senha@123", user.PasswordHash)
            .Returns(true);
        passwordHasher
            .Hash(Arg.Any<Password>())
            .Returns(newPasswordHash);

        var controller = CreateController(userRepository, passwordHasher, user.Id, nameof(UserRole.User));
        var request = new ChangePasswordRequest(
            "Senha@123",
            "NovaSenha@123",
            "NovaSenha@123");

        // Act
        var actionResult = await controller.ChangePasswordAsync(request, CancellationToken.None);

        // Assert
        actionResult.ShouldBeOfType<NoContentResult>();
        user.PasswordHash.ShouldBe(newPasswordHash);
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_QuandoAdminAutenticadoERequestValido_DeveRetornarNoContent()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var birthDate = new DateOnly(1993, 6, 17);
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);
        userRepository
            .GetByEmailAsync(Email.Create("maicon.guedes@email.com"), Arg.Any<CancellationToken>())
            .Returns((User?)null);
        userRepository
            .GetByCpfAsync(Cpf.Create("529.982.247-25"), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var controller = CreateController(userRepository, passwordHasher, adminId);
        var request = new UpdateUserRequest(
            "Maicon Guedes",
            "maicon.guedes@email.com",
            "529.982.247-25",
            birthDate);

        // Act
        var actionResult = await controller.UpdateAsync(user.Id, request, CancellationToken.None);

        // Assert
        actionResult.ShouldBeOfType<NoContentResult>();
        user.Name.ShouldBe("Maicon Guedes");
        user.Email.ShouldBe(Email.Create("maicon.guedes@email.com"));
        user.Cpf.ShouldBe(Cpf.Create("529.982.247-25"));
        user.BirthDate.ShouldBe(birthDate);
        user.UpdatedBy.ShouldBe(adminId);
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateAsync_QuandoAdminAutenticadoEUsuarioExistir_DeveRetornarNoContent()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var user = CreateUser();
        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .GetByIdAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(user);

        var controller = CreateController(userRepository, passwordHasher, adminId);

        // Act
        var actionResult = await controller.DeactivateAsync(user.Id, CancellationToken.None);

        // Assert
        actionResult.ShouldBeOfType<NoContentResult>();
        user.IsActive.ShouldBeFalse();
        user.UpdatedBy.ShouldBe(adminId);
        await userRepository.Received(1).UpdateAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListAsync_QuandoAdminAutenticado_DeveRetornarOkComUsuarios()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var firstUser = CreateUser();
        var secondUser = CreateUser("Ana Guedes", "ana@email.com", "168.995.350-09");
        secondUser.Deactivate(adminId);

        var userRepository = Substitute.For<IUserRepository>();
        var passwordHasher = Substitute.For<IPasswordHasher>();

        userRepository
            .ListAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { firstUser, secondUser });

        var controller = CreateController(userRepository, passwordHasher, adminId);

        // Act
        var actionResult = await controller.ListAsync(CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.ShouldBeOfType<OkObjectResult>();
        var response = okResult.Value.ShouldBeOfType<List<ListUserResponse>>();

        response.Count.ShouldBe(2);
        response.ShouldContain(user =>
            user.UserId == firstUser.Id &&
            user.Name == "Maicon Guedes" &&
            user.Email == "maicon@email.com" &&
            user.Cpf == "52998224725" &&
            user.BirthDate == new DateOnly(1993, 6, 17) &&
            user.Role == nameof(UserRole.User) &&
            user.IsActive);
        response.ShouldContain(user =>
            user.UserId == secondUser.Id &&
            user.Name == "Ana Guedes" &&
            user.Email == "ana@email.com" &&
            user.Cpf == "16899535009" &&
            user.BirthDate == new DateOnly(1993, 6, 17) &&
            user.Role == nameof(UserRole.User) &&
            !user.IsActive);
        await userRepository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void ListAsync_DeveExigirRoleAdministrator()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.ListAsync));

        // Act
        var authorizeAttribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        authorizeAttribute.Roles.ShouldBe(nameof(UserRole.Administrator));
    }

    [Fact]
    public void UpdateMeAsync_DeveExigirUsuarioAutenticado()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.UpdateMeAsync));

        // Act
        var authorizeAttribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        authorizeAttribute.Roles.ShouldBeNull();
    }

    [Fact]
    public void ChangePasswordAsync_DeveExigirUsuarioAutenticado()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.ChangePasswordAsync));

        // Act
        var authorizeAttribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        authorizeAttribute.Roles.ShouldBeNull();
    }

    [Fact]
    public void UpdateAsync_DeveExigirRoleAdministrator()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.UpdateAsync));

        // Act
        var authorizeAttribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        authorizeAttribute.Roles.ShouldBe(nameof(UserRole.Administrator));
    }

    [Fact]
    public void DeactivateAsync_DeveExigirRoleAdministrator()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.DeactivateAsync));

        // Act
        var authorizeAttribute = method!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>()
            .Single();

        // Assert
        authorizeAttribute.Roles.ShouldBe(nameof(UserRole.Administrator));
    }

    [Fact]
    public void UpdateMeAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.UpdateMeAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status204NoContent].Type.ShouldBe(typeof(void));
        responseTypes[StatusCodes.Status400BadRequest].Type.ShouldBe(typeof(ValidationProblemDetails));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status409Conflict].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    [Fact]
    public void ChangePasswordAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.ChangePasswordAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status204NoContent].Type.ShouldBe(typeof(void));
        responseTypes[StatusCodes.Status400BadRequest].Type.ShouldBe(typeof(ValidationProblemDetails));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    [Fact]
    public void UpdateAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.UpdateAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status204NoContent].Type.ShouldBe(typeof(void));
        responseTypes[StatusCodes.Status400BadRequest].Type.ShouldBe(typeof(ValidationProblemDetails));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status404NotFound].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status409Conflict].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    [Fact]
    public void DeactivateAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.DeactivateAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status204NoContent].Type.ShouldBe(typeof(void));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status404NotFound].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    [Fact]
    public void ListAsync_DeveDocumentarRespostasEsperadasNoSwagger()
    {
        // Arrange
        var method = typeof(UsersController)
            .GetMethod(nameof(UsersController.ListAsync));

        // Act
        var responseTypes = method!
            .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), inherit: false)
            .Cast<ProducesResponseTypeAttribute>()
            .ToDictionary(attribute => attribute.StatusCode);

        // Assert
        responseTypes[StatusCodes.Status200OK].Type.ShouldBe(typeof(IReadOnlyCollection<ListUserResponse>));
        responseTypes[StatusCodes.Status401Unauthorized].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status403Forbidden].Type.ShouldBe(typeof(ProblemDetails));
        responseTypes[StatusCodes.Status500InternalServerError].Type.ShouldBe(typeof(ProblemDetails));
    }

    private static UsersController CreateController(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        Guid userId,
        string role = nameof(UserRole.Administrator))
    {
        var deactivateUseCase = new DeactivateUserUseCase(userRepository);
        var registerUseCase = new RegisterUserUseCase(userRepository, passwordHasher, Substitute.For<IUserCreatedEventPublisher>());
        var changePasswordUseCase = new ChangePasswordUseCase(userRepository, passwordHasher);
        var listUsersUseCase = new ListUsersUseCase(userRepository);
        var updateUserUseCase = new UpdateUserUseCase(userRepository);
        var updateCurrentUserUseCase = new UpdateCurrentUserUseCase(userRepository);

        return new UsersController(
            deactivateUseCase,
            registerUseCase,
            changePasswordUseCase,
            listUsersUseCase,
            updateUserUseCase,
            updateCurrentUserUseCase)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = CreateHttpContext(userId, role)
            }
        };
    }

    private static DefaultHttpContext CreateHttpContext(Guid userId, string role = nameof(UserRole.Administrator))
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtClaimNames.Role, role)
        };

        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };
    }

    private static User CreateUser(
        string name = "Maicon Guedes",
        string email = "maicon@email.com",
        string cpf = "529.982.247-25")
    {
        var emailValueObject = Email.Create(email);
        var cpfValueObject = Cpf.Create(cpf);
        var passwordHash = PasswordHash.Create("$2a$11$hashfakeparatestes");

        return User.Create(name, emailValueObject, cpfValueObject, new DateOnly(1993, 6, 17), passwordHash);
    }
}


