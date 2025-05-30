using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Xunit;

namespace Application.Tests
{
    public class DummyEntity : EntityBase { }

    public class DummyEntityService : EntityServiceBase<DummyEntity>
    {
        private readonly IRepositoryEntityBase<DummyEntity> _repository;
        protected override IRepositoryEntityBase<DummyEntity> Repository => _repository;

        public DummyEntityService(
            IUnitOfWork unitOfWork,
            IStringLocalizerFactory localizerFactory,
            IValidator<DummyEntity> validator,
            IRepositoryEntityBase<DummyEntity> repository)
            : base(unitOfWork, localizerFactory, validator)
        {
            _repository = repository;
        }
    }

    public class EntityServiceBaseTests
    {
        private readonly Mock<IRepositoryEntityBase<DummyEntity>> _repositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock = new();
        private readonly Mock<IValidator<DummyEntity>> _validatorMock = new();
        private readonly Mock<IStringLocalizer> _localizerMock = new();

        private DummyEntityService CreateService()
        {
            _localizerFactoryMock.Setup(f => f.Create(It.IsAny<Type>())).Returns(_localizerMock.Object);
            _localizerMock.Setup(l => l[It.IsAny<string>()]).Returns(new LocalizedString("key", "Mensagem"));

            return new DummyEntityService(
                _unitOfWorkMock.Object,
                _localizerFactoryMock.Object,
                _validatorMock.Object,
                _repositoryMock.Object
            );
        }

        [Fact]
        public async Task CreateAsyncDeveCriarEntidadeERetornarSucesso()
        {
            // Arrange
            var service = CreateService();
            var entity = new DummyEntity();

            // Act
            var result = await service.CreateAsync(entity);

            // Assert
            _repositoryMock.Verify(r => r.CreateAsync(entity, true), Times.Once);
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal("Mensagem", result.Message);
        }

        [Fact]
        public async Task DeleteAsyncDeveRetornarNotFoundQuandoEntidadeNaoExiste()
        {
            // Arrange
            var service = CreateService();
            _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((DummyEntity?)null);

            // Act
            var result = await service.DeleteAsync(1);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal("Mensagem", result.Message);
        }

        [Fact]
        public async Task UpdateAsyncDeveRetornarNotFoundQuandoEntidadeNaoExiste()
        {
            // Arrange
            var service = CreateService();
            var entity = new DummyEntity { Id = 1 };
            _repositoryMock.Setup(r => r.GetByIdAsync(entity.Id)).ReturnsAsync((DummyEntity?)null);

            // Act
            var result = await service.UpdateAsync(entity);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal("Mensagem", result.Message);
        }

        [Fact]
        public async Task GetItemAsyncDeveRetornarEntidadeQuandoEncontrada()
        {
            // Arrange
            var service = CreateService();
            var entity = new DummyEntity { Id = 123 };
            _repositoryMock.Setup(r => r.GetByIdAsync(123)).ReturnsAsync(entity);

            // Act
            var result = await service.GetItemAsync(123);

            // Assert
            Assert.Same(entity, result);
        }
    }
}