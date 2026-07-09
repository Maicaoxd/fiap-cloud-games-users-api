using UsersAPI.Api.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace UsersAPI.Tests.Api.Common;

[Trait("Category", "Unit")]
public sealed class ApiValidationProblemDetailsFactoryTests
{
    [Fact]
    public void CreateInvalidModelStateResponse_QuandoModelStateForInvalido_DeveRetornarBadRequestComCamposEmCamelCase()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/api/users";

        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Name", ApiMessages.User.NameRequired);
        modelState.AddModelError("ConfirmPassword", ApiMessages.User.ConfirmPasswordRequired);

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor(),
            modelState);

        // Act
        var actionResult = ApiValidationProblemDetailsFactory.CreateInvalidModelStateResponse(actionContext);

        // Assert
        var badRequestResult = actionResult.ShouldBeOfType<BadRequestObjectResult>();
        var problemDetails = badRequestResult.Value.ShouldBeOfType<ValidationProblemDetails>();

        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problemDetails.Title.ShouldBe(ApiMessages.Validation.Title);
        problemDetails.Detail.ShouldBe(ApiMessages.Validation.InvalidFields);
        problemDetails.Instance.ShouldBe("/api/users");
        problemDetails.Errors.ContainsKey("name").ShouldBeTrue();
        problemDetails.Errors.ContainsKey("confirmPassword").ShouldBeTrue();
    }
}

