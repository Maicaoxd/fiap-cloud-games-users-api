using UsersAPI.Api.Contracts.Authentication.ForgotPassword;
using UsersAPI.Api.Contracts.Authentication.Login;
using UsersAPI.Application.Users.Authenticate;
using UsersAPI.Application.Users.ForgotPassword;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace UsersAPI.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthenticationController : ControllerBase
    {
        private readonly AuthenticateUserUseCase _authenticateUserUseCase;
        private readonly ForgotPasswordUseCase _forgotPasswordUseCase;

        public AuthenticationController(
            AuthenticateUserUseCase authenticateUserUseCase,
            ForgotPasswordUseCase forgotPasswordUseCase)
        {
            _authenticateUserUseCase = authenticateUserUseCase;
            _forgotPasswordUseCase = forgotPasswordUseCase;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken)
        {
            var command = new AuthenticateUserCommand(
                request.Email,
                request.Password);

            var result = await _authenticateUserUseCase.ExecuteAsync(command, cancellationToken);
            var response = new LoginResponse(result.AccessToken);

            return Ok(response);
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPasswordAsync(
            ForgotPasswordRequest request,
            CancellationToken cancellationToken)
        {
            var command = new ForgotPasswordCommand(
                request.Email,
                request.Cpf,
                request.BirthDate!.Value,
                request.NewPassword,
                request.ConfirmNewPassword);

            await _forgotPasswordUseCase.ExecuteAsync(command, cancellationToken);

            return NoContent();
        }
    }
}
