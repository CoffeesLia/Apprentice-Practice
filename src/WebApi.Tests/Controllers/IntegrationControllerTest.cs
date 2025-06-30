using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.Mapper;
using Stellantis.ProjectName.WebApi.ViewModels;
using WebApi.Tests.Helpers;

namespace WebApi.Tests.Controllers
{
    public class IntegrationControllerTests
    {
        private readonly Mock<IIntegrationService> _serviceMock;
        private readonly IntegrationController _controller;
        private readonly Fixture _fixture = new();

        public IntegrationControllerTests()
        {
            _serviceMock = new Mock<IIntegrationService>();
            var mapperConfiguration = new MapperConfiguration(x => { x.AddProfile<AutoMapperProfile>(); });
            var mapper = mapperConfiguration.CreateMapper();
            var localizerFactor = LocalizerFactorHelper.Create();
            _controller = new IntegrationController(_serviceMock.Object, mapper, localizerFactor);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultWhenIntegrationsExist()
        {
            // Arrange
            var filterDto = _fixture.Create<IntegrationFilterDto>();
            var pagedResult = new PagedResult<Integration> { Result = [], Page = 0, PageSize = 0, Total = 0 };
            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResultVm<IntegrationVm>>(okResult.Value);
            Assert.Equal(pagedResult.Result.Count(), returnValue.Result.Count());
        }

        [Fact]
        public async Task GetListAsyncShouldReturnEmptyResultWhenNoIntegrationsExist()
        {
            // Arrange
            var filterDto = _fixture.Create<IntegrationFilterDto>();
            var pagedResult = new PagedResult<Integration> { Result = [], Page = 0, PageSize = 0, Total = 0 };
            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResultVm<IntegrationVm>>(okResult.Value);
            Assert.Empty(returnValue.Result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentResultWhenIntegrationIsDeleted()
        {
            // Arrange
            var integrationId = _fixture.Create<int>();
            _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>())).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.DeleteAsync(integrationId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedResultWhenIntegrationIsCreated()
        {
            // Arrange
            var integrationDto = new IntegrationDto { Name = "Test Integration", Description = "Test Description", ApplicationDataId = 1 };
            var integration = new Integration("Test Integration", "Test Description") {  ApplicationDataId = 1 };
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Integration>())).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.CreateAsync(integrationDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal("GET", createdResult.ActionName);
            var integrationVm = Assert.IsType<IntegrationVm>(createdResult.Value);
            Assert.Equal(integration.Id, integrationVm.Id);
        }
        [Fact]
        public async Task GetAsyncShouldReturnOkResultWhenIntegrationExists()
        {
            // Arrange  
            var integrationId = _fixture.Create<int>();
            var integration = new Integration("Test Name", "Test Description")
            {
                Id = integrationId,
                ApplicationDataId = 1
            };
            _serviceMock.Setup(s => s.GetItemAsync(integrationId)).ReturnsAsync(integration);

            // Act  
            var result = await _controller.GetAsync(integrationId);

            // Assert  
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<IntegrationVm>(okResult.Value);
            Assert.Equal(integration.Id, returnValue.Id);
        }
        [Fact]
        public async Task GetAsyncShouldReturnNotFoundResultWhenIntegrationDoesNotExist()
        {
            // Arrange
            var integrationId = _fixture.Create<int>();
            _serviceMock.Setup(s => s.GetItemAsync(integrationId)).ReturnsAsync((Integration)null!);
            // Act
            var result = await _controller.GetAsync(integrationId);
            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }
        [Fact]
        public void IntegrationVmShouldSetPropertiesCorrectly()
        {
            // Arrange  
            var name = "Test Name";
            var description = "Test Description";
            var applicationDataId = 123;

            // Act  
            var integrationVm = new IntegrationVm
            {
                Name = name,
                Description = description,
                ApplicationDataId = applicationDataId
            };

            // Assert  
            Assert.Equal(name, integrationVm.Name);
            Assert.Equal(description, integrationVm.Description);
            Assert.Equal(applicationDataId, integrationVm.ApplicationDataId);
        }

        [Fact]
        public async Task GetListAsyncReturnList()
        {
            // Arrange  
            var filterDto = _fixture.Create<IntegrationFilterDto>();

            // Configuração para evitar referências circulares
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var pagedResult = _fixture.Build<PagedResult<Integration>>()
                                      .Without(pr => pr.Result) 
                                      .Create();
            pagedResult.Result = _fixture.CreateMany<Integration>(5).ToList();

            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<IntegrationFilter>())).ReturnsAsync(pagedResult);

            // Act  
            var result = await _controller.GetListAsync(filterDto);

            // Assert  
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<PagedResultVm<IntegrationVm>>(okResult.Value);
            Assert.NotNull(returnValue);
            Assert.Equal(pagedResult.Page, returnValue.Page);
            Assert.Equal(pagedResult.PageSize, returnValue.PageSize);
            Assert.Equal(pagedResult.Total, returnValue.Total);
            Assert.Equal(pagedResult.Result.Count(), returnValue.Result.Count());
        }
    }
}
