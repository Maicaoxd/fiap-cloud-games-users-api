using System.ComponentModel.DataAnnotations;
using UsersAPI.Api.Common;

namespace UsersAPI.Api.Contracts.Users.ChangePassword
{
    public sealed record ChangePasswordRequest(
        [Required(ErrorMessage = ApiMessages.User.CurrentPasswordRequired)]
        string CurrentPassword,
        [Required(ErrorMessage = ApiMessages.User.NewPasswordRequired)]
        string NewPassword,
        [Required(ErrorMessage = ApiMessages.User.ConfirmNewPasswordRequired)]
        string ConfirmNewPassword);
}
