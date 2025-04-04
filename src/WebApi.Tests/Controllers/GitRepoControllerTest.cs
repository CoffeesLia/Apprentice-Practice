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
using AutoFixture;


namespace WebApi.Tests.Controllers
{
    public class GitControllerBaseTests
    {
        private readonly Mock<IGitRepoService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GitRepoControllerBase _controller;
        private readonly Fixture _fixture;

        public GitControllerBaseTests()
        {
            _serviceMock = new Mock<IGitRepoService>();
            _mapperMock = new Mock<IMapper>();
            _fixture = new Fixture();
            _controller = new GitRepoControllerBase(_serviceMock.Object, _mapperMock.Object, new Mock<IStringLocalizerFactory>().Object);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenApplicationDataIsValid()
        {
            // Arrange
            var gitRepoDto = new GitRepoDto
            {
                Name = "Repo1",
                Description = "Description",
                Url = new Uri("https://existing-url.com"),
                ApplicationId = 1,
            };
            var gitRepoVm = new GitRepoVm
            {
                Name = "Repo1",
                Description = "Description",
                Url = new Uri("https://existing-url.com"),
                ApplicationId = 1,
            };

            _mapperMock.Setup(m => m.Map<GitRepo>(It.IsAny<GitRepoDto>())).Returns(new GitRepo("Valid Name")
            {
                Name = "Repo1",
                Description = "Description",
                Url = new Uri("https://existing-url.com"),
                ApplicationId = 2,
                Application = new ApplicationData("Name")
                {
                    ConfigurationItem = "ConfigItem",
                    ProductOwner = "Owner",
                    Name = "Name"
                },
            });

            _mapperMock.Setup(m => m.Map<GitRepoVm>(It.IsAny<GitRepo>())).Returns(gitRepoVm);

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<GitRepo>())).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.CreateAsync(gitRepoDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnGitRepoVm()
        {
            // Arrange
            var gitRepo = _fixture.Create<GitRepo>();
            var gitRepoVm = _fixture.Build<GitRepoVm>()
                .With(a => a.ApplicationId, gitRepo.ApplicationId)
                .With(a => a.Name, gitRepo.Name)
                .Create();

            _serviceMock.Setup(s => s.GetItemAsync(gitRepo.Id)).ReturnsAsync(gitRepo);
            _mapperMock.Setup(m => m.Map<GitRepoVm>(gitRepo)).Returns(gitRepoVm);

            // Act
            var result = await _controller.GetAsync(gitRepo.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGitRepoVm = Assert.IsType<GitRepoVm>(okResult.Value);

            Assert.Equal(gitRepoVm.ApplicationId, returnedGitRepoVm.ApplicationId);
            Assert.Equal(gitRepoVm.Name, returnedGitRepoVm.Name);
        }


        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            var filterDto = _fixture.Create<GitRepoFilterDto>();
            var filter = _fixture.Build<GitRepoFilter>().With(f => f.Name, filterDto.Name).Create();
            var pagedResult = _fixture.Create<PagedResult<GitRepo>>();
            var pagedResultVm = _fixture.Build<PagedResultVm<GitRepoVm>>()
                .With(p => p.Result, pagedResult.Result.Select(a => _fixture.Build<GitRepoVm>().With(vm => vm.Name, a.Name).Create()).ToList())
                .With(p => p.Page, pagedResult.Page)
                .With(p => p.PageSize, pagedResult.PageSize)
                .With(p => p.Total, pagedResult.Total)
                .Create();

            _mapperMock.Setup(m => m.Map<GitRepoFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<GitRepoVm>>(pagedResult)).Returns(pagedResultVm);

            var result = await _controller.GetListAsync(filterDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPagedResultVm = Assert.IsType<PagedResultVm<GitRepoVm>>(okResult.Value);
            Assert.Equal(pagedResultVm.Result.Count(), returnedPagedResultVm.Result.Count());
            Assert.Equal(pagedResultVm.Result.First().Name, returnedPagedResultVm.Result.First().Name);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenUpdateIsSuccessful()
        {
            var gitRepoDto = _fixture.Create<GitRepoDto>();
            var gitRepo = _fixture.Build<GitRepo>().With(a => a.ApplicationId, gitRepoDto.ApplicationId).With(a => a.Name, gitRepoDto.Name).Create();
            var gitRepoVm = _fixture.Build<GitRepoVm>().With(a => a.ApplicationId, gitRepoDto.ApplicationId).With(a => a.Name, gitRepoDto.Name).Create();

            _mapperMock.Setup(m => m.Map<GitRepo>(gitRepoDto)).Returns(gitRepo);
            _mapperMock.Setup(m => m.Map<GitRepoVm>(gitRepo)).Returns(gitRepoVm);
            _serviceMock.Setup(s => s.UpdateAsync(gitRepo)).ReturnsAsync(OperationResult.Complete("Success"));

            var result = await _controller.UpdateAsync(gitRepoDto.ApplicationId, gitRepoDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(gitRepoVm, okResult.Value);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenDeleteIsSuccessful()
        {
            int id = _fixture.Create<int>();
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete("Success"));

            var result = await _controller.DeleteAsync(id);

            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }
    }
}
