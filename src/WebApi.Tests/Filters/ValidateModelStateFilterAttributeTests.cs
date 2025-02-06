using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using Stellantis.ProjectName.WebApi.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace WebApi.Tests.Filters
{
    public class ValidateModelStateFilterAttributeTests
    {
        [Fact]
        public void OnActionExecuting_ReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var modelState = new ModelStateDictionary();
            modelState.AddModelError("Error", "Invalid model state");
            modelState.AddModelError("Error 2", "Invalid model state");

            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor(), modelState);

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                [],
                new Dictionary<string, object?>(),
                new Mock<Controller>().Object
            );

            var filter = new ValidateModelStateFilterAttribute();

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
            var valueResult = Assert.IsType<ErrorResponseVm>(result.Value);
            Assert.Equal(modelState.SelectMany(x => x.Value!.Errors.Select(x => x.ErrorMessage)), valueResult.Errors);
        }

        [Fact]
        public void OnActionExecuting_NotReturnBadRequest_WhenModelStateIsValid()
        {
            // Arrange
            var actionContext = new ActionContext
            {
                HttpContext = new DefaultHttpContext(),
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
            };

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                [],
                new Dictionary<string, object?>(),
                new Mock<Controller>().Object
            );

            var filter = new ValidateModelStateFilterAttribute();

            // Act
            filter.OnActionExecuting(actionExecutingContext);

            // Assert
            Assert.Null(actionExecutingContext.Result);
        }
    }
}