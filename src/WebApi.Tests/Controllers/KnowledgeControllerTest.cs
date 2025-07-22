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
    public class KnowledgeControllerTest
    {
        private readonly Mock<IKnowledgeService> _serviceMock;
        private readonly KnowledgeController _controller;
        private readonly IMapper _mapper;

        public KnowledgeControllerTest()
        {
            _serviceMock = new Mock<IKnowledgeService>();
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            _mapper = mapperConfiguration.CreateMapper();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactor = LocalizerFactorHelper.Create();
            _controller = new KnowledgeController(_serviceMock.Object, _mapper, localizerFactor);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenKnowledgeIsValid()
        {
            // Arrange
            KnowledgeDto knowledgeDto = new()
            {
                MemberId = 1,
                ApplicationId = 2
            };

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Knowledge>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.CreateAsync(knowledgeDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnKnowledgeVmWhenKnowledgeExists()
        {
            // Arrange
            int knowledgeId = 1;
            Knowledge knowledge = new()
            {
                Id = knowledgeId,
                MemberId = 2,
                ApplicationId = 3,
                SquadIdAtAssociationTime = 4
            };
            KnowledgeVm knowledgeVm = new()
            {
                Id = knowledgeId,
                MemberId = 2,
                ApplicationId = 3,
                SquadIdAtAssociationTime = 4
            };

            _serviceMock.Setup(s => s.GetItemAsync(knowledgeId)).ReturnsAsync(knowledge);

            // Act
            ActionResult<KnowledgeVm> result = await _controller.GetAsync(knowledgeId);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);

            KnowledgeVm actualVm = Assert.IsType<KnowledgeVm>(okResult.Value);
            Assert.Equal(knowledgeVm.Id, actualVm.Id);
            Assert.Equal(knowledgeVm.MemberId, actualVm.MemberId);
            Assert.Equal(knowledgeVm.ApplicationId, actualVm.ApplicationId);
            Assert.Equal(knowledgeVm.SquadIdAtAssociationTime, actualVm.SquadIdAtAssociationTime);
        }

        [Fact]
        public async Task GetAsyncShouldReturnNotFoundWhenKnowledgeDoesNotExist()
        {
            // Arrange
            int knowledgeId = 1;
            _serviceMock.Setup(s => s.GetItemAsync(knowledgeId)).ReturnsAsync((Knowledge?)null);

            // Act
            ActionResult<KnowledgeVm> result = await _controller.GetAsync(knowledgeId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void GetListAsyncShouldReturnPagedResultWhenCalledWithValidFilter()
        {
            // Arrange
            KnowledgeFilter filter = new()
            {
                MemberId = 1,
                ApplicationId = 2,
                SquadId = 3
            };
            PagedResult<Knowledge> pagedResult = new()
            {
                Result =
                [
                    new Knowledge
                    {
                        Id = 1,
                        MemberId = 1,
                        ApplicationId = 2,
                        SquadIdAtAssociationTime = 3
                    }
                ],
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            MapperConfiguration config = new(cfg => cfg.AddProfile<AutoMapperProfile>());
            IMapper mapper = config.CreateMapper();

            // Act
            PagedResultVm<KnowledgeVm> mappedResult = _mapper.Map<PagedResultVm<KnowledgeVm>>(pagedResult);

            // Assert
            Assert.NotNull(mappedResult);
            Assert.Equal(1, mappedResult.Page);
            Assert.Equal(10, mappedResult.PageSize);
            Assert.Equal(1, mappedResult.Total);
            Assert.Single(mappedResult.Result);
            Assert.Equal(1, mappedResult.Result.First().MemberId);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResultWhenKnowledgeIsValid()
        {
            // Arrange
            KnowledgeDto knowledgeDto = new()
            {
                MemberId = 1,
                ApplicationId = 2
            };
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Knowledge>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.UpdateAsync(1, knowledgeDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenDeletionIsSuccessful()
        {
            // Arrange
            int knowledgeId = 1;
            _serviceMock.Setup(service => service.DeleteAsync(knowledgeId)).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.DeleteAsync(knowledgeId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
