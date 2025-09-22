using System;
using System.Linq;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
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
                SquadId = squad.Id,
                Status = KnowledgeStatus.Atual,
                Member = member,
                Squad = squad
            };
            knowledge.Applications.Add(application);

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
                SquadId = squad.Id,
                Status = KnowledgeStatus.Atual,
                Member = member,
                Squad = squad
            };
            knowledge.Applications.Add(application);
            knowledge.ApplicationIds.Add(application.Id); 

            await _repository.CreateAssociationAsync(knowledge);

            // Busque o Knowledge realmente persistido
            var persistedKnowledge = _context.Set<Knowledge>()
                .FirstOrDefault(k =>
                    k.MemberId == member.Id &&
                    k.SquadId == squad.Id &&
                    k.Status == KnowledgeStatus.Atual &&
                    k.Applications.Any(a => a.Id == application.Id)
                );

            // Act
            Knowledge? retrievedKnowledge = await _repository.GetByIdAsync(persistedKnowledge!.Id);

            // Assert
            Assert.NotNull(retrievedKnowledge);
            Assert.Equal(persistedKnowledge.Id, retrievedKnowledge.Id);
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
                SquadId = squad.Id,
                Status = KnowledgeStatus.Atual,
                Member = member,
                Squad = squad
            };
            knowledge.Applications.Add(application);

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

        [Fact]
        public async Task ListMembersByApplicationAsyncShouldReturnMembers()
        {
            // Arrange
            var member1 = _fixture.Build<Member>().With(m => m.Id, 111).Create();
            var member2 = _fixture.Build<Member>().With(m => m.Id, 112).Create();
            var application = _fixture.Build<ApplicationData>().With(a => a.Id, 211).Create();
            var squad = _fixture.Build<Squad>().With(s => s.Id, 311).Create();

            _context.Members.AddRange(member1, member2);
            _context.Applications.Add(application);
            _context.Squads.Add(squad);
            await _context.SaveChangesAsync();

            var knowledge1 = new Knowledge
            {
                MemberId = member1.Id,
                SquadId = squad.Id,
                Status = KnowledgeStatus.Atual,
                Member = member1,
                Squad = squad
            };
            knowledge1.Applications.Add(application);
            var knowledge2 = new Knowledge
            {
                MemberId = member2.Id,
                SquadId = squad.Id,
                Status = KnowledgeStatus.Passado,
                Member = member2,
                Squad = squad
            };
            knowledge2.Applications.Add(application);

            await _repository.CreateAssociationAsync(knowledge1);
            await _repository.CreateAssociationAsync(knowledge2);

            // Act
            var allMembers = await _repository.ListMembersByApplicationAsync(application.Id);
            var atualMembers = await _repository.ListMembersByApplicationAsync(application.Id, KnowledgeStatus.Atual);

            // Assert
            Assert.Contains(allMembers, m => m.Id == member1.Id);
            Assert.Contains(allMembers, m => m.Id == member2.Id);
            Assert.Single(atualMembers.Where(m => m.Id == member1.Id));
            Assert.DoesNotContain(atualMembers, m => m.Id == member2.Id);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveKnowledgeById()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 121).Create();
            var application = _fixture.Build<ApplicationData>().With(a => a.Id, 221).Create();
            var squad = _fixture.Build<Squad>().With(s => s.Id, 321).Create();

            _context.Members.Add(member);
            _context.Applications.Add(application);
            _context.Squads.Add(squad);
            await _context.SaveChangesAsync();

            var knowledge = new Knowledge
            {
                MemberId = member.Id,
                SquadId = squad.Id,
                Status = KnowledgeStatus.Atual,
                Member = member,
                Squad = squad
            };
            knowledge.Applications.Add(application);

            await _repository.CreateAssociationAsync(knowledge);

            // Act
            await _repository.DeleteAsync(knowledge.Id);

            // Assert
            var retrieved = await _repository.GetByIdAsync(knowledge.Id);
            Assert.Null(retrieved);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultWithFilters()
        {
            // Arrange
            var member1 = _fixture.Build<Member>().With(m => m.Id, 201).Create();
            var member2 = _fixture.Build<Member>().With(m => m.Id, 202).Create();
            var application1 = _fixture.Build<ApplicationData>().With(a => a.Id, 301).Create();
            var application2 = _fixture.Build<ApplicationData>().With(a => a.Id, 302).Create();
            var squad1 = _fixture.Build<Squad>().With(s => s.Id, 401).Create();
            var squad2 = _fixture.Build<Squad>().With(s => s.Id, 402).Create();

            _context.Members.AddRange(member1, member2);
            _context.Applications.AddRange(application1, application2);
            _context.Squads.AddRange(squad1, squad2);
            await _context.SaveChangesAsync();

            var knowledge1 = new Knowledge
            {
                MemberId = member1.Id,
                SquadId = squad1.Id,
                Status = KnowledgeStatus.Atual,
                Member = member1,
                Squad = squad1
            };
            knowledge1.Applications.Add(application1);
            knowledge1.ApplicationIds.Add(application1.Id);

            var knowledge2 = new Knowledge
            {
                MemberId = member2.Id,
                SquadId = squad2.Id,
                Status = KnowledgeStatus.Passado,
                Member = member2,
                Squad = squad2
            };
            knowledge2.Applications.Add(application2);
            knowledge2.ApplicationIds.Add(application2.Id);

            await _repository.CreateAssociationAsync(knowledge1);
            await _repository.CreateAssociationAsync(knowledge2);
            // Act
            var filter = new KnowledgeFilter
            {
                MemberId = member1.Id,
                ApplicationId = application1.Id,
                SquadId = squad1.Id,
                Status = KnowledgeStatus.Atual,
                Page = 1,
                PageSize = 10
            };
            var pagedResult = await _repository.GetListAsync(filter);

            // Assert
            Assert.NotNull(pagedResult);
            Assert.Equal(1, pagedResult.Total);
            Assert.Single(pagedResult.Result);
            Assert.Equal(knowledge1.Id, pagedResult.Result.First().Id);
            Assert.Equal(1, pagedResult.Page);
            Assert.Equal(10, pagedResult.PageSize);
        }

        [Fact]
        public async Task GetListAsyncShouldRespectPagination()
        {
            // Arrange
            var member = _fixture.Build<Member>().With(m => m.Id, 221).Create();
            var application = _fixture.Build<ApplicationData>().With(a => a.Id, 321).Create();
            var squad = _fixture.Build<Squad>().With(s => s.Id, 421).Create();

            _context.Members.Add(member);
            _context.Applications.Add(application);
            _context.Squads.Add(squad);
            await _context.SaveChangesAsync();

            for (int i = 0; i < 15; i++)
            {
                var knowledge = new Knowledge
                {
                    MemberId = member.Id,
                    SquadId = squad.Id,
                    Status = KnowledgeStatus.Atual,
                    Member = member,
                    Squad = squad
                };
                knowledge.Applications.Add(application);
                knowledge.ApplicationIds.Add(application.Id); 
                await _repository.CreateAssociationAsync(knowledge);
            }

            // Act
            var filter = new KnowledgeFilter { Page = 2, PageSize = 10 };
            var pagedResult = await _repository.GetListAsync(filter);

            // Assert
            Assert.NotNull(pagedResult);
            Assert.Equal(15, pagedResult.Total);     
            Assert.Equal(2, pagedResult.Page);
            Assert.Equal(10, pagedResult.PageSize);
            Assert.Equal(5, pagedResult.Result.Count()); 
        }
        ~KnowledgeRepositoryTests()
        {
            Dispose(false);
        }
    }
}