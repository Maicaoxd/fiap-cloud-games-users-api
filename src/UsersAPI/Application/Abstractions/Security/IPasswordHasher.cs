using UsersAPI.Domain.Users.ValueObjects;

namespace UsersAPI.Application.Abstractions.Security
{
    public interface IPasswordHasher
    {
        PasswordHash Hash(Password password);

        bool Verify(string? password, PasswordHash passwordHash);
    }
}
