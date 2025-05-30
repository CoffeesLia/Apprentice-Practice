using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
    public class ImprovementControllerTest
    {
        private readonly Mock<IImprovementService> _serviceMock;
        private readonly ImprovementController _controller;
        private readonly Fixture _fixture;

        public ImprovementControllerTest()
        {
            _serviceMock = new Mock<IImprovementService>();
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfiguration.CreateMapper();
            var localizerFactory = LocalizerFactorHelper.Create();

            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customize<ImprovementDto>(c => c
                .With(dto => dto.StatusImprovement,
                      () => _fixture.Create<IncidentStatus>().ToString()));


            _controller = new ImprovementController(_serviceMock.Object, mapper, localizerFactory);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResult()
        {
            // Arrange
            ImprovementDto improvementDto = _fixture.Create<ImprovementDto>();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Improvement>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.CreateAsync(improvementDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnIncidentVm()
        {
            // Arrange
            Improvement improvement = _fixture.Create<Improvement>();
            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync(improvement);

            // Act
            ActionResult<ImprovementVm> result = await _controller.GetAsync(improvement.Id);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<ImprovementVm>(okResult.Value);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultVm()
        {
            // Arrange
            ImprovementFilterDto filterDto = _fixture.Create<ImprovementFilterDto>();
            PagedResult<Improvement> pagedResult = _fixture.Create<PagedResult<Improvement>>();
            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<ImprovementFilter>())).ReturnsAsync(pagedResult);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PagedResultVm<ImprovementVm>>(okResult.Value);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResult()
        {
            // Arrange
            int improvementId = _fixture.Create<int>();
            ImprovementDto incidentDto = _fixture.Create<ImprovementDto>();
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Improvement>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.UpdateAsync(improvementId, incidentDto);

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