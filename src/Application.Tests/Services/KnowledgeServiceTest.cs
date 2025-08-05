using System.Globalization;
using FluentValidation;
using FluentValidation.Results;
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
    public class KnowledgeServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IKnowledgeRepository> _knowledgeRepositoryMock;
        private readonly Mock<IMemberRepository> _memberRepositoryMock;
        private readonly Mock<IApplicationDataRepository> _applicationRepositoryMock;
        private readonly KnowledgeService _knowledgeService;

        public KnowledgeServiceTest()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _knowledgeRepositoryMock = new Mock<IKnowledgeRepository>();
            _memberRepositoryMock = new Mock<IMemberRepository>();
            _applicationRepositoryMock = new Mock<IApplicationDataRepository>();

            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = Helpers.LocalizerFactorHelper.Create();
            KnowledgeValidator knowledgeValidator = new(localizer);

            _unitOfWorkMock.Setup(u => u.KnowledgeRepository).Returns(_knowledgeRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(_memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(_applicationRepositoryMock.Object);

            _knowledgeService = new KnowledgeService(_unitOfWorkMock.Object, localizer, knowledgeValidator);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            Knowledge knowledge = new()
            {
                MemberId = 0,
                ApplicationId = 0
            };
            ValidationResult validationResult = new([new ValidationFailure("MemberId", "Member is required")]);
            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(validationResult);

            var service = new KnowledgeService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenAssociationExists()
        {
            // Arrange
            Knowledge knowledge = new()
            {
                MemberId = 1,
                ApplicationId = 2,
                AssociatedSquadId = 1
            };

            _memberRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Member { Id = 1, SquadId = 1, Name = "M", Role = "R", Email = "e@e.com", Cost = 1 });
            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new ApplicationData("App") { Id = 2, SquadId = 1, ProductOwner = "PO", ConfigurationItem = "CI" });
            _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(1, 2)).ReturnsAsync(true);

            // Act
            OperationResult result = await _knowledgeService.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(KnowledgeResource.AssociationAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenKnowledgeIsValid()
        {
            // Arrange
            Knowledge knowledge = new()
            {
                MemberId = 1,
                ApplicationId = 2,
                AssociatedSquadId = 1
            };

            _memberRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Member { Id = 1, SquadId = 1, Name = "M", Role = "R", Email = "e@e.com", Cost = 1 });
            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new ApplicationData("App") { Id = 2, SquadId = 1, ProductOwner = "PO", ConfigurationItem = "CI" });
            _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(1, 2)).ReturnsAsync(false);
            _knowledgeRepositoryMock.Setup(r => r.CreateAssociationAsync(1, 2, 1)).Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            OperationResult result = await _knowledgeService.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenKnowledgeDoesNotExist()
        {
            // Arrange
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Knowledge?)null);

            // Act
            OperationResult result = await _knowledgeService.GetItemAsync(1);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(KnowledgeResource.AssociationNotFound, result.Message);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnSuccessWhenKnowledgeExists()
        {
            // Arrange
            Knowledge knowledge = new()
            {
                Id = 1,
                MemberId = 1,
                ApplicationId = 2,
                AssociatedSquadId = 1
            };

            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);

            // Act
            OperationResult result = await _knowledgeService.GetItemAsync(knowledge.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            Knowledge knowledge = new()
            {
                Id = 1,
                MemberId = 1,
                ApplicationId = 2,
                AssociatedSquadId = 1
            };

            ValidationResult validationResult = new([new ValidationFailure("MemberId", "Member is required")]);
            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(validationResult);

            var service = new KnowledgeService(_unitOfWorkMock.Object, Helpers.LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.UpdateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenKnowledgeDoesNotExist()
        {
            // Arrange
            Knowledge knowledge = new()
            {
                Id = 1,
                MemberId = 1,
                ApplicationId = 2,
                AssociatedSquadId = 1
            };

            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync((Knowledge?)null);

            // Act
            OperationResult result = await _knowledgeService.UpdateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(KnowledgeResource.AssociationNotFound, result.Message);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnSuccessWhenKnowledgeIsValid()
        {
            // Arrange
            Knowledge knowledge = new()
            {
                Id = 1,
                MemberId = 1,
                ApplicationId = 2,
                AssociatedSquadId = 1
            };

            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            // Act
            OperationResult result = await _knowledgeService.UpdateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            KnowledgeFilter filter = new() { MemberId = 1, ApplicationId = 2, SquadId = 3 };
            List<Knowledge> knowledgeList =
            [
                new() { Id = 1, MemberId = 1, ApplicationId = 2, AssociatedSquadId = 3 },
                new() { Id = 2, MemberId = 1, ApplicationId = 2, AssociatedSquadId = 3 }
            ];
            PagedResult<Knowledge> pagedResult = new()
            {
                Result = knowledgeList,
                Page = 1,
                PageSize = 10,
                Total = 2
            };

            _knowledgeRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            PagedResult<Knowledge> result = await _knowledgeService.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Total);
            Assert.Equal(knowledgeList, result.Result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenKnowledgeDoesNotExist()
        {
            // Arrange
            int knowledgeId = 1;
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledgeId)).ReturnsAsync((Knowledge?)null);

            // Act
            OperationResult result = await _knowledgeService.DeleteAsync(knowledgeId);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenKnowledgeIsDeleted()
        {
            // Arrange
            Knowledge knowledge = new()
            {
                Id = 1,
                MemberId = 1,
                ApplicationId = 2,
                AssociatedSquadId = 1
            };

            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(new Member { Id = knowledge.Id, Role = "SquadLeader", SquadId = knowledge.AssociatedSquadId, Name = "Líder", Email = "l@l.com", Cost = 1 });
            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.ApplicationId)).ReturnsAsync(new ApplicationData("App") { Id = knowledge.ApplicationId, SquadId = knowledge.AssociatedSquadId, ProductOwner = "PO", ConfigurationItem = "CI" });


            // Act
            OperationResult result = await _knowledgeService.DeleteAsync(knowledge.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}
