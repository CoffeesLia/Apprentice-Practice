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
using Application.Tests.Helpers;

namespace Application.Services.Tests
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
            var localizer = LocalizerFactorHelper.Create();
            var memberValidator = new MemberValidator(localizer);

            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(_memberRepositoryMock.Object);

            _memberService = new MemberService(_unitOfWorkMock.Object, localizer, memberValidator);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task CreateAsyncWhenValidationFails()
        {
            // Arrange
            var member = _fixture.Build<Member>()
                                 .With(m => m.Name, string.Empty)
                                 .With(m => m.Email, string.Empty)
                                 .Create();

            // Act
            var result = await _memberService.CreateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(nameof(MemberResource.MemberNameIsRequired), result.Errors);
            Assert.Contains(nameof(MemberResource.MemberEmailIsRequired), result.Errors);

        }

        [Fact]
        public async Task CreateAsyncWhenEmailAlreadyExists()
        {
            // Arrange
            var member = _fixture.Build<Member>()
                                 .With(m => m.Name, "Valid Name")
                                 .With(m => m.Email, "valid.email@example.com")
                                 .Create();
            var validatorMock = new Mock<IValidator<Member>>();
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.ValidateAsync(member, default)).ReturnsAsync(validationResult);
            _memberRepositoryMock.Setup(r => r.IsEmailUnique(member.Email)).ReturnsAsync(false);

            // Act
            var result = await _memberService.CreateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains(nameof(MemberResource.MemberEmailAlreadyExists), result.Errors);
        }

        [Fact]
        public async Task CreateAsyncWhenSuccessful()
        {
            // Arrange
            var member = _fixture.Build<Member>()
                                 .With(m => m.Name, "Valid Name")
                                 .With(m => m.Email, "valid.email@example.com")
                                 .Create();
            var validatorMock = new Mock<IValidator<Member>>();
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.ValidateAsync(member, default)).ReturnsAsync(validationResult);
            _memberRepositoryMock.Setup(r => r.IsEmailUnique(member.Email)).ReturnsAsync(true);

            // Act
            var result = await _memberService.CreateAsync(member);

            // Inspecione o resultado da validação
            foreach (var error in validationResult.Errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResult()
        {
            // Arrange
            var filter = _fixture.Create<MemberFilter>();
            var pagedResult = _fixture.Create<PagedResult<Member>>();
            _memberRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(pagedResult);

            // Act
            var result = await _memberService.GetListAsync(filter);

            // Assert
            Assert.Equal(pagedResult, result);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Member?)null);

            // Act
            var result = await _memberService.GetItemAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncWhenItemExists()
        {
            // Arrange
            var member = _fixture.Create<Member>();
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);

            // Act
            var result = await _memberService.GetItemAsync(member.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncWhenValidationFails()
        {
            // Arrange
            var member = _fixture.Build<Member>()
                                 .With(m => m.Name, string.Empty)
                                 .With(m => m.Email, string.Empty)
                                 .Create();
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);

            // Act
            var result = await _memberService.UpdateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
            Assert.Contains(nameof(MemberResource.MemberNameIsRequired), result.Errors);
            Assert.Contains(nameof(MemberResource.MemberEmailIsRequired), result.Errors);
        }

        [Fact]
        public async Task UpdateAsyncWhenEmailAlreadyExists()
        {
            // Arrange
            var member = _fixture.Build<Member>()
                                 .With(m => m.Name, "Valid Name")
                                 .With(m => m.Email, "valid.email@example.com")
                                 .Create();
            var validatorMock = new Mock<IValidator<Member>>();
            var validationResult = new ValidationResult(); // Certifique-se de que o resultado da validação seja válido
            validatorMock.Setup(v => v.ValidateAsync(member, default)).ReturnsAsync(validationResult);
            _memberRepositoryMock.Setup(r => r.IsEmailUnique(member.Email)).ReturnsAsync(false);
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);

            // Act
            var result = await _memberService.UpdateAsync(member);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
            Assert.Contains(nameof(MemberResource.MemberEmailAlreadyExists), result.Errors);
        }

        [Fact]
        public async Task UpdateAsyncWhenSuccessful()
        {
            // Arrange
            var member = _fixture.Build<Member>()
                                 .With(m => m.Name, "Valid Name")
                                 .With(m => m.Email, "valid.email@example.com")
                                 .Create();
            var validatorMock = new Mock<IValidator<Member>>();
            var validationResult = new ValidationResult();
            validatorMock.Setup(v => v.ValidateAsync(member, default)).ReturnsAsync(validationResult);
            _memberRepositoryMock.Setup(r => r.IsEmailUnique(member.Email)).ReturnsAsync(true);
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);

            // Act
            var result = await _memberService.UpdateAsync(member);

            // Inspecione o resultado da validação
            foreach (var error in validationResult.Errors)
            {
                Console.WriteLine(error.ErrorMessage);
            }

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenItemDoesNotExist()
        {
            // Arrange
            var id = _fixture.Create<int>();
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Member?)null);

            // Act
            var result = await _memberService.DeleteAsync(id);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncWhenSuccessful()
        {
            // Arrange
            var member = _fixture.Create<Member>();
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);

            // Act
            var result = await _memberService.DeleteAsync(member.Id);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }
    }
}