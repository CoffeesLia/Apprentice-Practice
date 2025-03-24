using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;
using Stellantis.ProjectName.Application.Validators;
using System.Globalization;
using Stellantis.ProjectName.Application.Models.Filters;
using AutoFixture;
using FluentValidation.Results;
using Stellantis.ProjectName.Application.Interfaces.Services;
namespace Application.Tests.Services
{
    public class IntegrationServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IIntegrationRepository> _integrationRepositoryMock;
        private readonly IntegrationService _integrationService;

        public IntegrationServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _integrationRepositoryMock = new Mock<IIntegrationRepository>();
            var localizer = Helpers.LocalizerFactorHelper.Create();
            var integrationValidator = new IntegrationValidator(localizer);

            _unitOfWorkMock.Setup(u => u.IntegrationRepository).Returns(_integrationRepositoryMock.Object);

            _integrationService = new IntegrationService(_unitOfWorkMock.Object, localizer, integrationValidator);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnCompleteWhenIntegrationExists()
        {
            // Arrange
            var fixture = new Fixture();
            var integration = fixture.Create<Integration>();
            _integrationRepositoryMock.Setup(r => r.GetByIdAsync(integration.Id)).ReturnsAsync(integration);

            // Act
            var result = await _integrationService.GetItemAsync(integration.Id);

            // Assert
            Assert.IsType<Integration>(result);
        }
    }
}


