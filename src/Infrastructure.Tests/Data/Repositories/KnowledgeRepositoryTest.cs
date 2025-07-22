using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;

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
        public async Task CreateAssociationAsyncShouldAddKnowledge()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).With(m => m.SquadId, 1).Create();
            var app = _fixture.Build<ApplicationData>().With(a => a.Id, 2).With(a => a.SquadId, 1).With(a => a.ProductOwner, "PO").With(a => a.ConfigurationItem, "CI").Create();
            _context.Members.Add(member);
            _context.Applications.Add(app);
            await _context.SaveChangesAsync();

            // Act
            await _repository.CreateAssociationAsync(1, 2, 1);

            // Assert
            var knowledge = _context.Knowledges.FirstOrDefault();
            Assert.NotNull(knowledge);
            Assert.Equal(1, knowledge.MemberId);
            Assert.Equal(2, knowledge.ApplicationId);
            Assert.Equal(1, knowledge.SquadIdAtAssociationTime);
        }

        [Fact]
        public async Task AssociationExistsAsyncShouldReturnTrueIfExists()
        {
            // Arrange
            var knowledge = _fixture.Build<Knowledge>().With(k => k.MemberId, 1).With(k => k.ApplicationId, 2).With(k => k.SquadIdAtAssociationTime, 1).Create();
            _context.Knowledges.Add(knowledge);
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repository.AssociationExistsAsync(1, 2);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveOnlySpecificAssociation()
        {
            // Arrange
            var k1 = _fixture.Build<Knowledge>().With(k => k.MemberId, 1).With(k => k.ApplicationId, 2).With(k => k.SquadIdAtAssociationTime, 1).Create();
            var k2 = _fixture.Build<Knowledge>().With(k => k.MemberId, 1).With(k => k.ApplicationId, 3).With(k => k.SquadIdAtAssociationTime, 1).Create();
            _context.Knowledges.AddRange(k1, k2);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(1, 2);

            // Assert
            Assert.Single(_context.Knowledges);
            Assert.Equal(3, _context.Knowledges.First().ApplicationId);
        }

        [Fact]
        public async Task GetListAsyncShouldApplyFiltersCorrectly()
        {
            // Arrange
            var knowledges = _fixture.CreateMany<Knowledge>(10).ToList();
            knowledges[0].MemberId = 99;
            knowledges[0].ApplicationId = 88;
            knowledges[0].SquadIdAtAssociationTime = 77;
            _context.Knowledges.AddRange(knowledges);
            await _context.SaveChangesAsync();

            var filter = new KnowledgeFilter
            {
                MemberId = 99,
                ApplicationId = 88,
                SquadId = 77
            };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Result);
            Assert.Equal(99, result.Result.First().MemberId);
            Assert.Equal(88, result.Result.First().ApplicationId);
            Assert.Equal(77, result.Result.First().SquadIdAtAssociationTime);
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
