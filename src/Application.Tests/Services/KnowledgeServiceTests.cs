using Application.Tests.Helpers;
using AutoFixture;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using System;
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

            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = LocalizerFactorHelper.Create();
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
        public async Task CreateAsyncWhenValidationFails()
        {
            // Arrange
            Knowledge knowledge = _fixture.Build<Knowledge>()
                .With(k => k.MemberId, 0)
                .With(k => k.ApplicationId, 0)
                .Create();

            var validationResult = new ValidationResult(new[] { new ValidationFailure("MemberId", "MemberId is required") });
            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(validationResult);

            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenAssociationAlreadyExists()
        {
            // Arrange
            Knowledge knowledge = _fixture.Create<Knowledge>();
            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(new ValidationResult());
            _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(knowledge.MemberId, knowledge.ApplicationId, knowledge.SquadId)).ReturnsAsync(true);

            // Mock dos repositórios de membro e aplicação
            var memberRepositoryMock = new Mock<IMemberRepository>();
            var applicationRepositoryMock = new Mock<IApplicationDataRepository>();
            memberRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.MemberId)).ReturnsAsync(new Member { Id = knowledge.MemberId, SquadId = knowledge.SquadId, Name = "Test", Role = "Test", Email = "test@test.com", Cost = 1 });
            applicationRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.ApplicationId)).ReturnsAsync(new ApplicationData("App") { Id = knowledge.ApplicationId, SquadId = knowledge.SquadId });

            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(applicationRepositoryMock.Object);

            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(KnowledgeResource.AssociationAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateAsyncWhenSuccessful()
        {
            // Arrange
            var member = _fixture.Build<Member>()
                .With(m => m.Name, "Valid Name")
                .With(m => m.Email, "valid.email@example.com")
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
                .Create();

            var validatorMock = new Mock<IValidator<Knowledge>>();
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(validationResult);

            _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(knowledge.MemberId, knowledge.ApplicationId, knowledge.SquadId)).ReturnsAsync(false);

            // Mock dos repositórios de membro, aplicação e squad
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

            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Knowledge?)null);

            // Act
            OperationResult result = await _knowledgeService.GetItemAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemExists()
        {
            // Arrange
            Knowledge knowledge = _fixture.Create<Knowledge>();
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync(knowledge);

            // Act
            OperationResult result = await _knowledgeService.GetItemAsync(knowledge.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenKnowledgeDoesNotExist()
        {
            // Arrange
            Knowledge knowledge = _fixture.Create<Knowledge>();
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(knowledge.Id)).ReturnsAsync((Knowledge?)null);

            // Mock do validator para evitar NullReferenceException
            var validatorMock = new Mock<IValidator<Knowledge>>();
            validatorMock.Setup(v => v.ValidateAsync(knowledge, default)).ReturnsAsync(new ValidationResult());

            // Recrie o serviço com o validator mockado corretamente
            var service = new KnowledgeService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await service.UpdateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(KnowledgeResource.AssociationNotFound, result.Message);
        }

        [Fact]
        public async Task DeleteAsyncWhenItemDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Knowledge?)null);

            // Act
            OperationResult result = await _knowledgeService.DeleteAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }
        [Fact]
        public async Task DeleteAsyncWhenSuccessful()
        {
            // Arrange
            var member = _fixture.Build<Member>()
                .With(m => m.Id, 1)
                .With(m => m.SquadId, 10)
                .With(m => m.Role, "SquadLeader")
               // .With(m => m.SquadLeader, true)
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
            OperationResult result = await _knowledgeService.DeleteAsync(knowledge.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}
