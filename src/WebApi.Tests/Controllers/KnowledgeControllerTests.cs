using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
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
        public async Task UpdateAsyncShouldReturnOkResultWhenUserIsNotSquadLeader()
        {
            // Arrange
            int knowledgeId = 1;
            KnowledgeDto knowledgeDto = new()
            {
                MemberId = 2,
                ApplicationId = 3,
                SquadId = 4
            };

            // simula que o usuário não é líder do squad, mas o serviço retorna sucesso
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Knowledge>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.UpdateAsync(knowledgeId, knowledgeDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnConflictWhenUserIsNotSquadLeader()
        {
            // Arrange
            int knowledgeId = 1;

            // simula que o usuário não é líder do squad
            _serviceMock.Setup(s => s.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(OperationResult.Conflict("Usuário não é líder do squad"));

            // Act
            IActionResult result = await _controller.DeleteAsync(knowledgeId);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResultWhenUserIsSquadLeader()
        {
            // Arrange
            int knowledgeId = 1;
            KnowledgeDto knowledgeDto = new()
            {
                MemberId = 2,
                ApplicationId = 3,
                SquadId = 4
            };

            // simula que o usuário é líder do squad
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Knowledge>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.UpdateAsync(knowledgeId, knowledgeDto);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenUserIsSquadLeader()
        {
            // Arrange
            int knowledgeId = 1;

            // simula que o usuário é líder do squad
            _serviceMock.Setup(s => s.DeleteAsync(knowledgeId)).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.DeleteAsync(knowledgeId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnConflictWhenAssociationIsPast()
        {
            // Arrange
            var knowledgeDto = new KnowledgeDto
            {
                MemberId = 1,
                ApplicationId = 2,
                SquadId = 99 // Squad diferente do atual
            };

            _serviceMock
                .Setup(s => s.UpdateAsync(It.IsAny<Knowledge>()))
                .ReturnsAsync(OperationResult.Conflict("It's not allowed to remove past associations."));

            // Act
            IActionResult result = await _controller.UpdateAsync(1, knowledgeDto);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
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
    }
}
