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
    public class FeedbackControllerTest
    {
        private readonly Mock<IFeedbackService> _serviceMock;
        private readonly FeedbackController _controller;
        private readonly Fixture _fixture;

        public FeedbackControllerTest()
        {
            _serviceMock = new Mock<IFeedbackService>();
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfiguration.CreateMapper();
            var localizerFactory = LocalizerFactorHelper.Create();

            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _fixture.Customize<FeedbackDto>(c => c
                .With(dto => dto.Status,
                      () => _fixture.Create<IncidentStatus>().ToString()));


            _controller = new FeedbackController(_serviceMock.Object, mapper, localizerFactory);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResult()
        {
            // Arrange
            FeedbackDto feedbacksDto = _fixture.Create<FeedbackDto>();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Feedback>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.CreateAsync(feedbacksDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnIncidentVm()
        {
            // Arrange
            Feedback feedbacks = _fixture.Create<Feedback>();
            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync(feedbacks);

            // Act
            ActionResult<FeedbacksVm> result = await _controller.GetAsync(feedbacks.Id);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<FeedbacksVm>(okResult.Value);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultVm()
        {
            // Arrange
            FeedbackFilterDto filterDto = _fixture.Create<FeedbackFilterDto>();
            PagedResult<Feedback> pagedResult = _fixture.Create<PagedResult<Feedback>>();
            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<FeedbackFilter>())).ReturnsAsync(pagedResult);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PagedResultVm<FeedbacksVm>>(okResult.Value);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResult()
        {
            // Arrange
            int feedbacksId = _fixture.Create<int>();
            FeedbackDto incidentDto = _fixture.Create<FeedbackDto>();
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Feedback>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.UpdateAsync(feedbacksId, incidentDto);

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