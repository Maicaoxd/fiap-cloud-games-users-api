using UsersAPI.Api.Contracts.Events;
using UsersAPI.Application.Abstractions.Messaging;
using UsersAPI.Domain.Users;
using MassTransit;

namespace UsersAPI.Infrastructure.Messaging
{
    public sealed class MassTransitUserCreatedEventPublisher : IUserCreatedEventPublisher
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MassTransitUserCreatedEventPublisher(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public Task PublishAsync(User user, CancellationToken cancellationToken = default)
        {
            var @event = new UserCreatedEvent(
                user.Id,
                user.Name,
                user.Email.Value,
                user.CreatedAt);

            return _publishEndpoint.Publish(@event, cancellationToken);
        }
    }
}
