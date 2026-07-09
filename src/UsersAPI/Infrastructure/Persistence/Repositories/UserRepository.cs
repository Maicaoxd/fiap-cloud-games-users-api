using UsersAPI.Application.Abstractions.Persistence;
using UsersAPI.Application.Common.Exceptions;
using UsersAPI.Domain.Users;
using UsersAPI.Domain.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace UsersAPI.Infrastructure.Persistence.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private const string UniqueEmailIndexName = "IX_Users_Email";
        private const string UniqueCpfIndexName = "IX_Users_Cpf";

        private readonly UsersDbContext _dbContext;

        public UserRepository(UsersDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .AnyAsync(user => user.Email == email, cancellationToken);
        }

        public async Task<bool> ExistsByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .AnyAsync(user => user.Cpf == cpf, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
        }

        public async Task<User?> GetByCpfAsync(Cpf cpf, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .SingleOrDefaultAsync(user => user.Cpf == cpf, cancellationToken);
        }

        public async Task<User?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
        }

        public async Task<IReadOnlyCollection<User>> ListAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .OrderBy(user => user.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _dbContext.Users.AddAsync(user, cancellationToken);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (
                SqlServerUniqueConstraintDetector.IsUniqueConstraintViolation(
                    exception,
                    UniqueEmailIndexName))
            {
                throw new EmailAlreadyRegisteredException();
            }
            catch (DbUpdateException exception) when (
                SqlServerUniqueConstraintDetector.IsUniqueConstraintViolation(
                    exception,
                    UniqueCpfIndexName))
            {
                throw new CpfAlreadyRegisteredException();
            }
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _dbContext.Users.Update(user);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (
                SqlServerUniqueConstraintDetector.IsUniqueConstraintViolation(
                    exception,
                    UniqueEmailIndexName))
            {
                throw new EmailAlreadyRegisteredException();
            }
            catch (DbUpdateException exception) when (
                SqlServerUniqueConstraintDetector.IsUniqueConstraintViolation(
                    exception,
                    UniqueCpfIndexName))
            {
                throw new CpfAlreadyRegisteredException();
            }
        }
    }
}

