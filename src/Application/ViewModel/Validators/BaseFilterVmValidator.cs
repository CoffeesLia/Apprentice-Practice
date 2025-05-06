using FluentValidation;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.Application.Models.Validators
{
    public class BaseFilterVmValidator<T> : AbstractValidator<T> where T : Filter
    {
        public BaseFilterVmValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage(FilterValidatorResource.PageGreaterThanZero);
        }
    }
}
