using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
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

            Microsoft.Extensions.Localization.IStringLocalizerFactory localizer = Application.Tests.Helpers.LocalizerFactorHelper.Create();
            KnowledgeValidator validator = new(localizer);


            _unitOfWorkMock.Setup(u => u.KnowledgeRepository).Returns(_knowledgeRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.MemberRepository).Returns(_memberRepositoryMock.Object);
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(_applicationRepositoryMock.Object);

            _knowledgeService = new KnowledgeService(_unitOfWorkMock.Object, localizer, validator);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnInvalidDataWhenValidationFails()
        {
            // Arrange
            Knowledge knowledge = new() { MemberId = 0, ApplicationId = 0 };

            // Act
            OperationResult result = await _knowledgeService.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenAssociationExists()
        {
            // Arrange
            Knowledge knowledge = new() { MemberId = 1, ApplicationId = 2 };
            _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(1, 2)).ReturnsAsync(true);

            // Act
            OperationResult result = await _knowledgeService.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnNotFoundWhenMemberOrApplicationNotFound()
        {
            // Arrange
            Knowledge knowledge = new() { MemberId = 1, ApplicationId = 2 };
            _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(1, 2)).ReturnsAsync(false);
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Member?)null);
            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((ApplicationData?)null);

            // Act
            OperationResult result = await _knowledgeService.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenSquadIsDifferent()
        {
            // Arrange
            Knowledge knowledge = new() { MemberId = 1, ApplicationId = 2 };
            _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(1, 2)).ReturnsAsync(false);
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Member { Id = 1, SquadId = 1, Name = "A", Role = "R", Email = "a@a.com", Cost = 1 });
            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new ApplicationData("App") {Id = 2, SquadId = 2, ProductOwner = "", ConfigurationItem = "" });

            // Act
            OperationResult result = await _knowledgeService.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenValid()
        {
            // Arrange
            Knowledge knowledge = new() { MemberId = 1, ApplicationId = 2 };
            _knowledgeRepositoryMock.Setup(r => r.AssociationExistsAsync(1, 2)).ReturnsAsync(false);
            _memberRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Member { Id = 1, SquadId = 1, Name = "A", Role = "R", Email = "a@a.com", Cost = 1 });
            _applicationRepositoryMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new ApplicationData("App") {Id = 2, SquadId = 1, ProductOwner = "", ConfigurationItem = "" });

            // Act
            OperationResult result = await _knowledgeService.CreateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnNotFoundWhenItemDoesNotExist()
        {
            // Arrange
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Knowledge?)null);

            // Act
            OperationResult result = await _knowledgeService.DeleteAsync(1);

            // Assert
            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnSuccessWhenItemIsDeleted()
        {
            // Arrange
            var knowledge = new Knowledge { Id = 1, MemberId = 1, ApplicationId = 2 };
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(knowledge);

            // Act
            OperationResult result = await _knowledgeService.DeleteAsync(1);

            // Assert
            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnInvalidData()
        {
            // Arrange
            var knowledge = new Knowledge { Id = 1, MemberId = 1, ApplicationId = 2 };

            // Act
            OperationResult result = await _knowledgeService.UpdateAsync(knowledge);

            // Assert
            Assert.Equal(OperationStatus.InvalidData, result.Status);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnNotFoundWhenItemIsNotFound()
        {
            // Arrange
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Knowledge?)null);

            // Act
            var result = await _knowledgeService.GetItemAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetItemAsyncShouldReturnKnowledgeWhenItemIsFound()
        {
            // Arrange
            var knowledge = new Knowledge { Id = 1, MemberId = 1, ApplicationId = 2 };
            _knowledgeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(knowledge);

            // Act
            var result = await _knowledgeService.GetItemAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task ListApplicationsByMemberAsyncShouldReturnList()
        {
            // Arrange
            var applications = new List<ApplicationData>
            {
                new("App1") { Id = 1, SquadId = 1, ProductOwner = "PO", ConfigurationItem = "CI" },
                new ("App2") { Id = 2, SquadId = 1, ProductOwner = "PO", ConfigurationItem = "CI" }
            };
            _knowledgeRepositoryMock.Setup(r => r.ListApplicationsByMemberAsync(1)).ReturnsAsync(applications);

            // Act
            var result = await _knowledgeService.ListApplicationsByMemberAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
        }

        [Fact]
        public async Task ListMembersByApplicationAsyncShouldReturnList()
        {
            // Arrange
            var list = new List<Member> { new Member { Id = 1, Name = "A", Role = "R", Email = "a@a.com", Cost = 1 } };
            _knowledgeRepositoryMock.Setup(r => r.ListMembersByApplicationAsync(2)).ReturnsAsync(list);

            // Act
            var result = await _knowledgeService.ListMembersByApplicationAsync(2);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }
    }
}
