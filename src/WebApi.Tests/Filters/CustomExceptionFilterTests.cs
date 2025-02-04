using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Stellantis.ProjectName.WebApi.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;
using System.Net;

namespace WebApi.Tests.Filters
{
    public class CustomExceptionFilterTests
    {
        [Fact]
        public void OnException_ReturnInternalServerError_WhenExceptionIsThrown()
        {
            // Arrange
            var exception = new Exception("An error occurred");
            HttpContext httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var context = new ExceptionContext(actionContext, new List<IFilterMetadata>());
            context.Exception = exception;

            var filter = new CustomExceptionFilter();

            // Act
            filter.OnException(context);

            // Assert
            var result = Assert.IsType<ObjectResult>(context.Result);
            var errorResponse = Assert.IsType<ErrorResponseVm>(result.Value);
            Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Equal("An error occurred", errorResponse.Message);
        }

        [Fact]
        public void OnException_ReturnUnauthorized_WhenUnauthorizedAccessExceptionIsThrown()
        {
            // Arrange
            var exception = new UnauthorizedAccessException("An error occurred");
            HttpContext httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var context = new ExceptionContext(actionContext, new List<IFilterMetadata>());
            context.Exception = exception;

            var filter = new CustomExceptionFilter();

            // Act
            filter.OnException(context);

            // Assert
            var result = Assert.IsType<ObjectResult>(context.Result);
            var errorResponse = Assert.IsType<ErrorResponseVm>(result.Value);
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
            Assert.Equal("Usuário sem autorização", errorResponse.Message);
        }

        [Fact]
        public void OnException_ReturnUnsupportedMediaType_WhenFormatExceptionIsThrown()
        {
            // Arrange
            var exception = new FormatException("An error occurred");
            HttpContext httpContext = new DefaultHttpContext();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var context = new ExceptionContext(actionContext, new List<IFilterMetadata>());
            context.Exception = exception;

            var filter = new CustomExceptionFilter();

            // Act
            filter.OnException(context);

            // Assert
            var result = Assert.IsType<ObjectResult>(context.Result);
            var errorResponse = Assert.IsType<ErrorResponseVm>(result.Value);
            Assert.Equal((int)HttpStatusCode.UnsupportedMediaType, result.StatusCode);
            Assert.Equal("formato incorreto", errorResponse.Message);
        }
    }
}
