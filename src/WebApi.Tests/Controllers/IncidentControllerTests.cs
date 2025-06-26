using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.Mapper;
using Stellantis.ProjectName.WebApi.ViewModels;
using WebApi.Tests.Helpers;

namespace WebApi.Tests.Controllers
{
    public class IncidentsControllerTests
    {
        private readonly Mock<IIncidentService> _serviceMock;
        private readonly IncidentsController _controller;
        private readonly Fixture _fixture;

        public IncidentsControllerTests()
        {
            _serviceMock = new Mock<IIncidentService>();
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfiguration.CreateMapper();
            var localizerFactory = LocalizerFactorHelper.Create();

            _fixture = new Fixture();
            // Evita exceção de referência circular no AutoFixture
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            // Configura o AutoFixture para gerar valores válidos para o Status
            _fixture.Customize<IncidentDto>(c => c
                .With(dto => dto.Status, () => _fixture.Create<Stellantis.ProjectName.WebApi.Dto.IncidentStatus>()));


            var incidentDtoString = _fixture.Create<IncidentDto>().ToString();

            _controller = new IncidentsController(_serviceMock.Object, mapper, localizerFactory);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResult()
        {
            // Arrange
            IncidentDto incidentDto = _fixture.Create<IncidentDto>();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Incident>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.CreateAsync(incidentDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnIncidentVm()
        {
            // Arrange
            Incident incident = _fixture.Create<Incident>();
            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync(incident);

            // Act
            ActionResult<IncidentVm> result = await _controller.GetAsync(incident.Id);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<IncidentVm>(okResult.Value);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultVm()
        {
            // Arrange
            IncidentFilterDto filterDto = _fixture.Create<IncidentFilterDto>();
            PagedResult<Incident> pagedResult = _fixture.Create<PagedResult<Incident>>();
            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<IncidentFilter>())).ReturnsAsync(pagedResult);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PagedResultVm<IncidentVm>>(okResult.Value);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResult()
        {
            // Arrange
            int incidentId = _fixture.Create<int>();
            IncidentDto incidentDto = _fixture.Create<IncidentDto>();
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Incident>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.UpdateAsync(incidentId, incidentDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentResult()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.DeleteAsync(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}