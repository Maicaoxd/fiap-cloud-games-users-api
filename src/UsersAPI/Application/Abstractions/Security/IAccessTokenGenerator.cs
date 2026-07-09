using UsersAPI.Domain.Users;

namespace UsersAPI.Application.Abstractions.Security
{
    public interface IAccessTokenGenerator
    {
        string Generate(User user);
    }
}
