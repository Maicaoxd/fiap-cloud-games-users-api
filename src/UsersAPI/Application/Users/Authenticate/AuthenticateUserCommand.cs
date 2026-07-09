namespace UsersAPI.Application.Users.Authenticate
{
    public sealed record AuthenticateUserCommand(
        string Email,
        string Password);
}
