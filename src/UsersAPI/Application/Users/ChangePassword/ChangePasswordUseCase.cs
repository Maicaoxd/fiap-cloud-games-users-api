using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Abstractions.Security;
using UsersAPI.Application.Common;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Application.Users.ChangePassword
{
    public sealed class ChangePasswordUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public ChangePasswordUseCase(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task ExecuteAsync(
            ChangePasswordCommand command,
            CancellationToken cancellationToken = default)
        {
            EnsurePasswordsMatch(command.NewPassword, command.ConfirmNewPassword);

            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

            user = ResolveExistingUser(user);
            EnsureUserIsActive(user);
            EnsureCurrentPasswordIsValid(command.CurrentPassword, user.PasswordHash);

            var newPassword = Password.Create(command.NewPassword);
            var newPasswordHash = _passwordHasher.Hash(newPassword);

            user.ChangePassword(newPasswordHash, command.UserId);

            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        private static void EnsurePasswordsMatch(string newPassword, string confirmNewPassword)
        {
            if (newPassword != confirmNewPassword)
                throw new ArgumentException(ApplicationMessages.User.PasswordConfirmationDoesNotMatch);
        }

        private User ResolveExistingUser(User? user)
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

        private void EnsureCurrentPasswordIsValid(string currentPassword, PasswordHash passwordHash)
        {
            if (!_passwordHasher.Verify(currentPassword, passwordHash))
                throw new InvalidCredentialsException();
        }
    }
}
