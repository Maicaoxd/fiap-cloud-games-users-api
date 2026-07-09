using UsersAPI.Api.Common;
using UsersAPI.Application.Common;
using UsersAPI.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace UsersAPI.Api.Middlewares
{
    public sealed class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var problemDetails = CreateProblemDetails(context, exception);

                LogException(context, exception, problemDetails.Status!.Value);

                await WriteProblemDetailsAsync(context, problemDetails);
            }
        }

        private void LogException(HttpContext context, Exception exception, int statusCode)
        {
            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled exception while processing {Method} {Path}.",
                    context.Request.Method,
                    context.Request.Path);

                return;
            }

            _logger.LogWarning(
                exception,
                "Handled exception while processing {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);
        }

        private static async Task WriteProblemDetailsAsync(HttpContext context, ProblemDetails problemDetails)
        {
            context.Response.StatusCode = problemDetails.Status!.Value;

            await context.Response.WriteAsJsonAsync(
                problemDetails,
                options: null,
                contentType: "application/problem+json");
        }

        private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
        {
            return exception switch
            {
                BadHttpRequestException => CreateProblemDetails(
                    context,
                    StatusCodes.Status400BadRequest,
                    ApiMessages.Validation.Title,
                    ApiMessages.Validation.RequestBodyRequired),

                ArgumentException => CreateProblemDetails(
                    context,
                    StatusCodes.Status400BadRequest,
                    ApiMessages.Validation.Title,
                    exception.Message),

                EmailAlreadyRegisteredException => CreateProblemDetails(
                    context,
                    StatusCodes.Status409Conflict,
                    ApiMessages.Conflict.Title,
                    exception.Message),

                CpfAlreadyRegisteredException => CreateProblemDetails(
                    context,
                    StatusCodes.Status409Conflict,
                    ApiMessages.Conflict.Title,
                    exception.Message),

                UserNotFoundException => CreateProblemDetails(
                    context,
                    StatusCodes.Status404NotFound,
                    ApiMessages.NotFound.Title,
                    exception.Message),

                InvalidPasswordRecoveryDataException => CreateProblemDetails(
                    context,
                    StatusCodes.Status400BadRequest,
                    ApiMessages.Validation.Title,
                    exception.Message),

                InvalidCredentialsException => CreateProblemDetails(
                    context,
                    StatusCodes.Status401Unauthorized,
                    ApiMessages.Unauthorized.Title,
                    exception.Message),

                InactiveUserException => CreateProblemDetails(
                    context,
                    StatusCodes.Status403Forbidden,
                    ApiMessages.Forbidden.Title,
                    exception.Message),

                DbUpdateException dbUpdateException when IsUniqueConstraintViolation(dbUpdateException) => CreateProblemDetails(
                    context,
                    StatusCodes.Status409Conflict,
                    ApiMessages.Conflict.Title,
                    ApplicationMessages.Conflict.UniqueConstraintViolation),

                _ => CreateProblemDetails(
                    context,
                    StatusCodes.Status500InternalServerError,
                    ApiMessages.InternalServerError.Title,
                    ApiMessages.InternalServerError.Detail)
            };
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            var message = exception.GetBaseException().Message;

            return message.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
                   message.Contains("duplicate", StringComparison.OrdinalIgnoreCase);
        }

        private static ProblemDetails CreateProblemDetails(
            HttpContext context,
            int statusCode,
            string title,
            string detail)
        {
            return new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };
        }
    }
}
