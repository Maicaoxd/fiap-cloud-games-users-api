namespace UsersAPI.Application.Users.ForgotPassword
{
    public sealed record ForgotPasswordCommand(
        string Email,
        string Cpf,
        DateOnly BirthDate,
        string NewPassword,
        string ConfirmNewPassword);
}
