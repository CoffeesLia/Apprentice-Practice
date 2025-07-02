using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace WebApi.Tests.Controllers
{
    public class KnowledgeControllerTest
    {
        private readonly Mock<IKnowledgeService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly KnowledgeController _controller;

        public KnowledgeControllerTest()
        {
            _serviceMock = new Mock<IKnowledgeService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            _controller = new KnowledgeController(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }

        [Fact]
        public async Task CreateAsyncReturnsOk()
        {
            // Arrange
            var dto = new KnowledgeDto
            {
                MemberId = 1,
                ApplicationId = 2,
                LeaderSquadId = 1,
                CurrentSquadId = 1
            };

            var entity = new Knowledge { MemberId = 1, ApplicationId = 2, Id = 10 };
            var vm = new KnowledgeVm { Id = 10 };

            _mapperMock
                .Setup(m => m.Map<Knowledge>(It.IsAny<KnowledgeDto>()))
                .Returns(entity);

            _mapperMock
                .Setup(m => m.Map<KnowledgeVm>(It.IsAny<Knowledge>()))
                .Returns(vm);
            _serviceMock.Setup(service => service.CreateAsync(entity)).ReturnsAsync(OperationResult.Complete());



            // Act
            var result = await _controller.CreateAsync(dto);

            // Assert
            var createdAt = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(vm, createdAt.Value);
        }


        [Fact]
        public async Task AssociateAsyncReturnsNoContent()
        {
            var dto = new KnowledgeDto { MemberId = 1, ApplicationId = 2, CurrentSquadId = 3 };
            _serviceMock.Setup(s => s.CreateAssociationAsync(dto.MemberId, dto.ApplicationId, dto.CurrentSquadId))
                .Returns(Task.CompletedTask);

            var result = await _controller.AssociateAsync(dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task RemoveAssociationAsyncReturnsNoContent()
        {
            var dto = new KnowledgeDto { MemberId = 1, ApplicationId = 2, LeaderSquadId = 3 };
            _serviceMock.Setup(s => s.RemoveAssociationAsync(dto.MemberId, dto.ApplicationId, dto.LeaderSquadId))
                .Returns(Task.CompletedTask);

            var result = await _controller.RemoveAssociationAsync(dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ListApplicationsByMemberAsyncReturnsOk()
        {
            int memberId = 1;
            _serviceMock.Setup(s => s.ListApplicationsByMemberAsync(memberId))
                .ReturnsAsync([]);

            var result = await _controller.ListApplicationsByMemberAsync(memberId);

            Assert.IsType<OkObjectResult>(result.Result);
        }

        [Fact]
        public async Task ListMembersByApplicationAsyncReturnsOk()
        {
            int applicationId = 2;
            _serviceMock.Setup(s => s.ListMembersByApplicationAsync(applicationId))
                .ReturnsAsync(new List<Member>());

            var result = await _controller.ListMembersByApplicationAsync(applicationId);

            Assert.IsType<OkObjectResult>(result.Result);
        }
    }
}
