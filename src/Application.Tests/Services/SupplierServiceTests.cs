using Application.Tests.Helpers;
using AutoFixture;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;

namespace Application.Tests.Services
{
    public class SupplierServiceTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<ISupplierRepository> _repositoryMock = new();
        private readonly SupplierService _service;

        public SupplierServiceTests()
        {
            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            _unitOfWorkMock.SetupGet(x => x.SupplierRepository).Returns(_repositoryMock.Object);
            _service = new SupplierService(_unitOfWorkMock.Object, localizerFactory, null!);
        }

        /// <summary>
        /// Given a supplier with duplicate part numbers,
        /// when CreateAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task CreateAsync_Fail_WhenHasDuplicatePartNumber()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            var partNumber = _fixture.Create<PartNumberSupplier>();
            supplier.PartNumbers!.Add(partNumber);
            supplier.PartNumbers.Add(partNumber);

            _repositoryMock
                .Setup(x => x.VerifyCodeExistsAsync(supplier.Code!))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(supplier);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(string.Format(SupplierResources.DuplicatePartNumbers, partNumber.PartNumberId), result.Message);
        }

        /// <summary>
        /// Given a supplier with an existing code,
        /// when CreateAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task CreateAsync_Fail_WhenAlreadyExistItemSameCode()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();

            _repositoryMock
                .Setup(x => x.VerifyCodeExistsAsync(supplier.Code!))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CreateAsync(supplier);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(SupplierResources.AlreadyExistCode, result.Message);
        }

        /// <summary>
        /// Given a supplier with part numbers,
        /// when CreateAsync is called,
        /// then it should succeed.
        /// </summary>
        [Fact]
        public async Task CreateAsync_Success_WhenHasPartNumbers()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            var partNumbers = _fixture.CreateMany<PartNumberSupplier>(2).ToArray();
            supplier.PartNumbers!.Add(partNumbers[0]);
            supplier.PartNumbers!.Add(partNumbers[1]);

            _repositoryMock
                .Setup(x => x.VerifyCodeExistsAsync(supplier.Code!))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(supplier);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.RegisteredSuccessfully, result.Message);
        }

        /// <summary>
        /// Given a supplier without part numbers,
        /// when CreateAsync is called,
        /// then it should succeed.
        /// </summary>
        [Fact]
        public async Task CreateAsync_Success_WhenThereAreNoPartNumbers()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();

            _repositoryMock
                .Setup(x => x.VerifyCodeExistsAsync(supplier.Code!))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CreateAsync(supplier);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.RegisteredSuccessfully, result.Message);
        }

        /// <summary>
        /// Given a supplier with part numbers,
        /// when DeleteAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_Fail_WhenHasPartNumbers()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            supplier.PartNumbers.Add(new Fixture().Create<PartNumberSupplier>());

            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(supplier.Id))
                .ReturnsAsync(supplier);

            // Act
            var result = await _service.DeleteAsync(supplier.Id);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(SupplierResources.Undeleted, result.Message);
        }

        /// <summary>
        /// Given a supplier that does not exist,
        /// when DeleteAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_Fail_WhenNotFound()
        {
            // Act
            var result = await _service.DeleteAsync(_fixture.Create<int>());

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(GeneralResources.NotFound, result.Message);
        }

        /// <summary>
        /// Given a valid supplier,
        /// when DeleteAsync is called,
        /// then it should succeed.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_Success()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(supplier.Id))
                .ReturnsAsync(supplier);

            // Act
            var result = await _service.DeleteAsync(supplier.Id);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.DeletedSuccessfully, result.Message);
        }

        /// <summary>
        /// Given a supplier ID,
        /// when GetItemAsync is called,
        /// then it should return the supplier.
        /// </summary>
        [Fact]
        public async Task GetItemAsync_Success()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();

            _repositoryMock
                .Setup(x => x.GetByIdAsync(supplier.Id, false))
                .ReturnsAsync(supplier);

            // Act
            var result = await _service.GetItemAsync(supplier.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(supplier.Id, result.Id);
            Assert.Equal(supplier.Address, result.Address);
            Assert.Equal(supplier.Code, result.Code);
            Assert.Equal(supplier.CompanyName, result.CompanyName);
            Assert.Equal(supplier.Phone, result.Phone);
        }

        /// <summary>
        /// Given a supplier filter,
        /// when GetListAsync is called,
        /// then it should return a list of suppliers.
        /// </summary>
        [Fact]
        public async Task GetListAsync_Success()
        {
            // Arrange
            var filter = _fixture.Create<SupplierFilter>();
            var pagedResult = _fixture.Create<PagedResult<Supplier>>();

            _repositoryMock
                .Setup(x => x.GetListAsync(It.IsAny<SupplierFilter>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _service.GetListAsync(filter);

            // Assert
            Assert.True(result.Result!.Any());
            Assert.True(result.Total > 0);
        }

        /// <summary>
        /// Given a supplier with duplicate part numbers,
        /// when UpdateAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_Fail_WhenHasDuplicatePartNumber()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            var partNumber = _fixture.Create<PartNumberSupplier>();
            supplier.PartNumbers.Add(partNumber);
            supplier.PartNumbers.Add(partNumber);

            // Act
            var result = await _service.UpdateAsync(supplier);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(string.Format(SupplierResources.DuplicatePartNumbers, partNumber.PartNumberId), result.Message);
        }

        /// <summary>
        /// Given a supplier that does not exist,
        /// when UpdateAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_Fail_WhenNotFound()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();

            // Act
            var result = await _service.UpdateAsync(supplier);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(GeneralResources.NotFound, result.Message);
        }

        /// <summary>
        /// Given a supplier with part numbers,
        /// when UpdateAsync is called,
        /// then it should succeed.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_Success_WhenHasPartNumbers()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();
            var partNumbers = _fixture.CreateMany<PartNumberSupplier>(2).ToArray();
            supplier.PartNumbers!.Add(partNumbers[0]);
            supplier.PartNumbers!.Add(partNumbers[1]);

            _repositoryMock
                .Setup(x => x.GetByIdAsync(supplier.Id, false))
                .ReturnsAsync(supplier);

            // Act
            var result = await _service.UpdateAsync(supplier);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.UpdatedSuccessfully, result.Message);
        }

        /// <summary>
        /// Given a supplier without part numbers,
        /// when UpdateAsync is called,
        /// then it should succeed.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_Success_WhenThereAreNoPartNumbers()
        {
            // Arrange
            var supplier = _fixture.Create<Supplier>();

            _repositoryMock
                .Setup(x => x.GetByIdAsync(supplier.Id, false))
                .ReturnsAsync(supplier);

            // Act
            var result = await _service.UpdateAsync(supplier);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.UpdatedSuccessfully, result.Message);
        }
    }
}
