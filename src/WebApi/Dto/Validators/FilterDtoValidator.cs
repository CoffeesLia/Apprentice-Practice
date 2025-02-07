using FluentValidation;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.WebApi.Dto.Filters;

namespace Stellantis.ProjectName.WebApi.Dto.Validators
{
    internal class FilterDtoValidator<T> : AbstractValidator<T> where T : FilterDto
    {
        internal FilterDtoValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage(FilterResources.PageGreaterThanZero);
        }
    }
}
