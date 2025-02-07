using FluentValidation.TestHelper;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.Dto.Validators;

namespace WebApi.Tests.Dto.Validators
{
    public class FilterDtoValidatorTests
    {
        private readonly FilterDtoValidator<FilterDto> _validator;

        public FilterDtoValidatorTests()
        {
            _validator = new FilterDtoValidator<FilterDto>();
        }

        [Fact]
        public void Should_Have_Error_When_Page_Is_Less_Than_One()
        {
            // Arrange
            var model = new FilterDto { Page = 0 };

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Page)
                .WithErrorMessage("Page must be greater than zero.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_Page_Is_Greater_Than_Zero()
        {
            // Arrange
            var model = new FilterDto { Page = 1 };

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Page);
        }
    }
}
