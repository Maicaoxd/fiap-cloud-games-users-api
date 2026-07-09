namespace UsersAPI.Application.Users.Update
{
    public sealed record UpdateUserCommand(
        Guid UserId,
        string Name,
        string Email,
        string Cpf,
        DateOnly BirthDate,
        Guid UpdatedBy);
}
