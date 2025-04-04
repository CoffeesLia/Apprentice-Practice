using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Filters
{
    public class ValidateMemberVmFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.ContainsKey("memberVm"))
            {
                var memberVm = context.ActionArguments["memberVm"] as MemberVm;
                if (memberVm == null || string.IsNullOrEmpty(memberVm.Name) || string.IsNullOrEmpty(memberVm.Role) || memberVm.Cost <= 0)
                {
                    context.Result = new BadRequestObjectResult("MemberRequiredFieldsMissing");
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Do nothing
        }
    }
}