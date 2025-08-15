using System.Collections.ObjectModel;
using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;


namespace WebApi.Tests.Controllers
{
    public class FeedbackControllerTests
    {
        private readonly Mock<IFeedbackService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly FeedbacksController _controller;
        private readonly Fixture _fixture;

        public FeedbackControllerTests()
        {
            _serviceMock = new Mock<IFeedbackService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _fixture = new Fixture();

            _controller = new FeedbacksController(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCorrectResultWhenCreationIsSuccessful()
        {
            //Arrange
           FeedbackDto feedbackDto = _fixture.Create<FeedbackDto>();
            Feedback feedback = _fixture.Build<Feedback>().With(a => a.Title, feedbackDto.Title).Create();
            FeedbackVm feedbackVm = _fixture.Build<FeedbackVm>().With(a => a.Title, feedbackDto.Title).With(a => a.Id, feedback.Id).Create();

            _mapperMock.Setup(m => m.Map<Feedback>(feedbackDto)).Returns(feedback);
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Feedback>()))
               .ReturnsAsync(OperationResult.Complete("Success"));
            _mapperMock.Setup(m => m.Map<FeedbackVm>(feedback)).Returns(feedbackVm);

            //Act
           IActionResult result = await _controller.CreateAsync(feedbackDto);

            //Assert
            Assert.IsType<CreatedAtActionResult>(result); // Verifica se o resultado é do tipo CreatedAtActionResult
            CreatedAtActionResult? objectResult = result as CreatedAtActionResult; // Cast para acessar propriedades, se necessário
            Assert.NotNull(objectResult);
            Assert.Equal(feedbackVm, objectResult.Value);
        }

        [Fact]
        // Teste para verificar se GetAsync retorna FeedbackVm
        public async Task GetAsyncShouldReturnFeedbackVm()
        {
            // Arrange
            Feedback feedback = _fixture.Create<Feedback>();
            FeedbackVm feedbackVm = _fixture.Build<FeedbackVm>().With(a => a.Id, feedback.Id).With(a => a.Title, feedback.Title).Create();

            _serviceMock.Setup(s => s.GetItemAsync(feedback.Id)).ReturnsAsync(feedback);
            _mapperMock.Setup(m => m.Map<FeedbackVm>(feedback)).Returns(feedbackVm);

            // Act
            ActionResult<FeedbackVm> result = await _controller.GetAsync(feedback.Id);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            FeedbackVm returnedFeedbackVm = Assert.IsType<FeedbackVm>(okResult.Value);
            Assert.Equal(feedbackVm.Id, returnedFeedbackVm.Id);
            Assert.Equal(feedbackVm.Title, returnedFeedbackVm.Title);
        }
        [Fact]
        public async Task GetAsyncShouldReturnFeedbackVmWithApplications()
        {
            // Arrange
            Feedback feedback = _fixture.Create<Feedback>();
            IList<ApplicationVm> applications = _fixture.CreateMany<ApplicationVm>().ToList(); // Convert IEnumerable to IList
            FeedbackVm feedbackVm = new()
            {
                Id = feedback.Id,
                Title = feedback.Title,
                Application = new ApplicationVm
                {
                    Name = "Sample Application", 
                    ResponsibleId = 1,
                    SquadId = 1,
                    AreaId = 1
                }
            };

            foreach (ApplicationVm app in applications)
            {
                feedbackVm.Application = app; 
            }

            _serviceMock.Setup(s => s.GetItemAsync(feedback.Id)).ReturnsAsync(feedback);
            _mapperMock.Setup(m => m.Map<FeedbackVm>(feedback)).Returns(feedbackVm);

            // Act
            ActionResult<FeedbackVm> result = await _controller.GetAsync(feedback.Id);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            FeedbackVm returnedFeedbackVm = Assert.IsType<FeedbackVm>(okResult.Value);
            Assert.Equal(feedbackVm.Id, returnedFeedbackVm.Id);
            Assert.Equal(feedbackVm.Title, returnedFeedbackVm.Title);
            Assert.Equal(feedbackVm.Application, returnedFeedbackVm.Application);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenUpdateIsSuccessful()
        {
            // Arrange
            FeedbackDto feedbackDto = _fixture.Create<FeedbackDto>();
            Feedback feedback = _fixture.Build<Feedback>()
                               .With(a => a.Title, feedbackDto.Title)
                               .Create();
            FeedbackVm feedbackVm = _fixture.Build<FeedbackVm>()
                                 .With(a => a.Id, feedback.Id)
                                 .With(a => a.Title, feedbackDto.Title)
                                 .Create();

            _mapperMock.Setup(m => m.Map<Feedback>(feedbackDto)).Returns(feedback);
            _mapperMock.Setup(m => m.Map<FeedbackVm>(feedback)).Returns(feedbackVm);
            _serviceMock.Setup(s => s.UpdateAsync(feedback)).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            IActionResult result = await _controller.UpdateAsync(feedback.Id, feedbackDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(feedbackVm, okResult.Value);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenDeleteIsSuccessful()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.DeleteAsync(id);

            // Assert
            NoContentResult noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnOkResultWithPagedResult()
        {
            // Arrange
            FeedbackFilterDto filterDto = _fixture.Create<FeedbackFilterDto>();
            FeedbackFilter filter = _fixture.Create<FeedbackFilter>();
            PagedResult<Feedback> pagedResult = _fixture.Build<PagedResult<Feedback>>()
                                      .With(pr => pr.Result, [.. _fixture.CreateMany<Feedback>(2)])
                                      .With(pr => pr.Page, 1)
                                      .With(pr => pr.PageSize, 10)
                                      .With(pr => pr.Total, 2)
                                      .Create();
            PagedResultVm<FeedbackVm> pagedResultVm = _fixture.Build<PagedResultVm<FeedbackVm>>()
                                        .With(pr => pr.Result, [.. _fixture.CreateMany<FeedbackVm>(2)])
                                        .With(pr => pr.Page, 1)
                                        .With(pr => pr.PageSize, 10)
                                        .With(pr => pr.Total, 2)
                                        .Create();

            _mapperMock.Setup(m => m.Map<FeedbackFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<FeedbackVm>>(pagedResult)).Returns(pagedResultVm);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            PagedResultVm<FeedbackVm> returnedPagedResultVm = Assert.IsType<PagedResultVm<FeedbackVm>>(okResult.Value);
            Assert.Equal(pagedResultVm.Page, returnedPagedResultVm.Page);
            Assert.Equal(pagedResultVm.PageSize, returnedPagedResultVm.PageSize);
            Assert.Equal(pagedResultVm.Total, returnedPagedResultVm.Total);
            Assert.Equal(pagedResultVm.Result.Count(), returnedPagedResultVm.Result.Count());
        }

        [Fact]
        public async Task GetListAsyncShouldReturnEmptyPagedResultWhenNoFeedbacksFound()
        {
            // Arrange
            FeedbackFilterDto filterDto = _fixture.Create<FeedbackFilterDto>();
            FeedbackFilter filter = _fixture.Create<FeedbackFilter>();
            PagedResult<Feedback> pagedResult = new()
            {
                Result = [],
                Page = 1,
                PageSize = 10,
                Total = 0
            };
            PagedResultVm<FeedbackVm> pagedResultVm = new()
            {
                Result = [],
                Page = 1,
                PageSize = 10,
                Total = 0
            };

            _mapperMock.Setup(m => m.Map<FeedbackFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<FeedbackVm>>(pagedResult)).Returns(pagedResultVm);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            PagedResultVm<FeedbackVm> returnedPagedResultVm = Assert.IsType<PagedResultVm<FeedbackVm>>(okResult.Value);
            Assert.Empty(returnedPagedResultVm.Result);
            Assert.Equal(0, returnedPagedResultVm.Total);
        }

    }
}