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
            var member = new Member { Id = 1, Name = "Teste", Role = "Dev", Cost = 100, Email = "teste@teste.com", SquadId = 1 };
            var app = new ApplicationData("App") { Id = 2, SquadId = 1, ProductOwner = "PO" };
            _context.Members.Add(member);
            _context.Applications.Add(app);
            await _context.SaveChangesAsync();

            // Act
            await _repository.CreateAssociationAsync(1, 2, 1);

            // Assert
            var knowledge = _context.Knowledges
                .Include(k => k.Application)
                .FirstOrDefault();
            Assert.NotNull(knowledge);
            Assert.Equal(1, knowledge.MemberId);
            Assert.Equal(2, knowledge.ApplicationId);
            Assert.Equal(1, knowledge.AssociatedSquadId);
            Assert.Equal(app.Id, knowledge.Application.Id);
        }

        [Fact]
        public async Task AssociationExistsAsyncShouldReturnTrueIfExists()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 1).Create();
            var app = _fixture.Build<ApplicationData>().With(a => a.Id, 2).Create();
            _context.Members.Add(member);
            _context.Applications.Add(app);
            await _context.SaveChangesAsync();

            var knowledge = new Knowledge
            {
                MemberId = member.Id,
                Member = member,
                ApplicationId = app.Id,
                Application = app,
                AssociatedSquadId = 1
            };
            _context.Knowledges.Add(knowledge);
            await _context.SaveChangesAsync();

            // Act
            var exists = await _repository.AssociationExistsAsync(member.Id, app.Id);

            // Assert
            Assert.True(exists);
        }


        [Fact]
        public async Task DeleteAsyncShouldRemoveOnlySpecificAssociation()
        {
            // Arrange
            var member = new Member { Id = 1, Name = "M", Role = "R", Cost = 1, Email = "m@x.com", SquadId = 1 };
            var app1 = new ApplicationData("App1") { Id = 2, SquadId = 1, ProductOwner = "PO" };
            var app2 = new ApplicationData("App2") { Id = 3, SquadId = 1, ProductOwner = "PO" };
            _context.Members.Add(member);
            _context.Applications.AddRange(app1, app2);
            await _context.SaveChangesAsync();

            var k1 = new Knowledge
            {
                MemberId = member.Id,
                Member = member,
                ApplicationId = app1.Id,
                Application = app1,
                AssociatedSquadId = 1
            };
            var k2 = new Knowledge
            {
                MemberId = member.Id,
                Member = member,
                ApplicationId = app2.Id,
                Application = app2,
                AssociatedSquadId = 1
            };
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
            var member = new Member { Id = 99, Name = "Filtro", Role = "Dev", Cost = 100, Email = "filtro@teste.com", SquadId = 77 };
            var app = new ApplicationData("AppFiltro") { Id = 88, SquadId = 77, ProductOwner = "PO" };
            _context.Members.Add(member);
            _context.Applications.Add(app);
            await _context.SaveChangesAsync();

            var filtroKnowledge = new Knowledge
            {
                MemberId = member.Id,
                Member = member,
                ApplicationId = app.Id,
                Application = app,
                AssociatedSquadId = 77
            };

            var knowledges = _fixture.CreateMany<Knowledge>(9).ToList();
            foreach (var k in knowledges)
            {
                k.Member = null;
                k.Application = null;
            }
            knowledges.Insert(0, filtroKnowledge);

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
            Assert.Equal(77, result.Result.First().AssociatedSquadId);
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
