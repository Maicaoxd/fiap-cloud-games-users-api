namespace UsersAPI.Application.Users.Deactivate
{
    public sealed record DeactivateUserCommand(
        Guid UserId,
        Guid DeactivatedBy);
}
