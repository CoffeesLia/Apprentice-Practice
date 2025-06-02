using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using Stellantis.ProjectName.WebApi.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace WebApi.Tests
{
    public class ValidateModelStateFilterAttributeTests
    {
        [Fact]
        public void OnActionExecutingReturnBadRequestWhenModelStateIsInvalid()
        {
            // Arrange
            ModelStateDictionary modelState = new();
            modelState.AddModelError("Error", "Invalid model state");
            modelState.AddModelError("Error 2", "Invalid model state");

            ActionContext actionContext = new(new DefaultHttpContext(), new RouteData(), new ActionDescriptor(), modelState);

            ActionExecutingContext actionExecutingContext = new(
                actionContext,
                [],
                new Dictionary<string, object?>(),
                new Mock<Controller>().Object
            );

            ValidateModelStateFilterAttribute filter = new();

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            BadRequestObjectResult result = Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
            ErrorResponseVm valueResult = Assert.IsType<ErrorResponseVm>(result.Value);
            Assert.Equal(modelState.SelectMany(x => x.Value!.Errors.Select(x => x.ErrorMessage)), valueResult.Errors);
        }

        [Fact]
        public void OnActionExecutingNotReturnBadRequestWhenModelStateIsValid()
        {
            // Arrange
            ActionContext actionContext = new()
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            ActionExecutingContext actionExecutingContext = new(
                actionContext,
                [],
                new Dictionary<string, object?>(),
                new Mock<Controller>().Object
            );

            ValidateModelStateFilterAttribute filter = new();

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.Null(actionExecutingContext.Result);
        }
    }
}