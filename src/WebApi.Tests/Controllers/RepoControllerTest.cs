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
    public class RepoControllerBaseTests
    {
        private readonly Mock<IRepoService> _serviceMock;
        private readonly RepoController _controller;
        private readonly Fixture _fixture = new();

        public RepoControllerBaseTests()
        {
            _serviceMock = new Mock<IRepoService>();
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfiguration.CreateMapper();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactor = LocalizerFactorHelper.Create();
            _controller = new RepoController(_serviceMock.Object, mapper, localizerFactor);

            _fixture.Behaviors

             .OfType<ThrowingRecursionBehavior>()

             .ToList()

             .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultVm()
        {
            // Arrange
            RepoFilterDto filterDto = _fixture.Create<RepoFilterDto>();
            PagedResult<Repo> pagedResult = _fixture.Create<PagedResult<Repo>>();
            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<RepoFilter>())).ReturnsAsync(pagedResult);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PagedResultVm<RepoVm>>(okResult.Value);
        }

        [Fact]
        public async Task GetAsyncReturnsRepoVmWhenIdIsValid()
        {
            // Arrange  
            var repo = _fixture.Build<Repo>()
                .With(d => d.Id, 1)
                .With(d => d.Name, "Rep1")
                .With(d => d.Description,  "ValidDescription")
                .With(d => d.Url, new Uri("https://example.com/rep1"))
                .With(d => d.ApplicationId, 1)
                .Create();

            var expectedVm = new RepoVm
            {
                Id = repo.Id,
                Name = repo.Name,
                Description = repo.Description,
                Url = repo.Url,
                ApplicationId = repo.ApplicationId,
                ApplicationData = new ApplicationVm
                {
                    Id = 1,
                    Name = "App1",
                    Area = new AreaVm(),   
                    External = false
                }
            };

            _serviceMock.Setup(s => s.GetItemAsync(1)).ReturnsAsync(repo);

            // Act  
            var result = await _controller.GetAsync(1);

            // Assert  
            var okResult = Assert.IsType<OkObjectResult>(result);
            var vm = Assert.IsType<RepoVm>(okResult.Value);

            Assert.Equal(expectedVm.Id, vm.Id);
            Assert.Equal(expectedVm.Name, vm.Name);
            Assert.Equal(expectedVm.Url, vm.Url);
            Assert.Equal(expectedVm.ApplicationId, vm.ApplicationId);
        }

        [Fact]
        public async Task GetAsyncReturnsNotFoundWhenIdIsInvalid()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync((Repo?)null);

            // Act
            var result = await _controller.GetAsync(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenRepoIsValid()
        {
            // Arrange
            var dto = new RepoDto
            {
                Name = "Rep2",
                Description = "ValidDescription",
                Url = new Uri("https://example.com/rep2"),
                ApplicationId = 2
            };

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Repo>()))
                .ReturnsAsync(OperationResult.Complete("Registered successfully"));

            // Act
            var result = await _controller.CreateAsync(dto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResultWhenRepoIsValid()
        {
            // Arrange
            var dto = new RepoDto
            {
                Name = "Rep3",
                Description = "ValidDescription",
                Url = new Uri("https://example.com/rep2"),
                ApplicationId = 3
            };

            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Repo>()))
                .ReturnsAsync(OperationResult.Complete("Updated successfully"));

            // Act
            var result = await _controller.UpdateAsync(3, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsyncReturnsNoContentWhenDeletionIsSuccessful()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(service => service.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.DeleteAsync(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncReturnsNotFoundWhenItemDoesNotExist()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(service => service.DeleteAsync(id)).ReturnsAsync(OperationResult.NotFound("Not found"));

            // Act
            var result = await _controller.DeleteAsync(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
} 