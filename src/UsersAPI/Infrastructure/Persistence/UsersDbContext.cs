using UsersAPI.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace UsersAPI.Infrastructure.Persistence
{
    public sealed class UsersDbContext : DbContext
    {
        public UsersDbContext(DbContextOptions<UsersDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsersDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
