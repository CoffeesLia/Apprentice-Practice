using Application.Tests.Helpers;
using AutoFixture;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
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
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IValidator<Area>> _validatorMock = new();
        private readonly AreaService _service;
        private readonly Fixture _fixture = new();

        public AreaServiceTests()
        {
            IStringLocalizerFactory localizerFactory = LocalizerFactorHelper.Create();
            _unitOfWorkMock
                .SetupGet(x => x.AreaRepository)
                .Returns(_repositoryMock.Object);

            _service = new AreaService(_unitOfWorkMock.Object, localizerFactory, _validatorMock.Object);
        }

        /// <summary>
        /// Given a valid area that already exist, 
        /// when the system tries to create a new area 
        /// then the system returns an error message.
        /// </summary>
        [Fact]
        public async Task CreateArea_NameAlreadyExists_Fail()
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
        /// Given a valid area that already exist, 
        /// when the system tries to create a new area 
        /// then the system returns an error message.
        /// </summary>
        [Fact]
        public async Task CreateArea_NameDoesntExists_Success()
        {
            // Arrange
            var area = _fixture.Create<Area>();

            // Act
            var result = await _service.CreateAsync(area);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
            Assert.Equal(GeneralResources.RegisteredSuccessfully, result.Message);
            _repositoryMock.Verify(x => x.GetByNameAsync(area.Name), Times.Once);
        }

        /// <summary>
        /// O nome da área deve respeitar o limite mínimo e máximo de caracteres.
        /// </summary>

        /// <summary>
        /// Caso o nome já exista, o sistema deve exibir uma mensagem de erro clara.
        /// </summary>

        /// <summary>
        /// O sistema deve confirmar o sucesso da criação da área.O sistema deve permitir a edição do nome da área.
        /// </summary>
    }
}
