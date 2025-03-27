using System;
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
        public void AddEntityMember_ShouldThrowException_WhenEmailIsNotUnique()
        {
            //arrange
            var entityMember = new EntityMember
            {
                Name = "Ana",
                Role = "Developer",
                Cost = 1000,
                Email = "ana.souza7@exemplo.com"
            };

            _memberRepositoryMock.Setup(repo => repo.IsEmailUnique(entityMember.Email)).Returns(false);

            //actEassert
            Assert.Throws<InvalidOperationException>(() => _memberService.AddEntityMember(entityMember));
        }

        /*
        [Fact]
        public void AddEntityMember_ShouldThrowException_WhenRequiredFieldsAreNotFilled()
        {
            //arrange
            var entityMember = new EntityMember
            {
                Name = "",
                Role = "",
                Cost = 0,
                Email = "ana.souza7@exemplo.com"
            };

            //actEassert
            Assert.Throws<ArgumentException>(() => _memberService.AddEntityMember(entityMember));
        }
        */

        [Fact]
        public void AddEntityMember_ShouldAddMember_WhenAllFieldsAreValid()
        {
            //arrange
            var entityMember = new EntityMember
            {
                Name = "Ana",
                Role = "Developer",
                Cost = 1000,
                Email = "ana.souza7@exemplo.com"
            };

            _memberRepositoryMock.Setup(repo => repo.IsEmailUnique(entityMember.Email)).Returns(true);

            //act
            _memberService.AddEntityMember(entityMember);

            //assert
            _memberRepositoryMock.Verify(repo => repo.AddEntityMember(entityMember), Times.Once);
        }
    }
}