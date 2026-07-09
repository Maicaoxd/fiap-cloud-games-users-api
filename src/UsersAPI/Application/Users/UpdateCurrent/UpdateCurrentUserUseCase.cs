using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Application.Users.UpdateCurrent
{
    public sealed class UpdateCurrentUserUseCase
    {
        private readonly IUserRepository _userRepository;

        public UpdateCurrentUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ExecuteAsync(
            UpdateCurrentUserCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

            user = ResolveExistingUser(user);
            EnsureUserIsActive(user);

            var email = Email.Create(command.Email);
            var userWithSameEmail = await _userRepository.GetByEmailAsync(email, cancellationToken);

            if (userWithSameEmail is not null && userWithSameEmail.Id != user.Id)
                throw new EmailAlreadyRegisteredException();

            user.UpdateProfile(command.Name, email, user.Cpf, command.BirthDate, command.UserId);

            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        private static User ResolveExistingUser(User? user)
        {
            if (user is null)
                throw new InvalidCredentialsException();

            return user;
        }

        private static void EnsureUserIsActive(User user)
        {
            if (!user.IsActive)
                throw new InactiveUserException();
        }
    }
}
