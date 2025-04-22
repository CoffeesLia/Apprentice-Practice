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
using Stellantis.ProjectName.WebApi.ViewModels;
using WebApi.Tests.Helpers;

namespace WebApi.Tests.Controllers
{
    public class GitControllerBaseTests
    {
        private readonly Mock<IGitRepoService> _serviceMock;
        private readonly GitRepoControllerBase _controller;
        private readonly Fixture _fixture = new();
        private readonly Mock<IMapper> _mapperMock;

        public GitControllerBaseTests()
        {
            _serviceMock = new Mock<IGitRepoService>();
            _mapperMock = new Mock<IMapper>();

            var localizerFactory = LocalizerFactorHelper.Create();
            _controller = new GitRepoControllerBase(_serviceMock.Object, _mapperMock.Object, localizerFactory);
        }

        /// <summary>
        /// Teste para verificar se CreateAsync retorna CreatedAtActionResult quando criação é bem-sucedida
        /// </summary>
        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenApplicationDataIsValid()
        {
            // Arrange
            var dto = _fixture.Create<GitRepoDto>();
            var entity = _fixture.Create<GitRepo>();
            var vm = _fixture.Create<GitRepoVm>();

            _mapperMock.Setup(m => m.Map<GitRepo>(dto)).Returns(entity);
            _mapperMock.Setup(m => m.Map<GitRepoVm>(It.IsAny<GitRepo>())).Returns(vm);
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<GitRepo>())).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.CreateAsync(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.IsType<GitRepoVm>(createdResult.Value);
        }

        /// <summary>
        /// Teste para verificar se GetAsync retorna GitRepoVm corretamente
        /// </summary>
        [Fact]
        public async Task GetAsyncShouldReturnGitRepoVm()
        {
            // Arrange
            var entity = _fixture.Create<GitRepo>();
            var vm = _fixture.Build<GitRepoVm>()
                .With(x => x.ApplicationId, entity.ApplicationId)
                .With(x => x.Name, entity.Name)
                .Create();

            _serviceMock.Setup(s => s.GetItemAsync(entity.Id)).ReturnsAsync(entity);
            _mapperMock.Setup(m => m.Map<GitRepoVm>(entity)).Returns(vm);

            // Act
            var result = await _controller.GetAsync(entity.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedVm = Assert.IsType<GitRepoVm>(okResult.Value);
            Assert.Equal(vm.ApplicationId, returnedVm.ApplicationId);
            Assert.Equal(vm.Name, returnedVm.Name);
        }

        /// <summary>
        /// Teste para verificar se GetListAsync retorna resultado paginado corretamente
        /// </summary>
        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filterDto = _fixture.Create<GitRepoFilterDto>();
            var filter = _fixture.Create<GitRepoFilter>();
            var pagedResult = _fixture.Create<PagedResult<GitRepo>>();
            var pagedVm = _fixture.Create<PagedResultVm<GitRepoVm>>();

            _mapperMock.Setup(m => m.Map<GitRepoFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<GitRepoVm>>(pagedResult)).Returns(pagedVm);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<PagedResultVm<GitRepoVm>>(okResult.Value);
            Assert.Equal(pagedVm.Total, returned.Total);
            Assert.Equal(pagedVm.Result.Count(), returned.Result.Count());
        }

        /// <summary>
        /// Teste para verificar se UpdateAsync retorna sucesso quando atualização é realizada
        /// </summary>
        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenUpdateIsSuccessful()
        {
            // Arrange
            var dto = _fixture.Create<GitRepoDto>();
            var entity = _fixture.Create<GitRepo>();
            var vm = _fixture.Create<GitRepoVm>();

            _mapperMock.Setup(m => m.Map<GitRepo>(dto)).Returns(entity);
            _mapperMock.Setup(m => m.Map<GitRepoVm>(entity)).Returns(vm);
            _serviceMock.Setup(s => s.UpdateAsync(entity)).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.UpdateAsync(dto.ApplicationId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(vm, okResult.Value);
        }

        /// <summary>
        /// Teste para verificar se DeleteAsync retorna NoContent quando exclusão é bem-sucedida
        /// </summary>
        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenDeleteIsSuccessful()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.DeleteAsync(id);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }
    }
}
