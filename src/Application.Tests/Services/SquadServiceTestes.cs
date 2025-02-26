using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using System;
using Xunit;
namespace Stellantis.ProjectName.Tests.Services
{
    public class SquadServiceTests
    {
        [Fact]
        public void CreateSquad_ShouldThrowException_WhenNameIsEmpty()
        {
            var squadRepositoryMock = new Mock<ISquadRepository>();
            var squadService = new SquadService(squadRepositoryMock.Object);

            Assert.Throws<ArgumentException>(() => squadService.CreateSquad("", "Description"));
        }

        [Fact]
        public void CreateSquad_ShouldThrowException_WhenDescriptionIsEmpty()
        {
            var squadRepositoryMock = new Mock<ISquadRepository>();
            var squadService = new SquadService(squadRepositoryMock.Object);

            Assert.Throws<ArgumentException>(() => squadService.CreateSquad("SquadName", ""));
        }

        [Fact]
        public void CreateSquad_ShouldThrowException_WhenSquadNameAlreadyExists()
        {
            var squadRepositoryMock = new Mock<ISquadRepository>();
            squadRepositoryMock.Setup(repo => repo.GetByName(It.IsAny<string>())).Returns(new EntitieSquad());

            var squadService = new SquadService(squadRepositoryMock.Object);

            Assert.Throws<InvalidOperationException>(() => squadService.CreateSquad("ExistingSquad", "Description"));
        }

        [Fact]
        public void CreateSquad_ShouldAddSquad_WhenValid()
        {
            var squadRepositoryMock = new Mock<ISquadRepository>();
            squadRepositoryMock.Setup(repo => repo.GetByName(It.IsAny<string>())).Returns((EntitieSquad)null);

            var squadService = new SquadService(squadRepositoryMock.Object);

            squadService.CreateSquad("NewSquad", "Description");

            squadRepositoryMock.Verify(repo => repo.Add(It.IsAny<EntitieSquad>()), Times.Once);
        }
    }
}