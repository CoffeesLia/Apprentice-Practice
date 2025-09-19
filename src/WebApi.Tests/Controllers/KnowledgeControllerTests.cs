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
    public class KnowledgeControllerTests
    {
        private readonly Mock<IKnowledgeService> _serviceMock;
        private readonly KnowledgeController _controller;

        public KnowledgeControllerTests()
        {
            _serviceMock = new Mock<IKnowledgeService>();
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfiguration.CreateMapper();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactor = LocalizerFactorHelper.Create();
            _controller = new KnowledgeController(_serviceMock.Object, mapper, localizerFactor);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResultWhenUpdateIsSuccessful()
        {
            // Arrange
            int knowledgeId = 1;
            KnowledgeDto knowledgeDto = new()
            {
                MemberId = 2,
                ApplicationIds = new List<int> { 3 },
                SquadId = 4,
                Status = KnowledgeStatus.Atual
            };

            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Knowledge>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.UpdateAsync(knowledgeId, knowledgeDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenAssociationIsPast()
        {
            // Arrange
            var knowledgeDto = new KnowledgeDto
            {
                MemberId = 1,
                ApplicationIds = new List<int> { 2 },
                SquadId = 99,
                Status = KnowledgeStatus.Passado
            };

            _serviceMock
                .Setup(s => s.UpdateAsync(It.IsAny<Knowledge>()))
                .ReturnsAsync(OperationResult.Conflict("It's not allowed to update past associations."));

            // Act
            IActionResult result = await _controller.UpdateAsync(1, knowledgeDto);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenDeleteIsSuccessful()
        {
            // Arrange
            int knowledgeId = 1;
            _serviceMock.Setup(s => s.DeleteAsync(knowledgeId)).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.DeleteAsync(knowledgeId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnConflictWhenAssociationIsPast()
        {
            // Arrange
            int knowledgeId = 1;
            _serviceMock
                .Setup(s => s.DeleteAsync(knowledgeId))
                .ReturnsAsync(OperationResult.Conflict("It's not allowed to remove past associations."));

            // Act
            IActionResult result = await _controller.DeleteAsync(knowledgeId);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnOkResultWhenCreateIsSuccessful()
        {
            // Arrange
            var knowledgeDto = new KnowledgeDto
            {
                MemberId = 2,
                ApplicationIds = new List<int> { 3 },
                SquadId = 4,
                Status = KnowledgeStatus.Atual
            };

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Knowledge>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.CreateAsync(knowledgeDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnKnowledgeVmWhenExists()
        {
            // Arrange
            int knowledgeId = 1;
            var knowledge = new Knowledge
            {
                Id = 1,
                MemberId = 1,
                SquadId = 3,
                Status = KnowledgeStatus.Atual,
                Member = new() { Id = 1, Name = "Nome Teste", Role = "Cargo Teste", Cost = 1000m, Email = "teste@email.com" },
                Squad = new() { Id = 3, Name = "Nome Teste", Description = "Descrição Teste" }
            };
            knowledge.ApplicationIds.Add(3);

            // Act
            var result = await _controller.GetAsync(knowledgeId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<KnowledgeVm>>(result);
            Assert.IsType<OkObjectResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnNotFoundWhenNotExists()
        {
            // Arrange
            int knowledgeId = 99;
            _serviceMock.Setup(s => s.GetItemAsync(knowledgeId)).ReturnsAsync((Knowledge?)null);

            // Act
            var result = await _controller.GetAsync(knowledgeId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<KnowledgeVm>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultVm()
        {
            // Arrange
            var filterDto = new KnowledgeFilterDto
            {
                Page = 1,
                PageSize = 10,
                MemberId = 1,
                ApplicationId = 2,
                SquadId = 3,
                Status = KnowledgeStatus.Atual
            };
            var knowledge = new Knowledge
            {
                Id = 1,
                MemberId = 1,
                SquadId = 3,
                Status = KnowledgeStatus.Atual,
                Member = new() { Id = 1, Name = "Nome Teste", Role = "Cargo Teste", Cost = 1000m, Email = "teste@email.com" },
                Squad = new() { Id = 3, Name = "Nome Teste", Description = "Descrição Teste" }
            };
            knowledge.ApplicationIds.Add(2);

            var pagedResult = new PagedResult<Knowledge>
            {
                Result = new List<Knowledge> { knowledge },
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<KnowledgeFilter>())).ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PagedResultVm<KnowledgeVm>>(okResult.Value);
        }

        [Fact]
        public void KnowledgeFilterDtoShouldSetAndGetProperties()
        {
            // Arrange
            var filterDto = new KnowledgeFilterDto
            {
                Page = 2,
                PageSize = 20,
                MemberId = 5,
                ApplicationId = 10,
                SquadId = 15,
                Status = KnowledgeStatus.Atual
            };

            // Assert
            Assert.Equal(2, filterDto.Page);
            Assert.Equal(20, filterDto.PageSize);
            Assert.Equal(5, filterDto.MemberId);
            Assert.Equal(10, filterDto.ApplicationId);
            Assert.Equal(15, filterDto.SquadId);
            Assert.Equal(KnowledgeStatus.Atual, filterDto.Status);
        }

        [Fact]
        public void KnowledgeFilterDtoDefaultValuesShouldBeCorrect()
        {
            // Arrange
            var filterDto = new KnowledgeFilterDto
            {
                Page = 0,
                PageSize = 0
            };

            // Assert
            Assert.Equal(0, filterDto.MemberId);
            Assert.Equal(0, filterDto.ApplicationId);
            Assert.Equal(0, filterDto.SquadId);
            Assert.Equal(default(KnowledgeStatus), filterDto.Status);
        }
    }
}