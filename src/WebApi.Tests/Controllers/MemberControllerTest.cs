using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.ViewModels;
using Xunit;

namespace WebApi.Tests.Controllers
{
    public class MemberControllerTest
    {
        private readonly Mock<IMemberService> _memberServiceMock;
        private readonly Mock<IStringLocalizer<ServiceResources>> _localizerMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly MemberController _memberController;

        public MemberControllerTest()
        {
            _memberServiceMock = new Mock<IMemberService>();
            _localizerMock = new Mock<IStringLocalizer<ServiceResources>>();
            _mapperMock = new Mock<IMapper>();
            _memberController = new MemberController(_memberServiceMock.Object, _localizerMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task UpdateMemberShouldReturnOkWhenUpdateIsSuccessful()
        {
            //arrange
            var memberVm = new MemberVm
            {
                Id = Guid.NewGuid(),
                Name = "Ana",
                Role = "Developer",
                Cost = 1000,
                Email = "ana.souza7@exemplo.com"
            };

            var entityMember = new EntityMember
            {
                Id = memberVm.Id,
                Name = memberVm.Name,
                Role = memberVm.Role,
                Cost = memberVm.Cost,
                Email = memberVm.Email
            };

            _mapperMock.Setup(m => m.Map<EntityMember>(memberVm)).Returns(entityMember);
            _localizerMock.Setup(localizer => localizer["MemberUpdatedSuccessfully"]).Returns(new LocalizedString("MemberUpdatedSuccessfully", "Member updated successfully. "));

            //act
            var result = await _memberController.UpdateMember(memberVm.Id, memberVm);

            //assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Member updated successfully. ", okResult.Value);
        }

        [Fact]
        public async Task UpdateMember_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            //arrange
            var memberVm = new MemberVm
            {
                Id = Guid.NewGuid(),
                Name = "Ana",
                Role = "Developer",
                Cost = 1000,
                Email = "ana.souza7@exemplo.com"
            };

            var entityMember = new EntityMember
            {
                Id = memberVm.Id,
                Name = memberVm.Name,
                Role = memberVm.Role,
                Cost = memberVm.Cost,
                Email = memberVm.Email
            };

            _mapperMock.Setup(m => m.Map<EntityMember>(memberVm)).Returns(entityMember);
            _memberServiceMock.Setup(service => service.UpdateEntityMemberAsync(entityMember)).ThrowsAsync(new Exception("Update faled."));

            //act
            var result = await _memberController.UpdateMember(memberVm.Id, memberVm);

            //assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Update failed.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetMembers_ShouldReturnOk_WhenMembersExist()
        {
            //arrange
            var members = new List<EntityMember>
            {
                new EntityMember {  Id = Guid.NewGuid(), Name = "Ana", Role = "Developer", Email = "ana.souza7@exemplo.com" },
                new EntityMember {  Id = Guid.NewGuid(), Name = "Raquel", Role = "Manager", Email = "raquel.souza77@exemplo.com" }
            };

            var memberVms = new List<MemberVm>
            {
                new MemberVm { Id = members[0].Id, Name = "Ana", Role = "Developer", Email = "ana.souza7@exemplo.com" },
                new MemberVm { Id = members[1].Id, Name = "Raquel", Role = "Manager", Email = "raquel.souza77@exemplo.com" }
            };

            _memberServiceMock.Setup(service => service.GetMembersAsync(null, null, null)).ReturnsAsync(members);
            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<MemberVm>>(members)).Returns(memberVms);

            //act
            var result = await _memberController.GetMembers(null, null, null);

            //assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMembers = Assert.IsType<List<MemberVm>>(okResult.Value);
            Assert.Equal(2, returnedMembers.Count);

        }

        [Fact]
        public async Task GetMembers_ShouldReturnEmptyList_WhenNoMembersExist()
        {
            //arrange
            _memberServiceMock.Setup(service => service.GetMembersAsync(null, null, null)).ReturnsAsync(new List<EntityMember>());
            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<MemberVm>>(It.IsAny<List<EntityMember>>())).Returns(new List<MemberVm>());

            //act 
            var result = await _memberController.GetMembers(null, null, null);

            //assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMembers = Assert.IsType<List<MemberVm>>(okResult.Value);
            Assert.Empty(returnedMembers);
        }

        [Fact]
        public async Task DeleteMember_ShouldReturnOk_WhenDeleteIsSuccessful()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            _localizerMock.Setup(localizer => localizer["MemberDeletedSuccessfully"]).Returns(new LocalizedString("MemberDeletedSuccessfully", "Member deleted successfully."));

            // Act
            var result = await _memberController.DeleteMember(memberId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Member deleted successfully.", okResult.Value);
        }

        [Fact]
        public async Task DeleteMember_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            _memberServiceMock.Setup(service => service.DeleteMemberAsync(memberId)).Throws(new InvalidOperationException("Delete failed."));

            // Act
            var result = await _memberController.DeleteMember(memberId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Delete failed.", badRequestResult.Value);
        }
    }
}
