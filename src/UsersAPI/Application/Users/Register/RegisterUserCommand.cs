namespace UsersAPI.Application.Users.Register
{
    public sealed record RegisterUserCommand(
        string Name,
        string Email,
        string Cpf,
        DateOnly BirthDate,
        string Password,
        string ConfirmPassword);
}
