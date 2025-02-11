using Application.Tests.Helpers;
using AutoFixture;
using FluentValidation;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Intrinsics.X86;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Tests.Services
{
    public class AreaServiceTests
    {
        private readonly Mock<IAreaRepository> _repositoryMock = new();
        private readonly Mock<IValidator<Area>> _validatorMock = new();
        private readonly AreaService _service;
        private readonly Fixture _fixture = new();

        public AreaServiceTests()
        {
            var localizerFactory = LocalizerFactorHelper.Create();
            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.AreaRepository)
                .Returns(_repositoryMock.Object);

            _service = new AreaService(unitOfWorkMock.Object, localizerFactory, _validatorMock.Object);
        }

        /// <summary>
        /// Given a valid area that already exist, 
        /// when the system tries to create a new area 
        /// then the system returns an error message.
        /// </summary>
        [Fact]
        public async Task CreateAreaAsync_NameAlreadyExists_Fail()
        {
            // Arrange
            var area = _fixture.Create<Area>();
            _repositoryMock
                .Setup(x => x.GetByNameAsync(area.Name))
                .ReturnsAsync(area);

            // Act
            var result = await _service.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(AreaResources.AlreadyExists, result.Message);
            _repositoryMock.Verify(x => x.GetByNameAsync(area.Name), Times.Once);
        }


        /// <summary>
        /// Given a valid area that doesn't exist, 
        /// when the system tries to create a new area 
        /// then the system returns a success message.
        /// </summary>
        [Fact]
        public async Task CreateAreaAsync_NameDoesntExists_Success()
        {
            // Arrange
            var area = _fixture.Create<Area>();

            // Act
            var result = await _service.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(ServiceResources.RegisteredSuccessfully, result.Message);
            _repositoryMock.Verify(x => x.GetByNameAsync(area.Name), Times.Once);
        }

        /// <summary>
        /// Given a list of areas,
        /// when the system tries to list them,
        /// then the system returns the list.
        /// </summary>
        [Fact]
        public async Task GetListAsync_AllAreas()
        {
            // Arrange
            var areas = _fixture.CreateMany<Area>(3);
            var pagedResult = new PagedResult<Area> { Result = areas };

            _repositoryMock
                .Setup(x => x.GetListAsync(It.IsAny<AreaFilter>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _service.GetListAsync();

            // Assert
            Assert.Equal(areas, result.Result);
            _repositoryMock.Verify(x => x.GetListAsync(It.IsAny<AreaFilter>()), Times.Once);
        }

        /// <summary>
        /// Given a list of areas,
        /// when the system tries to list them filtered by name,
        /// then the system returns the list filtered by name.
        /// </summary>
        [Fact]
        public async Task GetListAsync_FilterByName()
        {
            // Arrange
            var name = _fixture.Create<string>();
            var filter = new AreaFilter { Name = name };
            var areas = _fixture
                .CreateMany<int>()
                .Select(x => new Area(name) { Id = x });
            var pagedResult = new PagedResult<Area> { Result = areas };

            _repositoryMock
                .Setup(x => x.GetListAsync(filter))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _service.GetListAsync(filter);

            // Assert
            Assert.Equal(areas, result.Result);
            _repositoryMock.Verify(x => x.GetListAsync(filter), Times.Once);
        }

        /// <summary>
        /// Given a list of areas,
        /// when the system tries to list them ordered by name,
        /// then the system returns the list ordered by name.
        /// </summary>
        [Fact]
        public async Task GetListAsync_OrderedByName()
        {
            // Arrange
            var filter = new AreaFilter { Sort = nameof(Area.Name), SortDir = "ASC" };
            var areas = _fixture.CreateMany<Area>(3);
            var pagedResult = new PagedResult<Area> { Result = areas };
            _repositoryMock
                .Setup(x => x.GetListAsync(filter))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _service.GetListAsync(filter);

            // Assert
            Assert.Equal(areas, result.Result);
            _repositoryMock.Verify(x => x.GetListAsync(filter), Times.Once);
        }

        /// <summary>
        /// Given a valid area that doesn't exist,
        /// when the system tries to update the area,
        /// then the system returns an error message.
        /// </summary>
        [Fact]
        public async Task UpdateAreaAsync_NameDoesntExists_Success()
        {
            // Arrange
            var oldArea = _fixture.Create<Area>();
            var areaToUpdate = new Area(_fixture.Create<string>())
            {
                Id = oldArea.Id
            };

            _repositoryMock
                .Setup(x => x.GetByNameAsync(areaToUpdate.Name))
                .ReturnsAsync((Area?)null);
            _repositoryMock
                .Setup(x => x.GetByIdAsync(oldArea.Id))
                .ReturnsAsync(oldArea);

            // Act
            var result = await _service.UpdateAsync(areaToUpdate);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(ServiceResources.UpdatedSuccessfully, result.Message);
            _repositoryMock.Verify(x => x.GetByNameAsync(areaToUpdate.Name), Times.Once);
            _repositoryMock.Verify(x => x.GetByIdAsync(oldArea.Id), Times.Once);
        }

        ///<summary>
        /// Given a valid area that already exists,
        /// when the system tries to update the area,
        /// then the system returns an error message.
        /// </summary>
        [Fact]
        public async Task UpdateAreaAsync_NameAlreadyExists_Fail()
        {
            // Arrange
            var oldArea = _fixture.Create<Area>();
            var areaToUpdate = new Area(_fixture.Create<string>())
            {
                Id = oldArea.Id
            };
            var areaSameName = new Area(_fixture.Create<string>())
            {
                Name = areaToUpdate.Name
            };

            _repositoryMock
                .Setup(x => x.GetByNameAsync(areaToUpdate.Name))
                .ReturnsAsync(areaSameName);
            _repositoryMock
                .Setup(x => x.GetByIdAsync(oldArea.Id))
                .ReturnsAsync(oldArea);

            // Act
            var result = await _service.UpdateAsync(areaToUpdate);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(AreaResources.AlreadyExists, result.Message);
            _repositoryMock.Verify(x => x.GetByNameAsync(areaToUpdate.Name), Times.Once);
            _repositoryMock.Verify(x => x.GetByIdAsync(oldArea.Id), Times.Never);
        }

        ///<summary>
        /// Given a valid area that already exists,
        /// when the system tries to update the area,
        /// then the system returns an error message.
        /// </summary>
        [Fact]
        public async Task UpdateAreaAsync_NotFound()
        {
            // Arrange
            var area = _fixture.Create<Area>();

            _repositoryMock
                .Setup(x => x.GetByIdAsync(area.Id))
                .ReturnsAsync((Area?)null);

            // Act
            var result = await _service.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(ServiceResources.NotFound, result.Message);
            _repositoryMock.Verify(x => x.GetByIdAsync(area.Id), Times.Once);
        }
        ///<summary>
        /// O sistema deve respeitar os limites de caracteres do nome da área.
        /// </summary>
    }
}
