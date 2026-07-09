using Microsoft.EntityFrameworkCore;
using UsersAPI.Infrastructure.Persistence;

namespace UsersAPI.Health
{
    public interface IDatabaseHealthChecker
    {
        Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
    }

    public sealed class DatabaseHealthChecker : IDatabaseHealthChecker
    {
        private readonly UsersDbContext _dbContext;

        public DatabaseHealthChecker(UsersDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        {
            return _dbContext.Database.CanConnectAsync(cancellationToken);
        }
    }
}
