using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Abstractions.Security;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Application.Users.Authenticate
{
    public sealed class AuthenticateUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAccessTokenGenerator _accessTokenGenerator;

        public AuthenticateUserUseCase(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IAccessTokenGenerator accessTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _accessTokenGenerator = accessTokenGenerator;
        }

        public async Task<AuthenticateUserResult> ExecuteAsync(
            AuthenticateUserCommand command,
            CancellationToken cancellationToken = default)
        {
            var email = Email.Create(command.Email);
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            user = ResolveExistingUser(user);
            EnsureCredentialsAreValid(command.Password, user.PasswordHash);
            EnsureAccountIsActive(user);

            var accessToken = _accessTokenGenerator.Generate(user);

            return new AuthenticateUserResult(accessToken);
        }

        private static User ResolveExistingUser(User? user)
        {
            if (user is null)
                throw new InvalidCredentialsException();

            return user;
        }

        private void EnsureCredentialsAreValid(string? password, PasswordHash passwordHash)
        {
            if (!_passwordHasher.Verify(password, passwordHash))
                throw new InvalidCredentialsException();
        }

        private static void EnsureAccountIsActive(User user)
        {
            if (!user.IsActive)
                throw new InactiveUserException();
        }
    }
}
