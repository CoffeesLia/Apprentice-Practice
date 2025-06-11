using System.Runtime.InteropServices;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class FeedbacksRepositoryTest : IDisposable
    {
        private readonly Context _context;
        private readonly FeedbacksRepository _repository;
        private readonly Fixture _fixture;
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);


        public FeedbacksRepositoryTest()
        {
            _fixture = new Fixture();
            // Evita exceção de referência circular no AutoFixture
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new FeedbacksRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdExists()
        {
            // Arrange
            Feedbacks feedbacks = _fixture.Create<Feedbacks>();
            await _context.Set<Feedbacks>().AddAsync(feedbacks);
            await _context.SaveChangesAsync();

            // Act
            Feedbacks? result = await _repository.GetByIdAsync(feedbacks.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(feedbacks.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();

            // Act
            Feedbacks? result = await _repository.GetByIdAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetListAsyncWhenCalled()
        {
            // Arrange
            var application = _fixture.Build<ApplicationData>()
                .With(a => a.Id, 0) // Deixe o EF Core gerar o Id
                .Create();
            await _context.Set<ApplicationData>().AddAsync(application);
            await _context.SaveChangesAsync();

            // Reutilize a instância rastreada de ApplicationData
            var trackedApplication = await _context.Set<ApplicationData>().FirstAsync(a => a.Id == application.Id);

            FeedbacksFilter filter = new()
            {
                Page = 1,
                PageSize = 10,
                Title = _fixture.Create<string>(),
                ApplicationId = trackedApplication.Id,
                StatusFeedbacks = null
            };

            const int Count = 10;
            IEnumerable<Feedbacks> incidents = _fixture
                .Build<Feedbacks>()
                .With(x => x.Title, filter.Title)
                .With(x => x.ApplicationId, trackedApplication.Id)
                .With(x => x.Application, trackedApplication) // Use a instância rastreada
                .With(x => x.StatusFeedbacks, FeedbacksStatus.Open)
                .CreateMany(Count);

            await _context.Set<Feedbacks>().AddRangeAsync(incidents);
            await _context.SaveChangesAsync();

            // Verifique se os dados foram salvos corretamente
            List<Feedbacks> savedIncidents = await _context.Set<Feedbacks>().ToListAsync();
            Assert.Equal(Count, savedIncidents.Count);

            // Act
            PagedResult<Feedbacks> result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, result.Total);
            Assert.Equal(filter.Page, result.Page);
            Assert.Equal(filter.PageSize, result.PageSize);

        }

        [Theory]
        [InlineData(FeedbacksStatus.Open)]
        [InlineData(FeedbacksStatus.InProgress)]
        [InlineData(FeedbacksStatus.Cancelled)]
        [InlineData(FeedbacksStatus.Closed)]
        [InlineData(FeedbacksStatus.Reopened)]
        public async Task GetListAsyncFilterByStatusReturnsOnlyMatchingStatus(FeedbacksStatus statusFeedbacks)
        {
            // Arrange
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // banco isolado!
                .Options;

            using var context = new Context(options);
            var repository = new FeedbacksRepository(context);

            var application = fixture.Build<ApplicationData>().Create();

            await context.Set<ApplicationData>().AddAsync(application);
            await context.SaveChangesAsync();

            var matchingIncidents = fixture.Build<Feedbacks>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .With(i => i.StatusFeedbacks, statusFeedbacks)
                .CreateMany(3)
                .ToList();

            var otherIncidents = fixture.Build<Feedbacks>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .With(i => i.StatusFeedbacks, statusFeedbacks == FeedbacksStatus.Open ? FeedbacksStatus.Closed : FeedbacksStatus.Open)
                .CreateMany(2)
                .ToList();

            await context.Set<Feedbacks>().AddRangeAsync(matchingIncidents.Concat(otherIncidents));
            await context.SaveChangesAsync();

            var filter = new FeedbacksFilter
            {
                Page = 1,
                PageSize = 10,
                StatusFeedbacks = statusFeedbacks,
                ApplicationId = application.Id
            };

            // Act
            var result = await repository.GetListAsync(filter);

            // Assert
            Assert.Equal(3, result.Total);
            Assert.All(result.Result, i => Assert.Equal(statusFeedbacks, i.StatusFeedbacks));
        }


        [Fact]
        public async Task GetByApplicationIdAsyncWhenExists()
        {
            // Arrange
            var application = _fixture.Build<ApplicationData>()
                .With(a => a.Id, 0) // Deixe o EF Core gerar o Id
                .Create();
            await _context.Set<ApplicationData>().AddAsync(application);
            await _context.SaveChangesAsync();

            var incidents = _fixture.Build<Feedbacks>()
                .With(x => x.ApplicationId, application.Id)
                .With(x => x.Application, application)
                .CreateMany(3)
                .ToList();

            await _context.Set<Feedbacks>().AddRangeAsync(incidents);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByApplicationIdAsync(application.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            Assert.All(result, i => Assert.Equal(application.Id, i.ApplicationId));
        }

        [Fact]
        public async Task GetByMemberIdAsyncWhenExists()
        {
            // Arrange
            int memberId = _fixture.Create<int>();
            var member = _fixture.Build<Member>().With(m => m.Id, memberId).Create();
            var feedbacks = _fixture.Build<Feedbacks>()
                .With(i => i.Members, [member])
                .Create();

            await _context.Set<Feedbacks>().AddAsync(feedbacks);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByMemberIdAsync(memberId);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(result, i => i.Members.Any(m => m.Id == memberId));
        }

        [Fact]
        public async Task GetByStatusAsyncWhenExists()
        {
            // Arrange
            var statusFeedbacks = FeedbacksStatus.Open;
            var incidents = _fixture.Build<Feedbacks>()
                .With(i => i.StatusFeedbacks, statusFeedbacks)
                .CreateMany(2)
                .ToList();

            await _context.Set<Feedbacks>().AddRangeAsync(incidents);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByStatusAsync(statusFeedbacks);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, i => Assert.Equal(statusFeedbacks, i.StatusFeedbacks));
        }

        [Fact]
        public async Task DeleteAsyncWhenCalled()
        {
            // Arrange
            var application = _fixture.Build<ApplicationData>()
                .With(a => a.Id, 0)
                .Create();
            await _context.Set<ApplicationData>().AddAsync(application);
            await _context.SaveChangesAsync();

            Feedbacks feedbacks = _fixture.Build<Feedbacks>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .With(i => i.Members, [])
                .Create();

            await _context.Set<Feedbacks>().AddAsync(feedbacks);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(feedbacks.Id);
            Feedbacks? result = await _context.Set<Feedbacks>().FindAsync(feedbacks.Id);

            // Assert
            Assert.Null(result);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _context?.Dispose();
            }

            if (nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeResource);
                nativeResource = IntPtr.Zero;
            }

            isDisposed = true;
        }
    }
}
