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
using Stellantis.ProjectName.WebApi.ViewModels;
using Stellantis.ProjectName.Application.Resources;
using AutoFixture;
using Xunit;

namespace Stellantis.ProjectName.WebApi.Tests.Controllers
{
    public class ResponsibleControllerTests
    {
        private readonly Mock<IResponsibleService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly ResponsibleController _controller;
        private readonly Fixture _fixture;

        public ResponsibleControllerTests()
        {
            _serviceMock = new Mock<IResponsibleService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _fixture = new Fixture();

            _controller = new ResponsibleController(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }

        [Fact]
        // Teste para verificar se CreateAsync retorna CreatedAtAction quando a criação é bem-sucedida
        public async Task CreateAsync_ShouldReturnCreatedAtAction_WhenCreationIsSuccessful()
        {
            // Arrange
            var responsibleDto = _fixture.Create<ResponsibleDto>();
            var responsible = _fixture.Create<Responsible>();
            var responsibleVm = _fixture.Create<ResponsibleVm>();

            _mapperMock.Setup(m => m.Map<Responsible>(responsibleDto)).Returns(responsible);
            _serviceMock.Setup(s => s.CreateAsync(responsible)).ReturnsAsync(OperationResult.Complete("Success"));
            _mapperMock.Setup(m => m.Map<ResponsibleVm>(responsible)).Returns(responsibleVm);

            // Act
            var result = await _controller.CreateAsync(responsibleDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetAsync), createdAtActionResult.ActionName);
            Assert.Equal(responsibleVm.Id, createdAtActionResult.RouteValues["id"]);
            Assert.Equal(responsibleVm, createdAtActionResult.Value);
        }

        [Fact]
        // Teste para verificar se GetAsync retorna ResponsibleVm
        public async Task GetAsyncShouldReturnResponsibleVm()
        {
            var responsible = _fixture.Create<Responsible>();
            var responsibleVm = _fixture.Create<ResponsibleVm>();

            _serviceMock.Setup(s => s.GetItemAsync(responsible.Id)).ReturnsAsync(responsible);
            _mapperMock.Setup(m => m.Map<ResponsibleVm>(responsible)).Returns(responsibleVm);

            var result = await _controller.GetAsync(responsible.Id);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedVm = Assert.IsType<ResponsibleVm>(okResult.Value);
            Assert.Equal(responsibleVm.Id, returnedVm.Id);
            Assert.Equal(responsibleVm.Nome, returnedVm.Nome);
            Assert.Equal(responsibleVm.Email, returnedVm.Email);
            Assert.Equal(responsibleVm.Area, returnedVm.Area);
        }

        [Fact]
        // Teste para verificar se GetListAsync retorna PagedResult
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            var filterDto = _fixture.Create<ResponsibleFilter>();
            var filter = _fixture.Create<ResponsibleFilter>();
            var pagedResult = _fixture.Create<PagedResult<Responsible>>();
            var pagedResultVm = _fixture.Create<PagedResult<ResponsibleVm>>();

            _mapperMock.Setup(m => m.Map<ResponsibleFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResult<ResponsibleVm>>(pagedResult)).Returns(pagedResultVm);

            var result = await _controller.GetListAsync(filterDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPagedResultVm = Assert.IsType<PagedResult<ResponsibleVm>>(okResult.Value);
            Assert.Equal(pagedResultVm.Result.Count(), returnedPagedResultVm.Result.Count());
            Assert.Equal(pagedResultVm.Result.First().Nome, returnedPagedResultVm.Result.First().Nome);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenUpdateIsSuccessful()
        {
            // Arrange
            var responsibleDto = _fixture.Create<ResponsibleDto>();
            var responsible = _fixture.Create<Responsible>();
            var responsibleVm = _fixture.Create<ResponsibleVm>();

            _serviceMock.Setup(s => s.GetItemAsync(responsible.Id)).ReturnsAsync(responsible);
            _mapperMock.Setup(m => m.Map<ResponsibleVm>(responsible)).Returns(responsibleVm);
            _serviceMock.Setup(s => s.UpdateAsync(responsible)).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.UpdateAsync(responsible.Id, responsibleDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(responsibleVm, okResult.Value);
        }

        [Fact]
        // Teste para verificar se DeleteAsync retorna NoContent quando a exclusão é bem-sucedida
        public async Task DeleteAsyncShouldReturnNoContentWhenDeleteIsSuccessful()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.DeleteAsync(id);

            // Assert
            var noContentResult = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContentResult.StatusCode);
        }
    }
}