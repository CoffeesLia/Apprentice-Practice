using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.ViewModels;


namespace WebApi.Tests.Controllers
{
    public class IntegrationControllerTests
    {
        private readonly Mock<IIntegrationService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly IntegrationControllerBase _controller;
        private readonly Fixture _fixture;
    
    public IntegrationControllerTests()
        {
            _serviceMock = new Mock<IIntegrationService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _fixture = new Fixture();
            _controller = new IntegrationControllerBase(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }
        
        [Fact]

        public async Task CreateAsyncShouldReturnCreatedAtActionWhenCreationIsSuccessful()
        {
            // Arrange
            var integrationDto = _fixture.Create<IntegrationDto>();
            var integration = _fixture.Build<Integration>().With(i => i.Name, integrationDto.Name).Create();
            var integrationVm = _fixture.Build<IntegrationVm>().With(i => i.Name, integrationDto.Name).With(i => i.Id, integration.Id).Create();

            _mapperMock.Setup(m => m.Map<Integration>(integrationDto)).Returns(integration);
            _serviceMock.Setup(s => s.CreateAsync(integration)).ReturnsAsync(OperationResult.Complete("Success"));
            _mapperMock.Setup(m => m.Map<IntegrationVm>(integration)).Returns(integrationVm);

            // Act
            var result = await _controller.CreateAsync(integrationDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetAsync), createdAtActionResult.ActionName);
            Assert.Equal(integration.Id, createdAtActionResult.RouteValues["id"]);
            Assert.Equal(integrationVm, createdAtActionResult.Value);
        }
    }
}
