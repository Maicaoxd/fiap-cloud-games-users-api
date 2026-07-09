namespace UsersAPI.Api.Contracts.Users.List
{
    public sealed record ListUserResponse(
        Guid UserId,
        string Name,
        string Email,
        string Cpf,
        DateOnly BirthDate,
        string Role,
        bool IsActive);
}
