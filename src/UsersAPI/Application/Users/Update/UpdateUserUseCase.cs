using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Application.Users.Update
{
    public sealed class UpdateUserUseCase
    {
        private readonly IUserRepository _userRepository;

        public UpdateUserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task ExecuteAsync(
            UpdateUserCommand command,
            CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);

            if (user is null)
                throw new UserNotFoundException();

            var email = Email.Create(command.Email);
            var cpf = Cpf.Create(command.Cpf);
            var userWithSameEmail = await _userRepository.GetByEmailAsync(email, cancellationToken);
            var userWithSameCpf = await _userRepository.GetByCpfAsync(cpf, cancellationToken);

            if (userWithSameEmail is not null && userWithSameEmail.Id != user.Id)
                throw new EmailAlreadyRegisteredException();

            if (userWithSameCpf is not null && userWithSameCpf.Id != user.Id)
                throw new CpfAlreadyRegisteredException();

            user.UpdateProfile(command.Name, email, cpf, command.BirthDate, command.UpdatedBy);

            await _userRepository.UpdateAsync(user, cancellationToken);
        }
    }
}
