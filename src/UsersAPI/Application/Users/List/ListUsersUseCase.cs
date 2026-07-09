using UsersAPI.Application.Abstractions.Persistence;

namespace UsersAPI.Application.Users.List
{
    public sealed class ListUsersUseCase
    {
        private readonly IUserRepository _userRepository;

        public ListUsersUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IReadOnlyCollection<ListUserResult>> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            var users = await _userRepository.ListAsync(cancellationToken);

            return users
                .Select(user => new ListUserResult(
                    user.Id,
                    user.Name,
                    user.Email.Value,
                    user.Cpf.Value,
                    user.BirthDate,
                    user.Role.ToString(),
                    user.IsActive))
                .ToList();
        }
    }
}
