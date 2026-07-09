using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Common.Exceptions;

namespace UsersAPI.Application.Users.Deactivate
{
    public sealed class DeactivateUserUseCase
    {
        private readonly IUserRepository _userRepository;

        public DeactivateUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ExecuteAsync(
            DeactivateUserCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

            if (user is null)
                throw new UserNotFoundException();

            if (!user.IsActive)
                return;

            user.Deactivate(command.DeactivatedBy);

            await _userRepository.UpdateAsync(user, cancellationToken);
        }
    }
}
