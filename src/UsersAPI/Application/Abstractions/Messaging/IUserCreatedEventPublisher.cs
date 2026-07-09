using UsersAPI.Domain.Users;

namespace UsersAPI.Application.Abstractions.Messaging
{
    public interface IUserCreatedEventPublisher
    {
        Task PublishAsync(User user, CancellationToken cancellationToken = default);
    }
}
