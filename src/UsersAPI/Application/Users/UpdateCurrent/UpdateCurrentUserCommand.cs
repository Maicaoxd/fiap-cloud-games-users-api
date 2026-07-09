namespace UsersAPI.Application.Users.UpdateCurrent
{
    public sealed record UpdateCurrentUserCommand(
        Guid UserId,
        string Name,
        string Email,
        DateOnly BirthDate);
}
