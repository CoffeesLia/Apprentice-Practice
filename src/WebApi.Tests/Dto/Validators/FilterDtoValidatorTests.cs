using System.Globalization;
using FluentValidation.TestHelper;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.Dto.Validators;

namespace WebApi.Tests
{
    public class FilterDtoValidatorTests
    {
        private readonly FilterDtoValidator<FilterDto> _validator;

        public FilterDtoValidatorTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _validator = new FilterDtoValidator<FilterDto>();
        }

        [Fact]
        public void ShouldHaveErrorWhenPageIsLessThanOne()
        {
            // Arrange
            FilterDto model = new() { Page = 0, PageSize = 10 };

            // Act & Assertdd
            TestValidationResult<FilterDto> result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Page)
                .WithErrorMessage(FilterResources.PageGreaterThanZero);
        }

        [Fact]
        public void ShouldNotHaveErrorWhenPageIsGreaterThanZero()
        {
            // Arrange
            FilterDto model = new() { Page = 1, PageSize = 10 };

            // Act & Assert
            TestValidationResult<FilterDto> result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Page);
        }
    }
}
