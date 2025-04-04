/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Moq;
using Xunit;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.Application.Tests.Services
{
    public class MemberServiceTest
    {
        private readonly Mock<IMemberRepository> _memberRepositoryMock;
        private readonly Mock<IStringLocalizer<ServiceResources>> _localizerMock;
        private readonly MemberService _memberService;

        public MemberServiceTest()
        {
            _memberRepositoryMock = new Mock<IMemberRepository>();
            _localizerMock = new Mock<IStringLocalizer<ServiceResources>>();
            _memberService = new MemberService(_memberRepositoryMock.Object, _localizerMock.Object);
        }

        [Fact]
        public async Task AddEntityMember_ShouldThrowException_WhenEmailIsNotUnique()
        {
            //arrange
            var entityMember = new EntityMember
            {
                Id = Guid.NewGuid(),
                Name = "Ana",
                Role = "Developer",
                Cost = 1000,
                Email = "ana.souza7@exemplo.com"
            };

            _memberRepositoryMock.Setup(repo => repo.IsEmailUnique(entityMember.Email, entityMember.Id));
            _localizerMock.Setup(localizer => localizer["MemberEmailAlreadyExists"]).Returns(new LocalizedString("MemberEmailAlreadyExists", "This e-mail already exists"));

            //actEassert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _memberService.UpdateEntityMemberAsync(entityMember));
            _localizerMock.Verify(localizer => localizer["MemberEmailAlreadyExists"], Times.Once);
            Assert.Equal("This e-mail already exists", exception.Message);
        }

        /*
        [Fact]
        public async Task UpdateEntityMember_ShouldThrowException_WhenRequiredFieldsAreNotFilled()
        {
            //arrange
            var entityMember = new EntityMember
            {
                Id = Guid.NewGuid(),
                Name = "",
                Role = "",
                Cost = 0,
                Email = "ana.souza7@exemplo.com"
            };

            _localizerMock.Setup(localizer => localizer["MemberRequiredFieldsMissing"]).Returns(new LocalizedString("MemberRequiredFieldsMissing", "All required fields must be completed. "));

            //actEassert
            Assert.Throws<ArgumentException>(() => _memberService.AddEntityMember(entityMember));

        }

        [Fact]
        public async Task GetMemberByIdAsync_ShouldReturnMember_WhenMemberExists()
        {
            //arrange
            var id = Guid.NewGuid();
            var entityMember = new EntityMember
            {
                Id = id,
                Name = "Ana",
                Role = "Developer",
                Cost = 1000,
                Email = "ana.souza7@exemplo.com"
            };

            _memberRepositoryMock.Setup(repo => repo.GetMemberByIdAsync(id)).ReturnsAsync(entityMember);

            //act
            var result = await _memberService.GetMemberByIdAsync(id);

            //assert
            Assert.Equal(entityMember, result);
        }

        [Fact]
        public async Task GetMemberByIdAsync_ShouldReturnNull_WhenMemberDoesNotExist()
        {
            //arrange
            var id = Guid.NewGuid();

            _memberRepositoryMock.Setup(repo => repo.GetMemberByIdAsync(id)).ReturnsAsync((EntityMember)null);

            //act
            var result = await _memberService.GetMemberByIdAsync(id);

            //assert
            Assert.Null(result);
        }
    }
}
*/