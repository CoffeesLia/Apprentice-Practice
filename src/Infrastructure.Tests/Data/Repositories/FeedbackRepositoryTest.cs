using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class FeedbackRepositoryTests : IDisposable
    {
        private readonly Context _context;
        private readonly FeedbackRepository _repository;
        private readonly Fixture _fixture;

        public FeedbackRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new Context(options);
            _repository = new FeedbackRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsyncReturnsFeedbackWhenExists()
        {
            // Arrange
            var feedback = _fixture.Build<Feedback>()
                .With(f => f.Id, 0)
                .Create();
            await _context.Set<Feedback>().AddAsync(feedback);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(feedback.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(feedback.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsyncReturnsNullWhenNotExists()
        {
            // Act
            var result = await _repository.GetByIdAsync(-1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsyncRemovesFeedbackWhenExists()
        {
            // Arrange
            var feedback = _fixture.Build<Feedback>()
                .With(f => f.Id, 0)
                .Create();
            await _context.Set<Feedback>().AddAsync(feedback);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(feedback.Id);
            var deleted = await _context.Set<Feedback>().FindAsync(feedback.Id);

            // Assert
            Assert.Null(deleted);
        }

        [Fact]
        public async Task GetListAsyncReturnsPagedResultWithFilter()
        {
            // Arrange
            var application = _fixture.Build<ApplicationData>()
                .With(a => a.Id, 0)
                .Create();
            await _context.Set<ApplicationData>().AddAsync(application);
            await _context.SaveChangesAsync();

            var trackedApp = await _context.Set<ApplicationData>().FirstAsync(a => a.Id == application.Id);

            var feedbacks = _fixture.Build<Feedback>()
                .With(f => f.Title, "TestTitle")
                .With(f => f.ApplicationId, trackedApp.Id)
                .With(f => f.Application, trackedApp)
                .With(f => f.Status, FeedbackStatus.Open)
                .CreateMany(5)
                .ToList();

            await _context.Set<Feedback>().AddRangeAsync(feedbacks);
            await _context.SaveChangesAsync();

            var filter = new FeedbackFilter
            {
                Page = 1,
                PageSize = 10,
                Title = "TestTitle",
                ApplicationId = trackedApp.Id,
                Status = FeedbackStatus.Open
            };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Total);
            Assert.All(result.Result, f => Assert.Equal("TestTitle", f.Title));
        }

        [Fact]
        public async Task GetListAsyncSetsPageToOneWhenPageIsZeroOrNegative()
        {
            // Arrange
            var application = _fixture.Build<ApplicationData>()
                .With(a => a.Id, 1)
                .Create();
            await _context.Set<ApplicationData>().AddAsync(application);
            await _context.SaveChangesAsync();

            var feedbacks = _fixture.Build<Feedback>()
                .With(f => f.ApplicationId, application.Id)
                .With(f => f.Application, application)
                .CreateMany(3)
                .ToList();

            await _context.Set<Feedback>().AddRangeAsync(feedbacks);
            await _context.SaveChangesAsync();

            var filter = new FeedbackFilter
            {
                Page = 0,
                PageSize = 10,
                ApplicationId = application.Id
            };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(1, result.Page);
            Assert.Equal(3, result.Total);
        }

        [Fact]
        public async Task GetMembersByApplicationIdAsyncReturnsMembersWhenApplicationExists()
        {
            // Arrange
            var squad = _fixture.Build<Squad>().With(s => s.Id, 0).Create();
            await _context.Squads.AddAsync(squad);
            await _context.SaveChangesAsync();

            var application = _fixture.Build<ApplicationData>()
                .With(a => a.SquadId, squad.Id)
                .Create();
            await _context.Applications.AddAsync(application);
            await _context.SaveChangesAsync();

            var trackedApplication = await _context.Applications.FirstAsync(a => a.Name == application.Name);

            var member = _fixture.Build<Member>()
                .With(m => m.SquadId, squad.Id)
                .Create();
            await _context.Members.AddAsync(member);
            await _context.SaveChangesAsync();

            var trackedMember = await _context.Members.FirstAsync(m => m.Name == member.Name);

            // Crie e salve um feedback que relacione o member à application
            var feedback = _fixture.Build<Feedback>()
                .With(f => f.ApplicationId, trackedApplication.Id)
                .With(f => f.Application, trackedApplication)
                .With(f => f.Members, [trackedMember])
                .Create();
            await _context.Feedbacks.AddAsync(feedback);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMembersByApplicationIdAsync(trackedApplication.Id);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetMembersByApplicationIdAsyncReturnsEmptyWhenApplicationNotExists()
        {
            // Act
            var result = await _repository.GetMembersByApplicationIdAsync(-1);

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(FeedbackStatus.Open)]
        [InlineData(FeedbackStatus.InProgress)]
        [InlineData(FeedbackStatus.Closed)]
        public async Task GetByStatusAsyncReturnsOnlyMatchingStatus(FeedbackStatus status)
        {
            // Arrange
            var feedbacks = _fixture.Build<Feedback>()
                .With(f => f.Status, status)
                .CreateMany(3)
                .ToList();

            var others = _fixture.Build<Feedback>()
                .With(f => f.Status, FeedbackStatus.Cancelled)
                .CreateMany(2)
                .ToList();

            await _context.Feedbacks.AddRangeAsync(feedbacks.Concat(others));
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByStatusAsync(status);

            // Assert
            Assert.All(result, f => Assert.Equal(status, f.Status));
        }

        [Fact]
        public async Task GetListAsyncFiltersByIdWhenIdIsGreaterThanZero()
        {
            // Arrange
            var feedback = _fixture.Build<Feedback>()
                .With(f => f.Id, 123)
                .With(f => f.Title, "FiltrarPorId")
                .Create();
            await _context.Set<Feedback>().AddAsync(feedback);
            await _context.SaveChangesAsync();

            var filter = new FeedbackFilter
            {
                Id = 123, // Ativa o filtro por Id
                Page = 1,
                PageSize = 10
            };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Result);
            Assert.Equal(123, result.Result.First().Id);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context?.Dispose();
            }
        }
    }
}