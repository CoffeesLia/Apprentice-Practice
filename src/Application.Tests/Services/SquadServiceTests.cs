using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Xunit;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Application.Tests.Helpers;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Validators;
using System.Globalization;

namespace Application.Tests.Services
{ 
public class SquadServiceTests
{
    private readonly Mock<ISquadRepository> _squadRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly SquadService _squadService;

    public SquadServiceTests()
    {
        CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _squadRepositoryMock = new Mock<ISquadRepository>();

        var localizerFactory = LocalizerFactorHelper.Create();
        var squadValidator = new SquadValidator(localizerFactory);

        _unitOfWorkMock.Setup(u => u.SquadRepository).Returns(_squadRepositoryMock.Object);
        _squadService = new SquadService(_unitOfWorkMock.Object, localizerFactory, squadValidator);
    }

        [Fact]
        public async Task CreateAsyncReturnsConflictWhenSquadIsNull()
        {
            // Arrange
            var expectedMessage = SquadResources.SquadCannotBeNull;

            // Act
            var result = await _squadService.CreateAsync(null);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(expectedMessage, result.Message);
        }



        [Fact]
        public async Task CreateAsyncReturnsInvalidDataWhenValidationFails()
        {
            // Arrange
            var squad = new Squad { Name = "" }; 

            // Act
            var result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncReturnsConflictWhenNameAlreadyExists()
        {
            // Arrange
            var squad = new Squad { Name = "Existing Squad" };
            var validationResult = new ValidationResult(); // Validação sem erros
            var validatorMock = new Mock<IValidator<Squad>>(); // Mock do validador
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(true);

            var localizerFactory = LocalizerFactorHelper.Create();
            var localizer = localizerFactory.Create(typeof(SquadResources));
            var squadService = new SquadService(_unitOfWorkMock.Object, localizerFactory, validatorMock.Object);

            // Act
            var result = await squadService.CreateAsync(squad);

            // Assert
            var expectedMessage = localizer[nameof(SquadResources.SquadNameAlreadyExists)];
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(expectedMessage, result.Message);
        }



        [Fact]
        // Testa se o método CreateAsync retorna sucesso quando um Squad válido é criado
        public async Task CreateAsyncReturnsSuccessWhenValidSquadIsCreated()
        {
            // Arrange
            var squad = new Squad { Name = "New Squad" };
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(false);

            // Act
            var result = await _squadService.CreateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }


        [Fact]
        // Testa se o método UpdateAsync retorna não encontrado quando o Squad não existe
        public async Task UpdateAsyncReturnsNotFoundWhenSquadDoesNotExist()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Updated Squad" };
            var validatorMock = new Mock<IValidator<Squad>>(); // Declare and initialize the validator mock
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(new ValidationResult());
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squad.Id)).ReturnsAsync((Squad?)null); // Use nullable type

            var squadService = new SquadService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(nameof(SquadResources.SquadNotFound), result.Message);
        }


        [Fact]
        // Testa se o método UpdateAsync retorna dados inválidos quando a validação falha
        public async Task UpdateAsyncReturnsInvalidDataWhenValidationFails()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Updated Squad" };
            var validationResult = new ValidationResult(new[] { new ValidationFailure("Name", "Invalid name") });
            var validatorMock = new Mock<IValidator<Squad>>(); // Criação do mock local
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(validationResult);

            var squadService = new SquadService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }


        [Fact]
        // Testa se o método UpdateAsync retorna conflito quando o nome do Squad já existe
        public async Task UpdateAsyncReturnsConflictWhenNameAlreadyExists()
        {
            // Arrange
            var squad = new Squad { Id = 1, Name = "Existing Squad" };
            var validatorMock = new Mock<IValidator<Squad>>(); // Criação do mock local
            validatorMock.Setup(v => v.ValidateAsync(squad, default)).ReturnsAsync(new ValidationResult());
            _squadRepositoryMock.Setup(r => r.GetByIdAsync(squad.Id)).ReturnsAsync(new Squad());
            _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(squad.Name)).ReturnsAsync(true);

            var squadService = new SquadService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await squadService.UpdateAsync(squad);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(nameof(SquadResources.SquadNameAlreadyExists), result.Message);
        }


        [Fact]
    // Testa se o método DeleteAsync retorna não encontrado quando o Squad não existe
    public async Task DeleteAsyncReturnsNotFoundWhenSquadDoesNotExist()
    {
        // Arrange
        var squadId = 1;
        _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(squadId)).ReturnsAsync(false);

        // Act
        var result = await _squadService.DeleteAsync(squadId);

        // Assert
        Assert.Equal(OperationStatus.NotFound, result.Status);
        Assert.Equal(nameof(SquadResources.SquadNotFound), result.Message);
    }

    [Fact]
    // Testa se o método DeleteAsync retorna sucesso quando o Squad é deletado
    public async Task DeleteAsyncReturnsSuccessWhenSquadIsDeleted()
    {
        // Arrange
        var squadId = 1;
        _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(squadId)).ReturnsAsync(true);

        // Act
        var result = await _squadService.DeleteAsync(squadId);

        // Assert
        Assert.Equal(OperationStatus.Success, result.Status);
    }

    [Fact]
    // Testa se o método GetListAsync retorna um resultado paginado
    public async Task GetListAsyncReturnsPagedResult()
    {
        // Arrange
        var filter = new SquadFilter { Name = "Test" };
        var pagedResult = new PagedResult<Squad>
        {
            Result = new List<Squad> { new Squad { Name = "Test Squad" } },
            Page = 1,
            PageSize = 10,
            Total = 1
        };
        _squadRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

        // Act
        var result = await _squadService.GetListAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Result);
    }

    [Fact]
    // Testa se o método VerifyNameAlreadyExistsAsync retorna conflito quando o nome já existe
    public async Task VerifyNameAlreadyExistsAsyncReturnsConflictWhenNameExists()
    {
        // Arrange
        var name = "Existing Squad";
        _squadRepositoryMock.Setup(r => r.VerifyNameAlreadyExistsAsync(name)).ReturnsAsync(true);

        // Act
        var result = await _squadService.VerifyNameAlreadyExistsAsync(name);

        // Assert
        Assert.Equal(OperationStatus.Conflict, result.Status);
        Assert.Equal(nameof(SquadResources.SquadNameAlreadyExists), result.Message);
    }

    [Fact]
    public async Task VerifySquadExistsAsyncReturnsSuccessWhenSquadExists()
    {
        // Arrange
        var squadId = 1;
        _squadRepositoryMock.Setup(r => r.VerifySquadExistsAsync(squadId)).ReturnsAsync(true);

        // Act
        var result = await _squadService.VerifySquadExistsAsync(squadId);

        // Assert
        Assert.Equal(OperationStatus.Success, result.Status); // Changed from "Complete" to "Success"
    }
        

    }
}