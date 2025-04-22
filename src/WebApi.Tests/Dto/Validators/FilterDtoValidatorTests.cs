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
        public void ShouldHaveErrorWhenPageIsLessThanOne()
        {
            // Arrange
            var model = new FilterDto { Page = 0 };

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(x => x.Page)
                .WithErrorMessage("A página deve ser maior que zero.");
        }

        [Fact]
        public void ShouldNotHaveErrorWhenPageIsGreaterThanZero()
        {
            // Arrange
            var model = new FilterDto { Page = 1 };

            // Act & Assert
            var result = _validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(x => x.Page);
        }
    }
}
