using Application.Tests.Helpers;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Domain.Enums;
using System.Globalization;
using Xunit;

namespace Application.Tests.Services
{
    public class PartNumberServiceTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IPartNumberRepository> _repositoryMock = new();
        private readonly PartNumberService _service;

        public PartNumberServiceTests()
        {
            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            _unitOfWorkMock.SetupGet(x => x.PartNumberRepository).Returns(_repositoryMock.Object);
            _service = new PartNumberService(_unitOfWorkMock.Object, localizerFactory);
        }

        /// <summary>
        /// Given a part number with an existing code,
        /// when CreateAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task CreateAsync_Fail_WhenCodeExists()
        {
            // Arrange
            var partNumber = new Fixture().Create<PartNumber>();
            _repositoryMock
                .Setup(x => x.VerifyCodeExists(partNumber.Code!))
                .Returns(true);

            // Act
            var result = await _service.CreateAsync(partNumber);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(PartNumberResources.AlreadyExistCode, result.Message);
        }

        /// <summary>
        /// Given a part number with an existing code,
        /// when UpdateAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_Fail_WhenCodeExists()
        {
            // Arrange
            var partNumber = new Fixture().Create<PartNumber>();
            _repositoryMock
                .Setup(x => x.VerifyCodeExists(partNumber.Code!))
                .Returns(true);

            // Act
            var result = await _service.UpdateAsync(partNumber);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(PartNumberResources.AlreadyExistCode, result.Message);
        }

        /// <summary>
        /// Given a part number that does not exist,
        /// when UpdateAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_Fail_WhenNotFound()
        {
            // Arrange
            var partNumber = new Fixture().Create<PartNumber>();
            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((PartNumber)null!);

            // Act
            var result = await _service.UpdateAsync(partNumber);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(GeneralResources.NotFound, result.Message);
        }

        /// <summary>
        /// Given different cultures,
        /// when localization is tested,
        /// then it should return localized strings.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("it")]
        [InlineData("pt-BR")]
        [InlineData("es-AR")]
        public void Localization_ReturnLocalizedStrings_ForDifferentCultures(string? culture)
        {
            // Arrange
            if (culture != null)
                CultureInfo.CurrentUICulture = new CultureInfo(culture);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddLocalization();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var localizerFactory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();
            var localizer = localizerFactory.Create(typeof(GeneralResources));

            foreach (var key in localizer.GetAllStrings())
            {
                // Act
                var localizedString = localizer[key];

                // Assert
                Assert.False(string.IsNullOrEmpty(localizedString));
                Console.WriteLine($"Culture: {CultureInfo.CurrentUICulture.Name}, Localized String: {localizedString}");
            }
        }

        /// <summary>
        /// Given a part number filter,
        /// when GetListAsync is called,
        /// then it should return a list of part numbers.
        /// </summary>
        [Fact]
        public async Task GetListAsync_ReturnPartNumbers()
        {
            // Arrange
            var partNumberFilter = new Fixture().Create<PartNumberFilter>();
            var paginationPartNumber = new Fixture().Create<PagedResult<PartNumber>>();
            _repositoryMock.Setup(x => x.GetListAsync(It.IsAny<PartNumberFilter>())).ReturnsAsync(paginationPartNumber);

            // Act
            var result = await _service.GetListAysnc(partNumberFilter);

            // Assert
            Assert.True(result.Result!.Any());
            Assert.True(result.Total > 0);
        }

        /// <summary>
        /// Given a part number with suppliers,
        /// when DeleteAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_Fail_WhenHasSuppliers()
        {
            // Arrange
            var partNumber = new PartNumber(_fixture.Create<string>(), _fixture.Create<string>(), PartNumberType.Internal);
            partNumber.Suppliers.Add(_fixture.Create<PartNumberSupplier>());
            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(partNumber);

            // Act
            var result = await _service.DeleteAsync(partNumber.Id);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(PartNumberResources.Undeleted, result.Message);
        }

        /// <summary>
        /// Given a part number with vehicles,
        /// when DeleteAsync is called,
        /// then it should fail.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_Fail_WhenHasVehicle()
        {
            // Arrange
            var partNumber = new PartNumber(_fixture.Create<string>(), _fixture.Create<string>(), PartNumberType.Internal);
            partNumber.Vehicles.Add(_fixture.Create<VehiclePartNumber>());
            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(partNumber);

            // Act
            var result = await _service.DeleteAsync(partNumber.Id);

            // Assert
            Assert.False(result.Success, result.Message);
            Assert.Equal(PartNumberResources.Undeleted, result.Message);
        }

        /// <summary>
        /// Given a part number that does not exist,
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
        /// Given a valid part number,
        /// when DeleteAsync is called,
        /// then it should succeed.
        /// </summary>
        [Fact]
        public async Task DeleteAsync_Success()
        {
            // Arrange
            var partNumber = new PartNumber(_fixture.Create<string>(), _fixture.Create<string>(), PartNumberType.Internal);
            _repositoryMock
                .Setup(x => x.GetFullByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(partNumber);

            // Act
            var result = await _service.DeleteAsync(partNumber.Id);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.DeletedSuccessfully, result.Message);
        }

        /// <summary>
        /// Given a valid part number ID,
        /// when GetItemAsync is called,
        /// then it should return the part number.
        /// </summary>
        [Fact]
        public async Task GetItemAsync_Success()
        {
            // Arrange
            var partNumber = _fixture.Create<PartNumber>();
            _repositoryMock
                .Setup(x => x.GetByIdAsync(partNumber.Id, false))
                .ReturnsAsync(partNumber);

            // Act
            var result = await _service.GetItemAsync(partNumber.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(partNumber.Code, result.Code);
            Assert.Equal(partNumber.Description, result.Description);
            Assert.Equal(partNumber.Type, result.Type);
        }

        /// <summary>
        /// Given a valid part number,
        /// when CreateAsync is called,
        /// then it should succeed.
        /// </summary>
        [Theory]
        [InlineData("12323")]
        [InlineData("12345678901")]
        public async Task CreateAsync_Succeed_WhenIsValid(string code)
        {
            // Arrange
            var partNumber = new Fixture().Create<PartNumber>();
            partNumber.Code = code;
            _repositoryMock
                .Setup(x => x.VerifyCodeExists(partNumber.Code))
                .Returns(false);

            // Act
            var result = await _service.CreateAsync(partNumber);

            // Assert
            Assert.True(result.Success, result.Message);
            Assert.Equal(GeneralResources.RegisteredSuccessfully, result.Message);
            _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<PartNumber>(), true), Times.Once);
        }

        /// <summary>
        /// Given a valid part number,
        /// when UpdateAsync is called,
        /// then it should succeed.
        /// </summary>
        [Fact]
        public async Task UpdateAsync_Succeed_WhenIsValid()
        {
            // Arrange
            var partNumber = new Fixture().Create<PartNumber>();
            _repositoryMock
                .Setup(x => x.GetByIdAsync(partNumber.Id, false))
                .ReturnsAsync(partNumber);

            // Act
            var result = await _service.UpdateAsync(partNumber);

            // Assert
            Assert.True(result.Success, result.Message);
            _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PartNumber>(), true), Times.Once);
        }
    }
}
