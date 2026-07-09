using UsersAPI.Application.Abstractions.Security;
using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Infrastructure.Security
{
    public sealed class BCryptPasswordHasher : IPasswordHasher
    {
        public PasswordHash Hash(Password password)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password.Value);

            return PasswordHash.Create(hash);
        }

        public bool Verify(string? password, PasswordHash passwordHash)
        {
            if (password is null)
                return false;

            return BCrypt.Net.BCrypt.Verify(password, passwordHash.Value);
        }
    }
}
