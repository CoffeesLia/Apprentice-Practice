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
                .With(k => k.ApplicationId, 0)
                .Create();

            var validationResult = new ValidationResult(new[] { new ValidationFailure("MemberId", "MemberId is required") });
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
            applicationRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.ApplicationId)).ReturnsAsync(new ApplicationData("App") { Id = knowledge.ApplicationId, SquadId = knowledge.SquadId });
            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);
            _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(knowledge.MemberId, knowledge.ApplicationId, knowledge.SquadId, knowledge.Status)).ReturnsAsync(true);

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
                .With(m => m.Role, "SquadLeader")
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
                .With(k => k.ApplicationId, application.Id)
                .With(k => k.SquadId, squad.Id)
                .With(k => k.Member, member)
                .With(k => k.Application, application)
                .With(k => k.Squad, squad)
                .With(k => k.Status, KnowledgeStatus.Atual)
                .Create();

            var validatorMock = new Mock<IValidator<Knowledge>>();
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(validationResult);

            var memberRepositoryMock = new Mock<IMemberRepository>();
            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            var squadRepositoryMock = new Mock<ISquadRepository>();

            memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.MemberId)).ReturnsAsync(member);
            applicationRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.ApplicationId)).ReturnsAsync(application);
            squadRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.SquadId)).ReturnsAsync(squad);

            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.SquadRepository).Returns(squadRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);
            _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(knowledge.MemberId, knowledge.ApplicationId, knowledge.SquadId, knowledge.Status)).ReturnsAsync(false);

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
                .With(m => m.Role, "SquadLeader")
                .Create();

            var application = _fixture.Build<ApplicationData>()
                .With(a => a.Id, 2)
                .With(a => a.SquadId, 10)
                .Create();

            var knowledge = _fixture.Build<Knowledge>()
                .With(k => k.Id, 1)
                .With(k => k.MemberId, member.Id)
                .With(k => k.ApplicationId, application.Id)
                .With(k => k.SquadId, member.SquadId)
                .With(k => k.Status, KnowledgeStatus.Atual)
                .Create();

            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);

            var memberRepositoryMock = new Mock<IMemberRepository>();
            memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.MemberId)).ReturnsAsync(member);
            memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(member); // performingUser

            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            applicationRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.ApplicationId)).ReturnsAsync(application);

            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);

            // Act
            var result = await _knowledgeService.DeleteAsync(knowledge.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
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
                new ApplicationData("App1") { Id = 1 },
                new ApplicationData("App2") { Id = 2 }
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
                new ApplicationData("App1") { Id = 1, SquadId = 10 },
                new ApplicationData("App2") { Id = 2, SquadId = 10 }
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