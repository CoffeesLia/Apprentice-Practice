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
    public class FeedbackControllerTests
    {
        private readonly Mock<IFeedbackService> _serviceMock;
        private readonly FeedbacksController _controller;
        private readonly Fixture _fixture;

        public FeedbackControllerTests()
        {
            _serviceMock = new Mock<IFeedbackService>();
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
            _fixture.Customize<FeedbackDto>(c => c
                .With(dto => dto.Status, () => _fixture.Create<Stellantis.ProjectName.WebApi.Dto.FeedbackStatus>()));


            var feedbackDtoString = _fixture.Create<FeedbackDto>().ToString();

            _controller = new FeedbacksController(_serviceMock.Object, mapper, localizerFactory);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResult()
        {
            // Arrange
            FeedbackDto feedbackDto = _fixture.Create<FeedbackDto>();
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Feedback>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.CreateAsync(feedbackDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnFeedbackVm()
        {
            // Arrange
            Feedback feedback = _fixture.Create<Feedback>();
            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync(feedback);

            // Act
            ActionResult<FeedbackVm> result = await _controller.GetAsync(feedback.Id);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<FeedbackVm>(okResult.Value);
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
            Assert.IsType<PagedResultVm<FeedbackVm>>(okResult.Value);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResult()
        {
            // Arrange
            int feedbackId = _fixture.Create<int>();
            FeedbackDto feedbackDto = _fixture.Create<FeedbackDto>();
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Feedback>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.UpdateAsync(feedbackId, feedbackDto);

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