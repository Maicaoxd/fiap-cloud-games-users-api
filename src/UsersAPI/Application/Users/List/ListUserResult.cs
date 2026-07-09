namespace UsersAPI.Application.Users.List
{
    public sealed record ListUserResult(
        Guid UserId,
        string Name,
        string Email,
        string Cpf,
        DateOnly BirthDate,
        string Role,
        bool IsActive);
}
