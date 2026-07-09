using UsersAPI.Application.Abstractions.Messaging;
using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Abstractions.Security;
using UsersAPI.Application.Common;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Application.Users.Register
{
    public sealed class RegisterUserUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserCreatedEventPublisher _userCreatedEventPublisher;

        public RegisterUserUseCase(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUserCreatedEventPublisher userCreatedEventPublisher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _userCreatedEventPublisher = userCreatedEventPublisher;
        }

        public async Task<RegisterUserResult> ExecuteAsync(
            RegisterUserCommand command,
            CancellationToken cancellationToken = default)
        {
            EnsurePasswordsMatch(command.Password, command.ConfirmPassword);

            var email = Email.Create(command.Email);
            var cpf = Cpf.Create(command.Cpf);
            var password = Password.Create(command.Password);

            if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
                throw new EmailAlreadyRegisteredException();

            if (await _userRepository.ExistsByCpfAsync(cpf, cancellationToken))
                throw new CpfAlreadyRegisteredException();

            var passwordHash = _passwordHasher.Hash(password);
            var user = User.Create(command.Name, email, cpf, command.BirthDate, passwordHash);

            await _userRepository.AddAsync(user, cancellationToken);
            await _userCreatedEventPublisher.PublishAsync(user, cancellationToken);

            return new RegisterUserResult(user.Id);
        }

        private static void EnsurePasswordsMatch(string password, string confirmPassword)
        {
            if (password != confirmPassword)
                throw new ArgumentException(ApplicationMessages.User.PasswordConfirmationDoesNotMatch);
        }
    }
}
