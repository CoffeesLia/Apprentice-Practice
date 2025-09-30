using Application.Tests.Helpers;
using AutoFixture;
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
using System.Globalization;
using Xunit;

namespace Application.Tests.Services
{
    public class KnowledgeServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IKnowledgeRepository> _knowledgeRepositoryMock;
        private readonly KnowledgeService _knowledgeService;
        private readonly Fixture _fixture;

        public KnowledgeServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _knowledgeRepositoryMock = new Mock<IKnowledgeRepository>();

            var localizer = LocalizerFactorHelper.Create();
            var validatorMock = new Mock<IValidator<Knowledge>>();

            _unitOfWorkMock.Setup(u => u.KnowledgeRepository).Returns(_knowledgeRepositoryMock.Object);

            _knowledgeService = new KnowledgeService(_unitOfWorkMock.Object, localizer, validatorMock.Object);

            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task CreateAsyncWhenValidationFailsReturnsInvalidData()
        {
            // Arrange
            var knowledge = _fixture.Build<Knowledge>()
                .With(k => k.MemberId, 0)
                .Create();
            knowledge.ApplicationIds.Add(0);

            var validationResult = new ValidationResult([new ValidationFailure("MemberId", "MemberId is required")]);
            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(validationResult);

            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await service.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncWhenAssociationAlreadyExistsReturnsConflict()
        {
            // Arrange
            var knowledge = _fixture.Build<Knowledge>()
                .With(k => k.Status, KnowledgeStatus.Atual)
                .Create();
            knowledge.ApplicationIds.Add(1);

            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(new ValidationResult());

            var memberRepositoryMock = new Mock<IMemberRepository>();
            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.MemberId)).ReturnsAsync(
                new Member
                {
                    Id = knowledge.MemberId,
                    SquadId = knowledge.SquadId,
                    Name = "Test Name",
                    Role = "Test Role",
                    Email = "test@email.com",
                    Cost = 1m
                }
            );
            foreach (var appId in knowledge.ApplicationIds)
            {
                applicationRepositoryMock.Setup(r => r.GetByIdAsync(appId)).ReturnsAsync(new ApplicationData("App") { Id = appId, SquadId = knowledge.SquadId });
                _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(knowledge.MemberId, appId, knowledge.SquadId, knowledge.Status)).ReturnsAsync(true);
            }
            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);

            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await service.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(KnowledgeResource.AssociationAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateAsyncWhenSuccessfulReturnsSuccess()
        {
            // Arrange
            var member = _fixture.Build<Member>()
                .With(m => m.Id, 1)
                .With(m => m.Name, "Valid Name")
                .With(m => m.Email, "valid.email@example.com")
                .With(m => m.Cost, 1m)
                .With(m => m.SquadId, 1)
                .Create();

            var application = _fixture.Build<ApplicationData>()
                .With(a => a.Id, 2)
                .With(a => a.Name, "App Teste")
                .With(a => a.SquadId, 1)
                .Create();

            var squad = new Squad { Id = 1, Name = "Squad Test" };

            var knowledge = _fixture.Build<Knowledge>()
                .With(k => k.MemberId, member.Id)
                .With(k => k.SquadId, squad.Id)
                .With(k => k.Member, member)
                .With(k => k.Squad, squad)
                .With(k => k.Status, KnowledgeStatus.Atual)
                .Create();
            knowledge.Applications.Add(application);
            knowledge.ApplicationIds.Add(application.Id);

            var validatorMock = new Mock<IValidator<Knowledge>>();
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(validationResult);

            var memberRepositoryMock = new Mock<IMemberRepository>();
            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            var squadRepositoryMock = new Mock<ISquadRepository>();

            memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.MemberId)).ReturnsAsync(member);
            foreach (var appId in knowledge.ApplicationIds)
                applicationRepositoryMock.Setup(r => r.GetByIdAsync(appId)).ReturnsAsync(application);
            squadRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.SquadId)).ReturnsAsync(squad);

            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.SquadRepository).Returns(squadRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            foreach (var appId in knowledge.ApplicationIds)
                _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(knowledge.MemberId, appId, knowledge.SquadId, knowledge.Status)).ReturnsAsync(false);

            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await service.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemDoesNotExistReturnsNotFound()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Knowledge?)null);

            // Act
            var result = await _knowledgeService.GetItemAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemExistsReturnsSuccess()
        {
            // Arrange
            var knowledge = _fixture.Create<Knowledge>();
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);

            // Act
            var result = await _knowledgeService.GetItemAsync(knowledge.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenKnowledgeDoesNotExistReturnsNotFound()
        {
            // Arrange
            var knowledge = _fixture.Create<Knowledge>();
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync((Knowledge?)null);

            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(new ValidationResult());

            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await service.UpdateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(KnowledgeResource.AssociationNotFound, result.Message);
        }

        [Fact]
        public async Task UpdateAsyncWhenAssociationIsPastReturnsConflict()
        {
            // Arrange
            var knowledge = _fixture.Build<Knowledge>()
                .With(k => k.Status, KnowledgeStatus.Passado)
                .Create();

            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);

            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(new ValidationResult());

            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await service.UpdateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(KnowledgeResource.CannotEditOrRemovePastAssociation, result.Message);
        }

        [Fact]
        public async Task DeleteAsyncWhenItemDoesNotExistReturnsNotFound()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Knowledge?)null);

            // Act
            var result = await _knowledgeService.DeleteAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenAssociationIsPastReturnsConflict()
        {
            // Arrange
            var knowledge = _fixture.Build<Knowledge>()
                .With(k => k.Status, KnowledgeStatus.Passado)
                .Create();

            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);

            // Act
            var result = await _knowledgeService.DeleteAsync(knowledge.Id);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(KnowledgeResource.CannotEditOrRemovePastAssociation, result.Message);
        }

        [Fact]
        public async Task DeleteAsyncWhenSuccessfulReturnsSuccess()
        {
            // Arrange
            var member = _fixture.Build<Member>()
                .With(m => m.Id, 1)
                .With(m => m.SquadId, 10)
                .Create();

            var application = _fixture.Build<ApplicationData>()
                .With(a => a.Id, 2)
                .With(a => a.SquadId, 10)
                .Create();

            var knowledge = _fixture.Build<Knowledge>()
                .With(k => k.Id, 1)
                .With(k => k.MemberId, member.Id)
                .With(k => k.SquadId, member.SquadId)
                .With(k => k.Status, KnowledgeStatus.Atual)
                .Create();
            knowledge.ApplicationIds.Add(application.Id);

            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);

            var memberRepositoryMock = new Mock<IMemberRepository>();
            memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.MemberId)).ReturnsAsync(member);
            memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(member); // performingUser

            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            applicationRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.ApplicationIds.First())).ReturnsAsync(application);

            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);

            // Act
            var result = await _knowledgeService.DeleteAsync(knowledge.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncWithMemberIdFilterReturnsFilteredResults()
        {
            // Arrange
            var filter = new KnowledgeFilter { MemberId = 1 };
            var knowledges = _fixture.Build<Knowledge>()
                .With(k => k.MemberId, 1)
                .CreateMany(3)
                .ToList();

            _knowledgeRepositoryMock
                .Setup(r => r.GetListAsync(It.Is<KnowledgeFilter>(f => f.MemberId == 1)))
                .ReturnsAsync(new PagedResult<Knowledge> { Result = knowledges, Page = 1, PageSize = 10, Total = 3 });

            _knowledgeRepositoryMock
                .Setup(r => r.GetListAsync(It.Is<KnowledgeFilter>(f => f.ApplicationId == 2)))
                .ReturnsAsync(new PagedResult<Knowledge> { Result = [], Page = 1, PageSize = 10, Total = 2 });

            _knowledgeRepositoryMock
                .Setup(r => r.GetListAsync(It.Is<KnowledgeFilter>(f => f.SquadId == 5)))
                .ReturnsAsync(new PagedResult<Knowledge> { Result = [], Page = 1, PageSize = 10, Total = 1 });

            _knowledgeRepositoryMock
                .Setup(r => r.GetListAsync(It.Is<KnowledgeFilter>(f => f.Status == KnowledgeStatus.Atual)))
                .ReturnsAsync(new PagedResult<Knowledge> { Result = [], Page = 1, PageSize = 10, Total = 4 });

            // Act
            var result = await _knowledgeService.GetListAsync(filter);

            // Assert
            Assert.Equal(1, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(4, result.Total);
        }

        [Fact]
        public async Task GetListAsyncWithApplicationIdFilterReturnsFilteredResults()
        {
            // Arrange
            var filter = new KnowledgeFilter { ApplicationId = 2 };
            _knowledgeRepositoryMock
                .Setup(r => r.GetListAsync(It.Is<KnowledgeFilter>(f => f.ApplicationId == 2)))
                .ReturnsAsync(new PagedResult<Knowledge> { Result = [], Page = 1, PageSize = 10, Total = 2 });

            // Act
            var result = await _knowledgeService.GetListAsync(filter);

            // Assert
            Assert.Equal(2, result.Total);
        }

        [Fact]
        public async Task GetListAsyncWithSquadIdFilterReturnsFilteredResults()
        {
            // Arrange
            var filter = new KnowledgeFilter { SquadId = 5 };
            _knowledgeRepositoryMock
                .Setup(r => r.GetListAsync(It.Is<KnowledgeFilter>(f => f.SquadId == 5)))
                .ReturnsAsync(new PagedResult<Knowledge> { Result = [], Page = 1, PageSize = 10, Total = 1 });

            // Act
            var result = await _knowledgeService.GetListAsync(filter);

            // Assert
            Assert.Equal(1, result.Total);
        }

        [Fact]
        public async Task GetListAsyncWithStatusFilterReturnsFilteredResults()
        {
            // Arrange
            var filter = new KnowledgeFilter { Status = KnowledgeStatus.Atual };
            _knowledgeRepositoryMock
                .Setup(r => r.GetListAsync(It.Is<KnowledgeFilter>(f => f.Status == KnowledgeStatus.Atual)))
                .ReturnsAsync(new PagedResult<Knowledge> { Result = [], Page = 1, PageSize = 10, Total = 4 });

            // Act
            var result = await _knowledgeService.GetListAsync(filter);

            // Assert
            Assert.Equal(4, result.Total);
        }

        [Fact]
        public async Task GetListAsyncWhenFilterIsNullReturnsAll()
        {
            // Arrange
            var knowledges = _fixture.Build<Knowledge>().CreateMany(2).ToList();
            _knowledgeRepositoryMock
                .Setup(r => r.GetListAsync(It.IsAny<KnowledgeFilter>()))
                .ReturnsAsync(new PagedResult<Knowledge> { Result = knowledges, Page = 1, PageSize = 10, Total = 2 });

            // Act
            var result = await _knowledgeService.GetListAsync(null!);

            // Assert
            Assert.Equal(2, result.Total);
        }

        [Fact]
        public async Task UpdateAsyncWhenMemberOrApplicationNotFoundReturnsNotFound()
        {
            // Arrange
            var knowledge = _fixture.Build<Knowledge>()
                 .With(k => k.Status, KnowledgeStatus.Atual)
                 .Create();
            knowledge.ApplicationIds.Add(1); // Adicione pelo menos um ID

            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);

            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(new ValidationResult());

            var memberRepositoryMock = new Mock<IMemberRepository>();
            memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.MemberId)).ReturnsAsync((Member?)null);

            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            applicationRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.ApplicationIds.First())).ReturnsAsync((ApplicationData?)null);

            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);

            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await service.UpdateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenMemberAndApplicationSquadIdMismatchReturnsConflict()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).With(m => m.SquadId, 1).Create();
            var application = _fixture.Build<ApplicationData>().With(a => a.Id, 2).With(a => a.SquadId, 2).Create();
            var knowledge = _fixture.Build<Knowledge>()
               .With(k => k.Id, 10)
               .With(k => k.MemberId, member.Id)
               .With(k => k.Status, KnowledgeStatus.Atual)
               .Create();
            knowledge.ApplicationIds.Add(application.Id);

            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);

            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(new ValidationResult());

            var memberRepositoryMock = new Mock<IMemberRepository>();
            memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.MemberId)).ReturnsAsync(member);

            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            applicationRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.ApplicationIds.First())).ReturnsAsync(application);

            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);

            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await service.UpdateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenAssociationAlreadyExistsReturnsConflict()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).With(m => m.SquadId, 1).Create();
            var application = _fixture.Build<ApplicationData>().With(a => a.Id, 2).With(a => a.SquadId, 1).Create();
            var knowledge = _fixture.Build<Knowledge>()
                .With(k => k.Id, 10)
                .With(k => k.MemberId, member.Id)
                .With(k => k.Status, KnowledgeStatus.Atual)
                .Create();
            knowledge.ApplicationIds.Add(application.Id);

            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);

            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(new ValidationResult());

            var memberRepositoryMock = new Mock<IMemberRepository>();
            memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.MemberId)).ReturnsAsync(member);

            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            applicationRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.ApplicationIds.First())).ReturnsAsync(application);

            var squadRepositoryMock = new Mock<ISquadRepository>();
            squadRepositoryMock.Setup(r => r.GetByIdAsync(member.SquadId)).ReturnsAsync(new Squad { Id = member.SquadId });

            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.SquadRepository).Returns(squadRepositoryMock.Object);

            _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(member.Id, application.Id, member.SquadId, knowledge.Status)).ReturnsAsync(true);

            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await service.UpdateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenSuccessfulReturnsSuccess()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).With(m => m.SquadId, 1).Create();
            var application = _fixture.Build<ApplicationData>().With(a => a.Id, 2).With(a => a.SquadId, 1).Create();
            var squad = new Squad { Id = 1, Name = "Squad Test" };
            var knowledge = _fixture.Build<Knowledge>()
                  .With(k => k.Id, 10)
                  .With(k => k.MemberId, member.Id)
                  .With(k => k.Status, KnowledgeStatus.Atual)
                  .Create();
            knowledge.ApplicationIds.Add(application.Id);

            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);

            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(new ValidationResult());

            var memberRepositoryMock = new Mock<IMemberRepository>();
            memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.MemberId)).ReturnsAsync(member);

            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            applicationRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.ApplicationIds.First())).ReturnsAsync(application);

            var squadRepositoryMock = new Mock<ISquadRepository>();
            squadRepositoryMock.Setup(r => r.GetByIdAsync(member.SquadId)).ReturnsAsync(squad);

            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.SquadRepository).Returns(squadRepositoryMock.Object);

            _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(member.Id, application.Id, member.SquadId, knowledge.Status)).ReturnsAsync(false);

            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            var result = await service.UpdateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
        [Fact]
        public void KnowledgeResourceApplicationIsRequiredShouldReturnResource()
        {
            // Act
            var value = KnowledgeResource.ApplicationIsRequired;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(value));
        }

        [Fact]
        public void KnowledgeResourceAssociationAlreadyExistsShouldReturnResource()
        {
            // Act
            var value = KnowledgeResource.AssociationAlreadyExists;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(value));
        }

        [Fact]
        public void KnowledgeResourceAssociationFoundShouldReturnResource()
        {
            // Act
            var value = KnowledgeResource.AssociationFound;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(value));
        }

        [Fact]
        public void KnowledgeResourceAssociationNotFoundShouldReturnResource()
        {
            // Act
            var value = KnowledgeResource.AssociationNotFound;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(value));
        }

        [Fact]
        public void KnowledgeResourceCannotEditOrRemovePastAssociationShouldReturnResource()
        {
            // Act
            var value = KnowledgeResource.CannotEditOrRemovePastAssociation;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(value));
        }

        [Fact]
        public void KnowledgeResourceMemberApplicationMustBelongToTheSameSquadShouldReturnResource()
        {
            // Act
            var value = KnowledgeResource.MemberApplicationMustBelongToTheSameSquad;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(value));
        }

        [Fact]
        public void KnowledgeResourceMemberApplicationNotFoundShouldReturnResource()
        {
            // Act
            var value = KnowledgeResource.MemberApplicationNotFound;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(value));
        }

        [Fact]
        public void KnowledgeResourceMemberIsRequiredShouldReturnResource()
        {
            // Act
            var value = KnowledgeResource.MemberIsRequired;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(value));
        }

        [Fact]
        public void KnowledgeResourceOnlyPossibleRemoveIfBelongToTheLeadersSquadShouldReturnResource()
        {
            // Act
            var value = KnowledgeResource.OnlyPossibleRemoveIfBelongToTheLeadersSquad;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(value));
        }

        [Fact]
        public void KnowledgeResourceUnsupportedMembershipUpdateShouldReturnResource()
        {
            // Act
            var value = KnowledgeResource.UnsupportedMembershipUpdate;

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(value));
        }
        [Fact]
        public async Task KnowledgeValidatorWhenMemberIdIsZeroShouldReturnMemberIsRequiredError()
        {
            // Arrange
            var localizerFactory = LocalizerFactorHelper.Create();
            var validator = new KnowledgeValidator(localizerFactory);
            var knowledge = new Knowledge { MemberId = 0 };
            knowledge.ApplicationIds.Add(1);

            // Act
            var result = await validator.ValidateAsync(knowledge);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Knowledge.MemberId) && e.ErrorMessage == KnowledgeResource.MemberIsRequired);
        }

        [Fact]
        public async Task KnowledgeValidatorWhenApplicationIdsIsEmptyShouldReturnApplicationIsRequiredError()
        {
            // Arrange
            var localizerFactory = LocalizerFactorHelper.Create();
            var validator = new KnowledgeValidator(localizerFactory);
            var knowledge = new Knowledge { MemberId = 1 };

            // Act
            var result = await validator.ValidateAsync(knowledge);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Knowledge.ApplicationIds) && e.ErrorMessage == KnowledgeResource.ApplicationIsRequired);
        }

        [Fact]
        public async Task KnowledgeValidatorWhenMemberIdAndApplicationIdsAreValidShouldBeValid()
        {
            // Arrange
            var localizerFactory = LocalizerFactorHelper.Create();
            var validator = new KnowledgeValidator(localizerFactory);
            var knowledge = new Knowledge
            {
                MemberId = 1,
                SquadId = 1
            };
            knowledge.ApplicationIds.Add(1);

            // Act
            var result = await validator.ValidateAsync(knowledge);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task GetSquadByMemberAsyncReturnsSquadWhenMemberExists()
        {
            var member = new Member
            {
                Id = 1,
                SquadId = 2,
                Name = "Test Name",
                Role = "Test Role",
                Cost = 1m,
                Email = "test@email.com"
            }; 
            var squad = new Squad { Id = 2, Name = "Squad Test" };

            var memberRepositoryMock = new Mock<IMemberRepository>();
            var squadRepositoryMock = new Mock<ISquadRepository>();
            memberRepositoryMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);
            squadRepositoryMock.Setup(r => r.GetByIdAsync(member.SquadId)).ReturnsAsync(squad);

            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.SquadRepository).Returns(squadRepositoryMock.Object);

            var result = await _knowledgeService.GetSquadByMemberAsync(member.Id);

            Assert.NotNull(result);
            Assert.Equal(squad.Id, result.Id);
        }

        [Fact]
        public async Task GetSquadByMemberAsyncReturnsNullWhenMemberDoesNotExist()
        {
            var memberRepositoryMock = new Mock<IMemberRepository>();
            memberRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Member?)null);
            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);

            var result = await _knowledgeService.GetSquadByMemberAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetApplicationsByMemberAsyncReturnsApplications()
        {
            var applications = new List<ApplicationData>
            {
                new("App1") { Id = 1 },
                new("App2") { Id = 2 }
            };
            _knowledgeRepositoryMock.Setup(r => r.ListApplicationsByMemberAsync(1, KnowledgeStatus.Atual)).ReturnsAsync(applications);

            var result = await _knowledgeService.GetApplicationsByMemberAsync(1);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, a => a.Id == 1);
            Assert.Contains(result, a => a.Id == 2);
        }

        [Fact]
        public async Task GetSquadsByApplicationAsyncReturnsSquadWhenApplicationHasSquad()
        {
            var application = new ApplicationData("App") { Id = 1, SquadId = 2 };
            var squad = new Squad { Id = 2, Name = "Squad Test" };

            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            var squadRepositoryMock = new Mock<ISquadRepository>();
            applicationRepositoryMock.Setup(r => r.GetByIdAsync(application.Id)).ReturnsAsync(application);
            squadRepositoryMock.Setup(r => r.GetByIdAsync(application.SquadId.Value)).ReturnsAsync(squad);

            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.SquadRepository).Returns(squadRepositoryMock.Object);

            var result = await _knowledgeService.GetSquadsByApplicationAsync(application.Id);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(squad.Id, result[0].Id);
        }

        [Fact]
        public async Task GetSquadsByApplicationAsyncReturnsEmptyWhenApplicationHasNoSquad()
        {
            var application = new ApplicationData("App") { Id = 1, SquadId = null };
            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            applicationRepositoryMock.Setup(r => r.GetByIdAsync(application.Id)).ReturnsAsync(application);

            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);

            var result = await _knowledgeService.GetSquadsByApplicationAsync(application.Id);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetApplicationsBySquadAsyncReturnsApplications()
        {
            var applications = new List<ApplicationData>
            {
                new("App1") { Id = 1, SquadId = 10 },
                new("App2") { Id = 2, SquadId = 10 }
            };
            var pagedResult = new PagedResult<ApplicationData> { Result = applications, Page = 1, PageSize = 10, Total = 2 };

            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            applicationRepositoryMock.Setup(r => r.GetListAsync(It.IsAny<ApplicationFilter>())).ReturnsAsync(pagedResult);

            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);

            var result = await _knowledgeService.GetApplicationsBySquadAsync(10);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, a => Assert.Equal(10, a.SquadId));
        }
    }
}