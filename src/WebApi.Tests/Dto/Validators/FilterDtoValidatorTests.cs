using FluentValidation.TestHelper;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.Dto.Validators;
using System.Globalization;

namespace WebApi.Tests.Dto.Validators
{
    public class FilterDtoValidatorTests
    {
        private readonly FilterDtoValidator<FilterDto> _validator;

        public FilterDtoValidatorTests()
        {
            _validator = new FilterDtoValidator<FilterDto>();
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        }

        [Fact]
        public void ShouldHaveErrorWhenPageIsLessThanOne()
        {
            // Arrange
            var model = new FilterDto { Page = 0, PageSize = 10 };

            // Act & Assertdd
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Page)
                .WithErrorMessage(FilterResources.PageGreaterThanZero);
        }

        [Fact]
        public void ShouldNotHaveErrorWhenPageIsGreaterThanZero()
        {
            // Arrange
            var model = new FilterDto { Page = 1, PageSize = 10 };

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Page);
        }
    }
}
