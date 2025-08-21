using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
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
        public async Task UpdateAsyncShouldReturnOkResultWhenUpdateIsSuccessful()
        {
            // Arrange
            int knowledgeId = 1;
            KnowledgeDto knowledgeDto = new()
            {
                MemberId = 2,
                ApplicationId = 3,
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
                ApplicationId = 2,
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
                ApplicationId = 3,
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
        public async Task AssociateMultipleShouldReturnOkWhenAllAssociationsAreCreated()
        {
            // Arrange
            var items = new List<KnowledgeDto>
            {
                new KnowledgeDto { MemberId = 1, ApplicationId = 2, SquadId = 3, Status = KnowledgeStatus.Atual },
                new KnowledgeDto { MemberId = 1, ApplicationId = 5, SquadId = 3, Status = KnowledgeStatus.Passado }
            };

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Knowledge>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.AssociateMultiple(items);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task GetAllByMemberShouldReturnOkResultWithList()
        {
            // Arrange
            int memberId = 1;
            var knowledges = new List<Knowledge>
            {
                new Knowledge { MemberId = 1, ApplicationId = 2, SquadId = 3, Status = KnowledgeStatus.Atual },
                new Knowledge { MemberId = 1, ApplicationId = 5, SquadId = 3, Status = KnowledgeStatus.Passado }
            };
            var pagedResult = new PagedResult<Knowledge>
            {
                Result = knowledges,
                Page = 1,
                PageSize = 10,
                Total = 2
            };

            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<KnowledgeFilter>())).ReturnsAsync(pagedResult);

            // Act
            IActionResult result = await _controller.GetAllByMember(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedList = Assert.IsAssignableFrom<List<KnowledgeVm>>(okResult.Value);
            Assert.Equal(2, returnedList.Count);
        }
    }
}