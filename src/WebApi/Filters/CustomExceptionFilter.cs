using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Filters
{
    internal sealed class CustomExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            ObjectResult objectResult;

            Exception contextException = context.Exception.InnerException ?? context.Exception;

            int statusCode = (int)HttpStatusCode.InternalServerError;
            string message = contextException.Message;

            if (contextException is UnauthorizedAccessException)
            {
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = "Usuário sem autorização";
            }
            else if (contextException is FormatException)
            {
                statusCode = (int)HttpStatusCode.UnsupportedMediaType;
                message = "formato incorreto";
            }

            objectResult = new ObjectResult(new ErrorResponseVm(statusCode, message))
            {
                StatusCode = statusCode
            };
            context.Result = objectResult;
        }
    }
}