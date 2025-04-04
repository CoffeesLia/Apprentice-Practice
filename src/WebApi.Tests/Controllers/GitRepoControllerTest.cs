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
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly GitRepoControllerBase _controller;
        private readonly Fixture _fixture;

        public GitControllerBaseTests()
        {
            _serviceMock = new Mock<IGitRepoService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _fixture = new Fixture();

            _controller = new GitRepoControllerBase(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }

        [Fact]
        // Teste para verificar se CreateAsync retorna CreatedAtAction quando a criação é bem-sucedida
        public async Task CreateAsyncShouldReturnCreatedAtActionWhenCreationIsSuccessful()
        {
            // Arrange
            var gitRepoDto = _fixture.Create<GitRepoDto>();
            var gitRepo = _fixture.Build<GitRepo>().With(a => a.Name, gitRepoDto.Name).Create();
            var gitRepoVm = _fixture.Build<GitRepoVm>().With(a => a.Name, gitRepoDto.Name).With(a => a.ApplicationId, gitRepo.ApplicationId).Create();

            _mapperMock.Setup(m => m.Map<GitRepo>(gitRepoDto)).Returns(gitRepo);
            _serviceMock.Setup(s => s.CreateAsync(gitRepo)).ReturnsAsync(OperationResult.Complete("Success"));
            _mapperMock.Setup(m => m.Map<GitRepoVm>(gitRepo)).Returns(gitRepoVm);

            // Act
            var result = await _controller.CreateAsync(gitRepoDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetAsync), createdResult.ActionName);
            Assert.Equal(gitRepo.ApplicationId, createdResult.RouteValues!["id"]);
            Assert.NotNull(createdResult.RouteValues);
            Assert.Equal(gitRepo.ApplicationId, createdResult.RouteValues["id"]);
            Assert.Equal(gitRepoVm, createdResult.Value);
        }

        [Fact]
        // Teste para verificar se GetAsync retorna AreaVm
        public async Task GetAsyncShouldReturnAreaVm()
        {
            // Arrange
            var gitRepo = _fixture.Create<GitRepo>();
            var gitRepoVm = _fixture.Build<GitRepoVm>().With(a => a.ApplicationId, gitRepo.ApplicationId).With(a => a.Name, gitRepo.Name).Create();

            _serviceMock.Setup(s => s.GetItemAsync(gitRepo.ApplicationId)).ReturnsAsync(gitRepo);
            _mapperMock.Setup(m => m.Map<GitRepoVm>(gitRepo)).Returns(gitRepoVm);

            // Act
            var result = await _controller.GetAsync(gitRepo.ApplicationId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedgitRepoVm = Assert.IsType<GitRepoVm>(okResult.Value);
            Assert.Equal(gitRepoVm.ApplicationId, returnedgitRepoVm.ApplicationId);
            Assert.Equal(gitRepoVm.Name, returnedgitRepoVm.Name);
        }

        [Fact]
        // Teste para verificar se GetListAsync retorna PagedResult
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
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
            _mapperMock.Setup(m => m.Map<PagedResultVm<GitRepoVm>>(pagedResult)).Returns(pagedResultVm);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPagedResultVm = Assert.IsType<PagedResultVm<GitRepoVm>>(okResult.Value);
            Assert.Equal(pagedResultVm.Result.Count(), returnedPagedResultVm.Result.Count());
            Assert.Equal(pagedResultVm.Result.First().Name, returnedPagedResultVm.Result.First().Name);
        }

        [Fact]
        // Teste para verificar se UpdateAsync retorna Success quando a atualização é bem-sucedida
        public async Task UpdateAsyncShouldReturnSuccessWhenUpdateIsSuccessful()
        {
            // Arrange
            var gitRepoDto = _fixture.Create<GitRepoDto>();
            var gitRepo = _fixture.Build<GitRepo>().With(a => a.ApplicationId, gitRepoDto.ApplicationId).With(a => a.Name, gitRepoDto.Name).Create();
            var gitRepoVm = _fixture.Build<GitRepoVm>().With(a => a.ApplicationId, gitRepoDto.ApplicationId).With(a => a.Name, gitRepoDto.Name).Create();

            _mapperMock.Setup(m => m.Map<GitRepo>(gitRepoDto)).Returns(gitRepo);
            _mapperMock.Setup(m => m.Map<GitRepoVm>(gitRepo)).Returns(gitRepoVm);
            _serviceMock.Setup(s => s.UpdateAsync(gitRepo)).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.UpdateAsync(gitRepoDto.ApplicationId, gitRepoDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(gitRepoVm, okResult.Value);
        }

        [Fact]
        // Teste para verificar se DeleteAsync retorna NoContent quando a exclusão é bem-sucedida
        public async Task DeleteAsyncShouldReturnNoContentWhenDeleteIsSuccessful()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.DeleteAsync(id);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }
    }
}