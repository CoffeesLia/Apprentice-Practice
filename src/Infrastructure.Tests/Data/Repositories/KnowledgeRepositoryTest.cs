using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;


namespace Infrastructure.Tests.Data.Repositories
{
    public class KnowledgeRepositoryTest : IDisposable
    {
        private bool _disposed;
        private readonly Context _context;
        private readonly KnowledgeRepository _repository;
        private readonly Fixture _fixture = new();

        public KnowledgeRepositoryTest()
        {
            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new KnowledgeRepository(_context);

            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task CreateAssociationAsyncShouldAddAssociation()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
            var app = _fixture.Build<ApplicationData>().With(a => a.Id, 2).Create();
            _context.Members.Add(member);
            _context.Applications.Add(app);
            await _context.SaveChangesAsync();

            // Act
            await _repository.CreateAssociationAsync(1, 2);

            // Assert
            var exists = await _repository.AssociationExistsAsync(1, 2);
            Assert.True(exists);
        }

        [Fact]
        public async Task RemoveAssociationAsyncShouldRemoveAssociation()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
            var app = _fixture.Build<ApplicationData>().With(a => a.Id, 2).Create();
            _context.Members.Add(member);
            _context.Applications.Add(app);
            _context.Knowledges.Add(new Knowledge { MemberId = 1, ApplicationId = 2 });
            await _context.SaveChangesAsync();

            // Act
            await _repository.RemoveAssociationAsync(1, 2);

            // Assert
            var exists = await _repository.AssociationExistsAsync(1, 2);
            Assert.False(exists);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnKnowledge()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
            var app = _fixture.Build<ApplicationData>().With(a => a.Id, 2).Create();
            var knowledge = new Knowledge { MemberId = 1, ApplicationId = 2 };
            _context.Members.Add(member);
            _context.Applications.Add(app);
            _context.Knowledges.Add(knowledge);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(knowledge.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.MemberId);
            Assert.Equal(2, result.ApplicationId);
        }

        [Fact]
        public async Task GetListAsyncShouldApplyFiltersCorrectly()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
            var app = _fixture.Build<ApplicationData>().With(a => a.Id, 2).Create();
            var knowledge = new Knowledge { MemberId = 1, ApplicationId = 2 };
            _context.Members.Add(member);
            _context.Applications.Add(app);
            _context.Knowledges.Add(knowledge);
            await _context.SaveChangesAsync();

            var filter = new KnowledgeFilter { MemberId = 1, ApplicationId = 2 };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Result);
            Assert.Equal(1, result.Result.First().MemberId);
            Assert.Equal(2, result.Result.First().ApplicationId);
        }

        [Fact]
        public async Task ListApplicationsByMemberAsyncShouldReturnApplications()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
            var app = _fixture.Build<ApplicationData>().With(a => a.Id, 2).Create();
            _context.Members.Add(member);
            _context.Applications.Add(app);
            _context.Knowledges.Add(new Knowledge { MemberId = 1, ApplicationId = 2 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ListApplicationsByMemberAsync(1);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result.First().Id);
        }

        [Fact]
        public async Task ListMembersByApplicationAsyncShouldReturnMembers()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
            var app = _fixture.Build<ApplicationData>().With(a => a.Id, 2).Create();
            _context.Members.Add(member);
            _context.Applications.Add(app);
            _context.Knowledges.Add(new Knowledge { MemberId = 1, ApplicationId = 2 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ListMembersByApplicationAsync(2);

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing && _context != null)
            {
                _context.Database.EnsureDeleted();
                _context.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~KnowledgeRepositoryTest()
        {
            Dispose(false);
        }
    }
}
