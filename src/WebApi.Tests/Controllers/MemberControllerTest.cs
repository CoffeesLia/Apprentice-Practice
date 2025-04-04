using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;
using Xunit;


namespace WebApi.Tests.Controllers
{
    public class MemberControllerTest
    {
        private readonly Mock<IMemberService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly MemberControllerBase _controller;

        public MemberControllerTest()
        {
            _serviceMock = new Mock<IMemberService>();
            _mapperMock = new Mock<IMapper>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            var localizer = new Mock<IStringLocalizer>();

            _localizerFactoryMock.Setup(f => f.Create(typeof(MemberResource))).Returns(localizer.Object);

            _controller = new MemberControllerBase(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenMemberIsValid()
        {
            // Arrange
            var memberDto = new MemberDto
            {
                Name = "Valid Name",
                Role = "Valid Role",
                Email = "valid@example.com",
                Cost = 100
            };
            var member = new Member
            {
                Name = "Valid Name",
                Role = "Valid Role",
                Email = "valid@example.com",
                Cost = 100

            };
            var memberVm = new MemberVm
            {
                Name = "Valid Name",
                Role = "Valid Role",
                Email = "valid@example.com",
                Cost = 100

            };

            _mapperMock.Setup(m => m.Map<Member>(It.IsAny<MemberDto>())).Returns(member);
            _mapperMock.Setup(m => m.Map<MemberVm>(It.IsAny<Member>())).Returns(memberVm);
            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Member>())).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.CreateAsync(memberDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnMemberVmWhenMemberExists()
        {
            // Arrange
            var memberId = 1;
            var member = new Member
            {
                Id = memberId,
                Name = "Test Member",
                Role = "Test Role",
                Email = "test@example.com",
                Cost = 100
            };
            var memberVm = new MemberVm
            {
                Id = memberId,
                Name = "Test Member",
                Role = "Test Role",
                Email = "test@example.com",
                Cost = 100

            };

            _serviceMock.Setup(s => s.GetItemAsync(memberId)).ReturnsAsync(member);
            _mapperMock.Setup(m => m.Map<MemberVm>(member)).Returns(memberVm);

            // Act
            var result = await _controller.GetAsync(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(memberVm, okResult.Value);
        }

        [Fact]
        public async Task GetAsyncShouldReturnNotFoundWhenMemberDoesNotExist()
        {
            // Arrange
            var memberId = 1;

            _serviceMock.Setup(s => s.GetItemAsync(memberId)).ReturnsAsync((Member?)null);

            // Act
            var result = await _controller.GetAsync(memberId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultWhenCalledWithValidFilter()
        {
            // Arrange
            var filterDto = new MemberFilterDto
            {
                Name = "Test Name",
                Role = "Test Role",
                Email = "test@example.com",
                Cost = 100

            };
            var filter = new MemberFilter
            {
                Name = "Test Name",
                Role = "Test Role",
                Email = "test@example.com",
                Cost = 100

            };
            var pagedResult = new PagedResult<Member>
            {
                Result = new List<Member>
                {
                    new Member
                    {
                        Name = "Test Name",
                        Role = "Test Role",
                        Email = "test@example.com",
                        Cost = 100
                    }
                },
                Page = 1,
                PageSize = 10,
                Total = 1
            };
            var pagedVmResult = new PagedResultVm<MemberVm>
            {
                Result = new List<MemberVm>
                {
                    new MemberVm
                    {
                        Name = "Test Name",
                        Role = "Test Role",
                        Email = "test@example.com",
                        Cost = 100

                    }
                },
                Page = 1,
                PageSize = 10,
                Total = 1
            };

            _mapperMock.Setup(m => m.Map<MemberFilter>(filterDto)).Returns(filter);
            _serviceMock.Setup(s => s.GetListAsync(filter)).ReturnsAsync(pagedResult);
            _mapperMock.Setup(m => m.Map<PagedResultVm<MemberVm>>(pagedResult)).Returns(pagedVmResult);

            // Act
            var result = await _controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            Assert.Equal(pagedVmResult, okResult.Value);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResultWhenMemberIsValid()
        {
            // Arrange
            var memberDto = new MemberDto
            {
                Id = 1,
                Name = "Valid Name",
                Role = "Valid Role",
                Email = "valid@example.com",
                Cost = 100

            };
            var memberVm = new MemberVm
            {
                Id = 1,
                Name = "Valid Name",
                Role = "Valid Role",
                Email = "valid@example.com",
                Cost = 100

            };

            _mapperMock.Setup(m => m.Map<Member>(It.IsAny<MemberDto>())).Returns(new Member
            {
                Id = 1,
                Name = "Valid Name",
                Role = "Valid Role",
                Email = "valid@example.com",
                Cost = 100

            });
            _mapperMock.Setup(m => m.Map<MemberVm>(It.IsAny<Member>())).Returns(memberVm);

            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Member>())).ReturnsAsync(OperationResult.Complete("Success"));

            // Act
            var result = await _controller.UpdateAsync(1, memberDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenDeletionIsSuccessful()
        {
            // Arrange
            int memberId = 1;
            _serviceMock.Setup(service => service.DeleteAsync(memberId)).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.DeleteAsync(memberId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
