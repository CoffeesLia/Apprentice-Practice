using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Stellantis.ProjectName.WebApi.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace WebApi.Tests
{
    public class CustomExceptionFilterTests
    {
        sealed internal class TestException : Exception
        {
            public TestException() : base()
            {
            }

            public TestException(string message) : base(message)
            {
            }

            public TestException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        [Fact]
        public void OnExceptionReturnInternalServerErrorWhenExceptionIsThrown()
        {
            // Arrange
            TestException exception = new("An error occurred");
            HttpContext httpContext = new DefaultHttpContext();
            ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
            ExceptionContext context = new(actionContext, [])
            {
                Exception = exception
            };

            CustomExceptionFilter filter = new();

            // Act
            filter.OnException(context);

            // Assert
            ObjectResult result = Assert.IsType<ObjectResult>(context.Result);
            ErrorResponseVm errorResponse = Assert.IsType<ErrorResponseVm>(result.Value);
            Assert.Equal((int)HttpStatusCode.InternalServerError, result.StatusCode);
            Assert.Equal("An error occurred", errorResponse.Message);
        }

        [Fact]
        public void OnExceptionReturnUnauthorizedWhenUnauthorizedAccessExceptionIsThrown()
        {
            // Arrange
            UnauthorizedAccessException exception = new("An error occurred");
            HttpContext httpContext = new DefaultHttpContext();
            ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
            ExceptionContext context = new(actionContext, [])
            {
                Exception = exception
            };

            CustomExceptionFilter filter = new();

            // Act
            filter.OnException(context);

            // Assert
            ObjectResult result = Assert.IsType<ObjectResult>(context.Result);
            ErrorResponseVm errorResponse = Assert.IsType<ErrorResponseVm>(result.Value);
            Assert.Equal((int)HttpStatusCode.Unauthorized, result.StatusCode);
            Assert.Equal("Usuário sem autorização", errorResponse.Message);
        }

        [Fact]
        public void OnExceptionReturnUnsupportedMediaTypeWhenFormatExceptionIsThrown()
        {
            // Arrange
            FormatException exception = new("An error occurred");
            HttpContext httpContext = new DefaultHttpContext();
            ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
            ExceptionContext context = new(actionContext, [])
            {
                Exception = exception
            };

            CustomExceptionFilter filter = new();

            // Act
            filter.OnException(context);

            // Assert
            ObjectResult result = Assert.IsType<ObjectResult>(context.Result);
            ErrorResponseVm errorResponse = Assert.IsType<ErrorResponseVm>(result.Value);
            Assert.Equal((int)HttpStatusCode.UnsupportedMediaType, result.StatusCode);
            Assert.Equal("formato incorreto", errorResponse.Message);
        }
    }
}
