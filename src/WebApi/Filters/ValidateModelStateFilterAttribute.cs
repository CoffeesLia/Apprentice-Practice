
using Application.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Net.Mime;

namespace CleanArchBase.Filters
{

    public class ValidateModelStateFilterAttribute(ILogger<ValidateModelStateFilterAttribute> logger) : ActionFilterAttribute
    {
        private readonly ILogger<ValidateModelStateFilterAttribute> _logger = logger;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values.Where(p => p.Errors.Count > 0)
                                                  .SelectMany(p => p.Errors)
                                                  .Select(p => p.ErrorMessage)
                                                  .ToList();
                context.Result = new BadRequestObjectResult(new ErrorResponseVM((int)HttpStatusCode.BadRequest, "", errors));

                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.HttpContext.Response.ContentType = MediaTypeNames.Application.Json;
            }
        }
    }
}
