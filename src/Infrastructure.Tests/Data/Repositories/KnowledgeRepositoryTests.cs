using System;
using System.Linq;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class KnowledgeRepositoryTests : IDisposable
    {
        private bool _disposed;

        private readonly Context _context;
        private readonly KnowledgeRepository _repository;
        private readonly Fixture _fixture = new();

        public KnowledgeRepositoryTests()
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
            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
            var application = _fixture.Build<ApplicationData>().With(a => a.Id, 2).Create();
            var squad = _fixture.Build<Squad>().With(s => s.Id, 3).Create();

            _context.Members.Add(member);
            _context.Applications.Add(application);
            _context.Squads.Add(squad);
            await _context.SaveChangesAsync();

            var knowledge = new Knowledge
            {
                MemberId = member.Id,
                ApplicationId = application.Id,
                SquadId = squad.Id,
                Status = KnowledgeStatus.Atual,
                Member = member,
                Application = application,
                Squad = squad
            };

            // Act
            await _repository.CreateAssociationAsync(knowledge);

            // Assert
            var exists = await _repository.AssociationExistsAsync(member.Id, application.Id, squad.Id, KnowledgeStatus.Atual);
            Assert.True(exists);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnKnowledge()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 10).Create();
            var application = _fixture.Build<ApplicationData>().With(a => a.Id, 20).Create();
            var squad = _fixture.Build<Squad>().With(s => s.Id, 30).Create();

            _context.Members.Add(member);
            _context.Applications.Add(application);
            _context.Squads.Add(squad);
            await _context.SaveChangesAsync();

            var knowledge = new Knowledge
            {
                MemberId = member.Id,
                ApplicationId = application.Id,
                SquadId = squad.Id,
                Status = KnowledgeStatus.Atual,
                Member = member,
                Application = application,
                Squad = squad
            };

            await _repository.CreateAssociationAsync(knowledge);

            // Act
            Knowledge? retrievedKnowledge = await _repository.GetByIdAsync(knowledge.Id);

            // Assert
            Assert.NotNull(retrievedKnowledge);
            Assert.Equal(knowledge.Id, retrievedKnowledge.Id);
        }

        [Fact]
        public async Task RemoveAsyncShouldRemoveKnowledge()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 100).Create();
            var application = _fixture.Build<ApplicationData>().With(a => a.Id, 200).Create();
            var squad = _fixture.Build<Squad>().With(s => s.Id, 300).Create();

            _context.Members.Add(member);
            _context.Applications.Add(application);
            _context.Squads.Add(squad);
            await _context.SaveChangesAsync();

            var knowledge = new Knowledge
            {
                MemberId = member.Id,
                ApplicationId = application.Id,
                SquadId = squad.Id,
                Status = KnowledgeStatus.Atual,
                Member = member,
                Application = application,
                Squad = squad
            };

            await _repository.CreateAssociationAsync(knowledge);

            // Act
            await _repository.RemoveAsync(member.Id, application.Id, squad.Id, KnowledgeStatus.Atual);

            // Assert
            var exists = await _repository.AssociationExistsAsync(member.Id, application.Id, squad.Id, KnowledgeStatus.Atual);
            Assert.False(exists);
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

        ~KnowledgeRepositoryTests()
        {
            Dispose(false);
        }
    }
}