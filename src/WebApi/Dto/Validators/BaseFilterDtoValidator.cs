using FluentValidation;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.WebApi.Dto.Filters;

namespace Stellantis.ProjectName.WebApi.Dto.Validators
{
    public class BaseFilterDtoValidator<T> : AbstractValidator<T> where T : BaseFilterDto
    {
        public BaseFilterDtoValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage(FilterResources.PageGreaterThanZero);
        }
    }
}
