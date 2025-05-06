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
    public class GitControllerBaseTests
    {
        private readonly Mock<IGitRepoService> _serviceMock;
        private readonly GitRepoController _controller;
        private readonly Fixture _fixture = new();

        public GitControllerBaseTests()
        {
            _serviceMock = new Mock<IGitRepoService>();
            var mapperConfiguration = new MapperConfiguration(x => { x.AddProfile<AutoMapperProfile>(); });
            var mapper = mapperConfiguration.CreateMapper();

            var localizerFactory = LocalizerFactorHelper.Create();
            _controller = new GitRepoController(_serviceMock.Object, mapper, localizerFactory);
        }

        // Teste para verificar se CreateAsync retorna CreatedAtActionResult quando criação é bem-sucedida
        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenApplicationDataIsValid()
        {
            // Arrange
            var dto = _fixture.Create<GitRepoDto>();

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<GitRepo>())).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.CreateAsync(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.IsType<GitRepoVm>(createdResult.Value);
        }

        // Teste para verificar se GetAsync retorna GitRepoVm corretamente
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

            // Act
            var result = await _controller.GetAsync(entity.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedVm = Assert.IsType<GitRepoVm>(okResult.Value);
            Assert.Equal(vm.ApplicationId, returnedVm.ApplicationId);
            Assert.Equal(vm.Name, returnedVm.Name);
        }

        // Teste para verificar se GetListAsync retorna resultado paginado corretamente
        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filterDto = _fixture.Create<GitRepoFilterDto>();
            var filter = _fixture.Create<GitRepoFilter>();
            var pagedResult = _fixture.Create<PagedResult<GitRepo>>();

            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetListAsync(filterDto);


            // Assert
            Assert.IsType<OkObjectResult>(result);

        }

        // Teste para verificar se UpdateAsync retorna sucesso quando atualização é realizada
        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenUpdateIsSuccessful()
        {
            // Arrange
            var dto = new GitRepoDto
            {
                Name = "ValidName",
                Description = "ValidDescription",
                Url = new Uri("https://example.com"),
                ApplicationId = 1
            };

            var entity = new GitRepo("ValidName")
            {
                Name = "ValidName",
                Description = "ValidDescription",
                Url = new Uri("https://example.com"),
                ApplicationId = 1
            };

            var vm = new GitRepoVm
            {
                Name = "ValidName",
                Description = "ValidDescription",
                Url = new Uri("https://example.com"),
                ApplicationId = 1
            };

            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<GitRepo>())).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.UpdateAsync(dto.ApplicationId, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.IsType<GitRepoVm>(okResult.Value);
        }

        //Teste para verificar se DeleteAsync retorna NoContent quando exclusão é bem-sucedida
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

