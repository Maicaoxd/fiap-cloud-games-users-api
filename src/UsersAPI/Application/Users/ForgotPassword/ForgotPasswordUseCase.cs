using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Abstractions.Security;
using UsersAPI.Application.Common;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Application.Users.ForgotPassword
{
    public sealed class ForgotPasswordUseCase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public ForgotPasswordUseCase(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task ExecuteAsync(
            ForgotPasswordCommand command,
            CancellationToken cancellationToken = default)
        {
            EnsurePasswordsMatch(command.NewPassword, command.ConfirmNewPassword);

            var email = Email.Create(command.Email);
            var cpf = Cpf.Create(command.Cpf);
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

            EnsureRecoveryDataIsValid(user, cpf, command.BirthDate);

            var newPassword = Password.Create(command.NewPassword);
            var newPasswordHash = _passwordHasher.Hash(newPassword);

            user!.ChangePassword(newPasswordHash, user.Id);

            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        private static void EnsurePasswordsMatch(string newPassword, string confirmNewPassword)
        {
            if (newPassword != confirmNewPassword)
                throw new ArgumentException(ApplicationMessages.User.PasswordConfirmationDoesNotMatch);
        }

        private static void EnsureRecoveryDataIsValid(
            Domain.Users.User? user,
            Cpf cpf,
            DateOnly birthDate)
        {
            if (user is null || !user.IsActive || !user.MatchesRecoveryData(cpf, birthDate))
                throw new InvalidPasswordRecoveryDataException();
        }
    }
}
