using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;

namespace WebApi.Tests.Controllers
{
    public class PartNumberControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IPartNumberService> _partNumberServiceMock;
        private readonly PartNumberController _controller;

        public PartNumberControllerTests()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddLocalization();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var localizerFactory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();

            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mapperMock = _fixture.Freeze<Mock<IMapper>>();
            _partNumberServiceMock = _fixture.Freeze<Mock<IPartNumberService>>();

            _controller = new PartNumberController(_mapperMock.Object, _partNumberServiceMock.Object, localizerFactory);
        }

        [Theory, AutoData]
        public async Task Create_ReturnOk_WhenCreationIsSuccessful(PartNumber partNumber, PartNumberDto partNumberDto, OperationResult operationResult)
        {
            // Arrange
            _partNumberServiceMock.Setup(s => s.CreateAsync(partNumber)).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.Create(partNumberDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Successfully registered", okResult.Value);
        }

        [Theory, AutoData]
        public async Task Create_ReturnBadRequest_WhenCreationFails(PartNumber partNumber, PartNumberDto partNumberDto, OperationResult operationResult)
        {
            // Arrange
            _mapperMock.Setup(m => m.Map<PartNumberDto>(partNumber)).Returns(partNumberDto);
            //operationResult.Success = false;
            _partNumberServiceMock.Setup(s => s.CreateAsync(partNumber)).ReturnsAsync(operationResult);

            // Act
            var result = await _controller.Create(partNumberDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(operationResult, badRequestResult.Value);
        }
    }
}