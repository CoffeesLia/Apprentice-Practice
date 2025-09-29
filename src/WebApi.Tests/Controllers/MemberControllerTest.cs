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
    public class MemberControllerTest
    {
        private readonly Mock<IMemberService> _serviceMock;
        private readonly MemberController _controller;

        public MemberControllerTest()
        {
            _serviceMock = new Mock<IMemberService>();
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfiguration.CreateMapper();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactor = LocalizerFactorHelper.Create();
            _controller = new MemberController(_serviceMock.Object, mapper, localizerFactor);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenMemberIsValid()
        {
            // Arrange
            MemberDto memberDto = new()
            {
                Name = "Valid Name",
                Role = "Valid Role",
                Email = "valid@example.com",
                Cost = 100
            };

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<Member>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.CreateAsync(memberDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncShouldReturnMemberVmWhenMemberExists()
        {
            // Arrange
            int memberId = 1;
            Member member = new()
            {
                Id = memberId,
                Name = "Test Member",
                Role = "Test Role",
                Email = "test@example.com",
                Cost = 100
            };
            MemberVm memberVm = new()
            {
                Id = memberId,
                Name = "Test Member",
                Role = "Test Role",
                Email = "test@example.com",
                Cost = 100

            };

            _serviceMock.Setup(s => s.GetItemAsync(memberId)).ReturnsAsync(member);

            // Act
            ActionResult<MemberVm> result = await _controller.GetAsync(memberId);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(200, okResult.StatusCode);

            // comporação
            MemberVm actualMemberVm = Assert.IsType<MemberVm>(okResult.Value);
            Assert.Equal(memberVm.Id, actualMemberVm.Id);
            Assert.Equal(memberVm.Name, actualMemberVm.Name);
            Assert.Equal(memberVm.Role, actualMemberVm.Role);
            Assert.Equal(memberVm.Email, actualMemberVm.Email);
            Assert.Equal(memberVm.Cost, actualMemberVm.Cost);
        }

        [Fact]
        public async Task GetAsyncShouldReturnNotFoundWhenMemberDoesNotExist()
        {
            // Arrange
            int memberId = 1;

            _serviceMock.Setup(s => s.GetItemAsync(memberId)).ReturnsAsync((Member?)null);

            // Act
            ActionResult<MemberVm> result = await _controller.GetAsync(memberId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public void GetListAsyncShouldReturnPagedResultWhenCalledWithValidFilter()
        {
            // Arrange
            MemberFilter filter = new()
            {
                Name = "Test Name",
                Role = "Test Role",
                Email = "test@example.com",
                Cost = 100

            };
            PagedResult<Member> pagedResult = new()
            {
                Result =
                [
                    new Member
                    {
                        Name = "Test Name",
                        Role = "Test Role",
                        Email = "test@example.com",
                        Cost = 100
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
            PagedResultVm<MemberVm> mappedResult = mapper.Map<PagedResultVm<MemberVm>>(pagedResult);

            // Assert
            Assert.NotNull(mappedResult);
            Assert.Equal(1, mappedResult.Page);
            Assert.Equal(10, mappedResult.PageSize);
            Assert.Equal(1, mappedResult.Total);
            Assert.Single(mappedResult.Result);
            Assert.Equal("Test Name", mappedResult.Result.First().Name);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResultWhenMemberIsValid()
        {
            // Arrange
            MemberDto memberDto = new()
            {
                Name = "Valid Name",
                Role = "Valid Role",
                Email = "valid@example.com",
                Cost = 100
            };
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<Member>())).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.UpdateAsync(1, memberDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNoContentWhenDeletionIsSuccessful()
        {
            // Arrange
            int memberId = 1;
            _serviceMock.Setup(service => service.DeleteAsync(memberId)).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.DeleteAsync(memberId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
