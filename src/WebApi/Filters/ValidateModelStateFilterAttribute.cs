using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;
using System.Net;
using System.Net.Mime;

namespace Stellantis.ProjectName.WebApi.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    internal sealed class ValidateModelStateFilterAttribute() : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            if (!context.ModelState.IsValid)
            {
                string[] errors = [.. context.ModelState.Values
                    .Where(p => p.Errors.Count > 0)
                    .SelectMany(p => p.Errors)
                    .Select(p => p.ErrorMessage)];

                context.Result = new BadRequestObjectResult(new ErrorResponseVm((int)HttpStatusCode.BadRequest, "", errors));
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.HttpContext.Response.ContentType = MediaTypeNames.Application.Json;
            }
        }
    }
}
