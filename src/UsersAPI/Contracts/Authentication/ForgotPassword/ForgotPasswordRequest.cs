using System.ComponentModel.DataAnnotations;
using UsersAPI.Api.Common;

namespace UsersAPI.Api.Contracts.Authentication.ForgotPassword
{
    public sealed record ForgotPasswordRequest(
        [Required(ErrorMessage = ApiMessages.User.EmailRequired)]
        string Email,
        [Required(ErrorMessage = ApiMessages.User.CpfRequired)]
        string Cpf,
        [Required(ErrorMessage = ApiMessages.User.BirthDateRequired)]
        DateOnly? BirthDate,
        [Required(ErrorMessage = ApiMessages.User.NewPasswordRequired)]
        string NewPassword,
        [Required(ErrorMessage = ApiMessages.User.ConfirmNewPasswordRequired)]
        string ConfirmNewPassword);
}
