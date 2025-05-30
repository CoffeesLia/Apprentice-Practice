using System.Runtime.InteropServices;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class ImprovementRepositoryTest : IDisposable
    {
        private readonly Context _context;
        private readonly ImprovementRepository _repository;
        private readonly Fixture _fixture;
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);


        public ImprovementRepositoryTest()
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
            _repository = new ImprovementRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdExists()
        {
            // Arrange
            Improvement improvement = _fixture.Create<Improvement>();
            await _context.Set<Improvement>().AddAsync(improvement);
            await _context.SaveChangesAsync();

            // Act
            Improvement? result = await _repository.GetByIdAsync(improvement.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(improvement.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();

            // Act
            Improvement? result = await _repository.GetByIdAsync(id);

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

            ImprovementFilter filter = new()
            {
                Page = 1,
                PageSize = 10,
                Title = _fixture.Create<string>(),
                ApplicationId = trackedApplication.Id,
                StatusImprovement = null
            };

            const int Count = 10;
            IEnumerable<Improvement> incidents = _fixture
                .Build<Improvement>()
                .With(x => x.Title, filter.Title)
                .With(x => x.ApplicationId, trackedApplication.Id)
                .With(x => x.Application, trackedApplication) // Use a instância rastreada
                .With(x => x.StatusImprovement, ImprovementStatus.Open)
                .CreateMany(Count);

            await _context.Set<Improvement>().AddRangeAsync(incidents);
            await _context.SaveChangesAsync();

            // Verifique se os dados foram salvos corretamente
            List<Improvement> savedIncidents = await _context.Set<Improvement>().ToListAsync();
            Assert.Equal(Count, savedIncidents.Count);

            // Act
            PagedResult<Improvement> result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, result.Total);
            Assert.Equal(filter.Page, result.Page);
            Assert.Equal(filter.PageSize, result.PageSize);

        }

        [Theory]
        [InlineData(ImprovementStatus.Open)]
        [InlineData(ImprovementStatus.InProgress)]
        [InlineData(ImprovementStatus.Cancelled)]
        [InlineData(ImprovementStatus.Closed)]
        [InlineData(ImprovementStatus.Reopened)]
        public async Task GetListAsyncFilterByStatusReturnsOnlyMatchingStatus(ImprovementStatus statusImprovement)
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
            var repository = new ImprovementRepository(context);

            var application = fixture.Build<ApplicationData>().Create();

            await context.Set<ApplicationData>().AddAsync(application);
            await context.SaveChangesAsync();

            var matchingIncidents = fixture.Build<Improvement>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .With(i => i.StatusImprovement, statusImprovement)
                .CreateMany(3)
                .ToList();

            var otherIncidents = fixture.Build<Improvement>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .With(i => i.StatusImprovement, statusImprovement == ImprovementStatus.Open ? ImprovementStatus.Closed : ImprovementStatus.Open)
                .CreateMany(2)
                .ToList();

            await context.Set<Improvement>().AddRangeAsync(matchingIncidents.Concat(otherIncidents));
            await context.SaveChangesAsync();

            var filter = new ImprovementFilter
            {
                Page = 1,
                PageSize = 10,
                StatusImprovement = statusImprovement,
                ApplicationId = application.Id
            };

            // Act
            var result = await repository.GetListAsync(filter);

            // Assert
            Assert.Equal(3, result.Total);
            Assert.All(result.Result, i => Assert.Equal(statusImprovement, i.StatusImprovement));
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

            var incidents = _fixture.Build<Improvement>()
                .With(x => x.ApplicationId, application.Id)
                .With(x => x.Application, application)
                .CreateMany(3)
                .ToList();

            await _context.Set<Improvement>().AddRangeAsync(incidents);
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
            var improvement = _fixture.Build<Improvement>()
                .With(i => i.Members, [member])
                .Create();

            await _context.Set<Improvement>().AddAsync(improvement);
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
            var statusImprovement = ImprovementStatus.Open;
            var incidents = _fixture.Build<Improvement>()
                .With(i => i.StatusImprovement, statusImprovement)
                .CreateMany(2)
                .ToList();

            await _context.Set<Improvement>().AddRangeAsync(incidents);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByStatusAsync(statusImprovement);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, i => Assert.Equal(statusImprovement, i.StatusImprovement));
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

            Improvement improvement = _fixture.Build<Improvement>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .With(i => i.Members, [])
                .Create();

            await _context.Set<Improvement>().AddAsync(improvement);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(improvement.Id);
            Improvement? result = await _context.Set<Improvement>().FindAsync(improvement.Id);

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
