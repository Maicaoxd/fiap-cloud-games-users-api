using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Application.Abstractions.Persistence
{
    public interface IUserRepository
    {
        Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

        Task<bool> ExistsByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default);

        Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

        Task<User?> GetByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default);

        Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken = default);

        Task AddAsync(User user, CancellationToken cancellationToken = default);

        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
    }
}
