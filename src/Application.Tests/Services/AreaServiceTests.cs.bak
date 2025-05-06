using Application.Tests.Helpers;
using AutoFixture;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.Validators;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;

namespace Application.Tests.Services
{
    public class AreaServiceTests
    {
        private readonly Mock<IAreaRepository> _repositoryMock = new();
        private readonly AreaService _service;
        private readonly Fixture _fixture = new();

        public AreaServiceTests()
        {
            // Integration tests
            var localizerFactory = LocalizerFactorHelper.Create();
            var areaValidator = new AreaValidator(localizerFactory);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock
                .SetupGet(x => x.AreaRepository)
                .Returns(_repositoryMock.Object);

            _service = new AreaService(unitOfWorkMock.Object, localizerFactory, areaValidator);
        }

        /// <summary>
        /// Given a valid area that already exist, 
        /// when the system tries to create a new area 
        /// then the system returns an error message.
        /// </summary>
        [Fact]
        public async Task CreateAsync_NameAlreadyExists_Conflict()
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
        /// Given an area with an invalid name,
        /// when the system tries to create a new area,
        /// then the system returns an error message.
        /// </summary>
        [Theory]
        [InlineData("T")]
        [InlineData("NameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNam")]
        public async Task CreateAsync_InvalidData(string name)
        {
            // Arrange
            var area = new Area(name);

            // Act
            var result = await _service.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(AreaResources.NameValidateLength, AreaValidator.MinimumLength, AreaValidator.MaximumLength), result.Errors.First());
        }

        /// <summary>
        /// Given a valid area that doesn't exist, 
        /// when the system tries to create a new area 
        /// then the system returns a success message.
        /// </summary>
        [Fact]
        public async Task CreateAsync_NameDoesntExists_Success()
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
        public async Task UpdateAsync_NameDoesntExists_Success()
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

        /// <summary>
        /// Given an area with an invalid name,
        /// when the system tries to update the area,
        /// then the system returns an error message.
        /// </summary>
        [Theory]
        [InlineData("T")]
        [InlineData("NameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNameInvalidNam")]
        public async Task UpdateAsync_InvalidData(string name)
        {
            // Arrange
            var area = new Area(name);

            // Act
            var result = await _service.UpdateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(string.Format(AreaResources.NameValidateLength, AreaValidator.MinimumLength, AreaValidator.MaximumLength), result.Errors.First());
        }

        ///<summary>
        /// Given a valid area that already exists,
        /// when the system tries to update the area,
        /// then the system returns an error message.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_NameAlreadyExists_Conflict()
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
        public async Task UpdateAsync_NotFound()
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

        /// <summary>
        ///  Given a valid area that doesn't have applications,
        ///  when the system tries to delete the area,
        ///  then the system returns an error message.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_DoesntHaveApplications_Success()
        {
            // Arrange
            var area = _fixture.Create<Area>();

            _repositoryMock
                .Setup(x => x.GetByIdAsync(area.Id))
                .ReturnsAsync(area);
            _repositoryMock
                .Setup(x => x.HasApplicationsAsync(area.Id))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteAsync(area.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(ServiceResources.DeletedSuccessfully, result.Message);
            _repositoryMock.Verify(x => x.GetByIdAsync(area.Id), Times.Once);
            _repositoryMock.Verify(x => x.HasApplicationsAsync(area.Id), Times.Once);
            _repositoryMock.Verify(x => x.DeleteAsync(area, true), Times.Once);
        }


        /// <summary>
        ///  Given a valid area that doesn't have applications,
        ///  when the system tries to delete the area,
        ///  then the system returns an error message.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_NotFoound()
        {
            // Arrange
            var area = _fixture.Create<Area>();

            _repositoryMock
                .Setup(x => x.HasApplicationsAsync(area.Id))
                .ReturnsAsync(false);
            _repositoryMock
                .Setup(x => x.GetByIdAsync(area.Id))
                .ReturnsAsync((Area?)null);

            // Act
            var result = await _service.DeleteAsync(area.Id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(ServiceResources.NotFound, result.Message);
            _repositoryMock.Verify(x => x.GetByIdAsync(area.Id), Times.Once);
            _repositoryMock.Verify(x => x.HasApplicationsAsync(area.Id), Times.Once);
            _repositoryMock.Verify(x => x.DeleteAsync(area, true), Times.Never);
        }

        /// <summary>
        ///  Given a valid area that have applications,
        ///  when the system tries to delete the area,
        ///  then the system returns an error message.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_HaveApplications_Conflit()
        {
            // Arrange
            var area = _fixture.Create<Area>();

            _repositoryMock
                .Setup(x => x.HasApplicationsAsync(area.Id))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(area.Id);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(AreaResources.Undeleted, result.Message);
            _repositoryMock.Verify(x => x.HasApplicationsAsync(area.Id), Times.Once);
            _repositoryMock.Verify(x => x.DeleteAsync(area, true), Times.Never);
        }

        /// <summary>
        /// O sistema deve permitir acessar detalhes da área.
        /// Deve exibir, no mínimo, o nome da área e a lista de aplicações vinculadas.
        /// </summary>
        [Fact]
        public async Task GetItemAsync()
        {
            // Arrange
            var area = _fixture.Create<Area>();
            _repositoryMock
                .Setup(x => x.GetByIdAsync(area.Id))
                .ReturnsAsync(area);

            // Act
            var result = await _service.GetItemAsync(area.Id);

            // Assert
            Assert.Equal(area, result);
            _repositoryMock.Verify(x => x.GetByIdAsync(area.Id), Times.Once);
        }

        /// <summary>
        /// Caso a área não exista, o sistema deve exibir uma mensagem de erro apropriada.
        /// </summary>
        [Fact]
        public async Task GetItemAsync_NotFound()
        {
            // Arrange
            var area = _fixture.Create<Area>();
            _repositoryMock
                .Setup(x => x.GetByIdAsync(area.Id))
                .ReturnsAsync((Area?)null);

            // Act
            var result = await _service.GetItemAsync(area.Id);

            // Assert
            Assert.Null(result);
            _repositoryMock.Verify(x => x.GetByIdAsync(area.Id), Times.Once);
        }
    }
}
