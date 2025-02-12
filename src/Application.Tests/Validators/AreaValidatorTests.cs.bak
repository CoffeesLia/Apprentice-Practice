using Application.Tests.Helpers;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Validators;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;

namespace Application.Tests.Validators
{
    public class AreaValidatorTests
    {
        private readonly AreaValidator _validator;

        public AreaValidatorTests()
        {
            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            _validator = new AreaValidator(localizerFactory);
        }

        [Fact]
        public void Name_IsNull_Error()
        {
            // Arrange
            var area = new Area(null!);

            // Act
            var result = _validator.TestValidate(area);

            // Assert
            result.ShouldHaveValidationErrorFor(a => a.Name)
                .WithErrorMessage(AreaResources.NameIsRequired);
        }

        [Fact]
        public void Name_IsEmpty_Error()
        {
            // Arrange
            var area = new Area(string.Empty);

            // Act
            var result = _validator.TestValidate(area);

            // Assert
            result.ShouldHaveValidationErrorFor(a => a.Name)
                .WithErrorMessage(AreaResources.NameIsRequired);
        }

        [Fact]
        public void Name_TooShort_Error()
        {
            // Arrange
            var area = new Area(new string('a', AreaValidator.MinimumLength - 1));

            // Act
            var result = _validator.TestValidate(area);

            // Assert
            result.ShouldHaveValidationErrorFor(a => a.Name)
                .WithErrorMessage(string.Format(AreaResources.NameValidateLength, AreaValidator.MinimumLength, AreaValidator.MaximumLength));
        }

        [Fact]
        public void Name_TooLong_Error()
        {
            // Arrange
            var area = new Area(new string('a', AreaValidator.MaximumLength + 1));

            // Act
            var result = _validator.TestValidate(area);

            // Assert
            result.ShouldHaveValidationErrorFor(a => a.Name)
                .WithErrorMessage(string.Format(AreaResources.NameValidateLength, AreaValidator.MinimumLength, AreaValidator.MaximumLength));
        }

        [Fact]
        public void Name_Valid_Success()
        {
            // Arrange
            var area = new Area("Valid Name");

            // Act
            var result = _validator.TestValidate(area);

            // Assert
            result.ShouldNotHaveValidationErrorFor(a => a.Name);
        }
    }
}