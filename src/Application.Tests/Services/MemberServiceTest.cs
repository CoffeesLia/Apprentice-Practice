using System.Globalization;
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
using Xunit;

namespace Application.Tests.Services
{
    public class MemberServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IMemberRepository> _memberRepositoryMock;
        private readonly MemberService _memberService;
        private readonly Fixture _fixture;

        public MemberServiceTests()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _memberRepositoryMock = new Mock<IMemberRepository>();

            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = LocalizerFactorHelper.Create();
            MemberValidator memberValidator = new(localizer);

            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(_memberRepositoryMock.Object);

            _memberService = new MemberService(_unitOfWorkMock.Object, localizer, memberValidator);

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
            Member member = _fixture.Build<Member>()
                                 .With(m => m.Name, string.Empty)
                                 .With(m => m.Email, string.Empty)
                                 .With(m => m.Role, string.Empty)
                                 .Create();

            // Act
            OperationResult result = await _memberService.CreateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(MemberResource.MemberNameIsRequired, result.Errors);
            Assert.Contains(MemberResource.MemberEmailIsRequired, result.Errors);
            Assert.Contains(MemberResource.MemberRoleIsRequired, result.Errors);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenMemberCostIsRequired()
        {
            // Arrange
            Member member = new()
            {
                Name = "Name",
                Role = "test",
                Email = "test@gamail.com",
                Cost = default
            };

            // Act
            OperationResult result = await _memberService.CreateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(MemberResource.MemberCostRequired, result.Errors.First());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenCostIsLessThanZero()
        {
            // Arrange
            Member member = new()
            {
                Name = "Name",
                Role = "test",
                Email = "test@gamail.com",
                Cost = -1
            };

            // Act
            OperationResult result = await _memberService.CreateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Equal(MemberResource.CostMemberLargestEqualZero, result.Errors.First());
        }

        [Fact]
        public async Task CreateAsyncWhenEmailAlreadyExists()
        {
            // Arrange
            Member member = _fixture.Build<Member>()
                                 .With(m => m.Name, "Valid Name")
                                 .With(m => m.Email, "valid.email@example.com")
                                 .With(m => m.SquadId, 1)
                                 .Create();
            ValidationResult validationResult = new();
            var validatorMock = new Mock<IValidator<Member>>();
            validatorMock.Setup(v => v.ValidateAsync(member, default)).ReturnsAsync(validationResult);
            _memberRepositoryMock.Setup(r => r.IsEmailUnique(member.Email, null)).ReturnsAsync(false);

            var squadMock = new Mock<ISquadRepository>();
            _unitOfWorkMock.Setup(u => u.SquadRepository).Returns(squadMock.Object);
            squadMock.Setup(s => s.GetByIdAsync(member.SquadId)).ReturnsAsync(new Squad { Id = member.SquadId, Name = "Squad Test" });

            var memberService = new MemberService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await memberService.CreateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Equal(MemberResource.MemberEmailAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateAsyncWhenSuccessful()
        {
            // Arrange
            Member member = _fixture.Build<Member>()
                                 .With(m => m.Name, "Valid Name")
                                 .With(m => m.Email, "valid.email@example.com")
                                 .With(m => m.SquadId, 1)
                                 .Create();

            var validatorMock = new Mock<IValidator<Member>>();
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.ValidateAsync(member, default)).ReturnsAsync(validationResult);
            _memberRepositoryMock.Setup(r => r.IsEmailUnique(member.Email, null)).ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            var squadMock = new Mock<ISquadRepository>();
            _unitOfWorkMock.Setup(u => u.SquadRepository).Returns(squadMock.Object);
            squadMock.Setup(s => s.GetByIdAsync(member.SquadId)).ReturnsAsync(new Squad { Id = member.SquadId, Name = "Squad Test" });

            var memberService = new MemberService(_unitOfWorkMock.Object, LocalizerFactorHelper.Create(), validatorMock.Object);

            // Act
            OperationResult result = await memberService.CreateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            MemberFilter filter = _fixture.Create<MemberFilter>();
            PagedResult<Member> pagedResult = _fixture.Create<PagedResult<Member>>();
            _memberRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            PagedResult<Member> result = await _memberService.GetListAsync(filter);

            // Assert
            Assert.Equal(pagedResult, result);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Member?)null);

            // Act
            OperationResult result = await _memberService.GetItemAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemExists()
        {
            // Arrange
            Member member = _fixture.Create<Member>();
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);

            // Act
            OperationResult result = await _memberService.GetItemAsync(member.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenValidationFails()
        {
            // Arrange
            Member member = _fixture.Build<Member>()
                                 .With(m => m.Name, string.Empty)
                                 .With(m => m.Email, string.Empty)
                                 .Create();
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);

            // Act
            OperationResult result = await _memberService.UpdateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnNotFoundWhenMemberDoesNotExist()
        {
            // Arrange
            int memberId = 1;
            Member member = new()
            {
                Id = memberId,
                Name = "Test Member",
                Role = "Test Role",
                Email = "test@example.com",
                Cost = 100
            };

            _memberRepositoryMock.Setup(r => r.GetByIdAsync(memberId)).ReturnsAsync((Member?)null);

            // Act
            OperationResult result = await _memberService.UpdateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
            Assert.Equal(MemberResource.MemberNotFound, result.Message);
        }

        [Fact]
        public async Task UpdateAsyncWhenEmailAlreadyExists()
        {
            // Arrange
            Member member = _fixture.Build<Member>()
                                 .With(m => m.Name, "Valid Name")
                                 .With(m => m.Email, "valid.email@example.com")
                                 .Create();
            Mock<IValidator<Member>> validatorMock = new();
            ValidationResult validationResult = new(); // Certifique-se de que o resultado da validação seja válido
            validatorMock.Setup(v => v.ValidateAsync(member, default)).ReturnsAsync(validationResult);
            _memberRepositoryMock.Setup(r => r.IsEmailUnique(member.Email, null)).ReturnsAsync(false);
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);

            // Act
            OperationResult result = await _memberService.UpdateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenSuccessful()
        {
            // Arrange
            Member member = _fixture.Build<Member>()
                                 .With(m => m.Name, "Valid Name")
                                 .With(m => m.Email, "valid.email@example.com")
                                 .Create();
            Mock<IValidator<Member>> validatorMock = new();
            ValidationResult validationResult = new();
            validatorMock.Setup(v => v.ValidateAsync(member, default)).ReturnsAsync(validationResult);
            _memberRepositoryMock.Setup(r => r.IsEmailUnique(member.Email, member.Id)).ReturnsAsync(true);
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);

            // Act
            OperationResult result = await _memberService.UpdateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenItemDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Member?)null);

            // Act
            OperationResult result = await _memberService.DeleteAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenSuccessful()
        {
            // Arrange
            Member member = _fixture.Create<Member>();
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);

            // Act
            OperationResult result = await _memberService.DeleteAsync(member.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}