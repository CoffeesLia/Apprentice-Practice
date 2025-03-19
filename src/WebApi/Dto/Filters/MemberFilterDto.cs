using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Stellantis.ProjectName.WebApi.Dto;

namespace Stellantis.ProjectName.WebApi.Filters
{
    public class ValidateMemberDtoFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.ContainsKey("memberDto"))
            {
                var memberDto = context.ActionArguments["memberDto"] as MemberDto;
                if (memberDto == null || string.IsNullOrEmpty(memberDto.Name) || string.IsNullOrEmpty(memberDto.Role) || memberDto.Cost <= 0)
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