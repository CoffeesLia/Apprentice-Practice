using Application.Tests.Helpers;
using AutoFixture;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using System.Linq.Expressions;
using Xunit;

namespace Application.Tests.Services
{
    public class EmployeeServiceTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IEmployeeRepository> _repositoryMock = new();
        private readonly EmployeeService _service;

        public EmployeeServiceTests()
        {
            var localizerFactory = LocalizerFactorHelper.Create();
            _unitOfWorkMock.SetupGet(x => x.EmployeeRepository).Returns(_repositoryMock.Object);
            _service = new EmployeeService(_unitOfWorkMock.Object, localizerFactory);
        }

        [Fact]
        public async Task CreateAsync_Success_ValidEmployee()
        {
            // Arrange
            var employee = _fixture.Create<Employee>();
            _repositoryMock
                .Setup(x => x.CreateAsync(employee, true))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateAsync(employee);

            // Assert
            Assert.True(result.Success, result.Message);
            _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Employee>(), true), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Fail_ArgumentNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _service.CreateAsync(null!));
        }

        [Fact]
        public async Task DeleteAsync_Success_ValidEmployee()
        {
            // Arrange
            var employee = _fixture.Create<Employee>();
            _repositoryMock
                .Setup(x => x.GetByIdAsync(employee.Id, false))
                .ReturnsAsync(employee);

            // Act
            var result = await _service.DeleteAsync(employee.Id);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.SuccessDelete, result.Message);
            _repositoryMock.Verify(x => x.DeleteAsync(employee, true), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Fail_NotFoundEmployee()
        {
            // Arrange
            _repositoryMock
                .Setup(x => x.GetByIdAsync(It.IsAny<int>(), false))
                .ReturnsAsync((Employee)null!);

            // Act
            var result = await _service.DeleteAsync(_fixture.Create<int>());

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(GeneralResources.NotFound, result.Message);
        }

        [Fact]
        public async Task GetItemAsync_ReturnEmployee()
        {
            // Arrange
            var employee = _fixture.Create<Employee>();
            _repositoryMock
                .Setup(x => x.GetByIdAsync(employee.Id, false))
                .ReturnsAsync(employee);

            // Act
            var result = await _service.GetItemAsync(employee.Id);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetItemAsync_ReturnNull_WhenNotExist()
        {
            // Arrange
            var result = await _service.GetItemAsync(0);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetListAsync_Success_ReturnEmployeeList()
        {
            // Arrange
            var employeeFilter = _fixture.Create<BaseFilter>();
            var list = _fixture.Create<List<Employee>>();
            var paginationEmployee = new PagedResult<Employee>()
            {
                Result = list,
                Total = list.Count
            };

            _repositoryMock
                .Setup(x => x.GetListAsync(It.IsAny<Expression<Func<Employee, bool>>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(paginationEmployee);

            // Act
            var pageResult = await _service.GetListAsync(employeeFilter);

            // Assert
            Assert.True(pageResult.Result.Any());
            Assert.True(pageResult.Total > 0);
        }


        [Fact]
        public async Task GetListAsync_Success_WhenFilterIsNull()
        {
            // Arrange
            var list = _fixture.Create<List<Employee>>();
            var paginationEmployee = new PagedResult<Employee>()
            {
                Result = list,
                Total = list.Count
            };
            _repositoryMock
                .Setup(x => x.GetListAsync(It.IsAny<Expression<Func<Employee, bool>>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(paginationEmployee);

            // Act
            var pageResult = await _service.GetListAsync(null!);

            // Assert
            Assert.True(pageResult.Result.Any());
            Assert.True(pageResult.Total > 0);
        }

        [Fact]
        public async Task UpdateAsync_Success_ValidEmployee()
        {
            // Arrange
            var employee = _fixture.Create<Employee>();
            var oldEmployee = _fixture.Create<Employee>();

            _repositoryMock.Setup(x => x.GetByIdAsync(employee.Id, false)).ReturnsAsync(oldEmployee);

            // Act
            var result = await _service.UpdateAsync(employee);

            // Assert
            Assert.True(result.Success, result.Message);
            _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Employee>(), true), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Fail_NotFoundEmployee()
        {
            // Arrange
            var employee = _fixture.Create<Employee>();

            _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), false)).ReturnsAsync((Employee)null!);

            // Act
            var result = await _service.UpdateAsync(employee);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(GeneralResources.NotFound, result.Message);
        }
    }
}
