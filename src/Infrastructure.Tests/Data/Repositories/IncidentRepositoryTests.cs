using System.Runtime.InteropServices;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class IncidentRepositoryTests : IDisposable
    {
        private readonly Context _context;
        private readonly IncidentRepository _repository;
        private readonly Fixture _fixture;
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);


        public IncidentRepositoryTests()
        {
            _fixture = new Fixture();
          
            _fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new IncidentRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdExists()
        {
            // Arrange
            Incident incident = _fixture.Create<Incident>();
            await _context.Set<Incident>().AddAsync(incident);
            await _context.SaveChangesAsync();

            // Act
            Incident? result = await _repository.GetByIdAsync(incident.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(incident.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();

            // Act
            Incident? result = await _repository.GetByIdAsync(id);

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

            IncidentFilter filter = new()
            {
                Page = 1,
                PageSize = 10,
                Title = _fixture.Create<string>(),
                ApplicationId = trackedApplication.Id,
                Status = null
            };

            const int Count = 10;
            IEnumerable<Incident> incidents = _fixture
                .Build<Incident>()
                .With(x => x.Title, filter.Title)
                .With(x => x.ApplicationId, trackedApplication.Id)
                .With(x => x.Application, trackedApplication) 
                .With(x => x.Status, IncidentStatus.Open)
                .CreateMany(Count);

            await _context.Set<Incident>().AddRangeAsync(incidents);
            await _context.SaveChangesAsync();

            // Verifique se os dados foram salvos corretamente
            List<Incident> savedIncidents = await _context.Set<Incident>().ToListAsync();
            Assert.Equal(Count, savedIncidents.Count);

            // Act
            PagedResult<Incident> result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, result.Total);
            Assert.Equal(filter.Page, result.Page);
            Assert.Equal(filter.PageSize, result.PageSize);

        }

        [Theory]
        [InlineData(IncidentStatus.Open)]
        [InlineData(IncidentStatus.InProgress)]
        [InlineData(IncidentStatus.Cancelled)]
        [InlineData(IncidentStatus.Closed)]
        [InlineData(IncidentStatus.Reopened)]
        public async Task GetListAsyncFilterByStatusReturnsOnlyMatchingStatus(IncidentStatus status)
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
            var repository = new IncidentRepository(context);

            var application = fixture.Build<ApplicationData>().Create();

            await context.Set<ApplicationData>().AddAsync(application);
            await context.SaveChangesAsync();

            var matchingIncidents = fixture.Build<Incident>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .With(i => i.Status, status)
                .CreateMany(3)
                .ToList();

            var otherIncidents = fixture.Build<Incident>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .With(i => i.Status, status == IncidentStatus.Open ? IncidentStatus.Closed : IncidentStatus.Open)
                .CreateMany(2)
                .ToList();

            await context.Set<Incident>().AddRangeAsync(matchingIncidents.Concat(otherIncidents));
            await context.SaveChangesAsync();

            var filter = new IncidentFilter
            {
                Page = 1,
                PageSize = 10,
                Status = status,
                ApplicationId = application.Id
            };

            // Act
            var result = await repository.GetListAsync(filter);

            // Assert
            Assert.Equal(3, result.Total);
            Assert.All(result.Result, i => Assert.Equal(status, i.Status));
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

            Incident incident = _fixture.Build<Incident>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .With(i => i.Members, [])
                .Create();

            await _context.Set<Incident>().AddAsync(incident);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(incident.Id);
            Incident? result = await _context.Set<Incident>().FindAsync(incident.Id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMembersByApplicationIdAsyncReturnsMembersWhenApplicationExists()
        {
            // Arrange
            var squad = new Squad
            {
                Id = 1,
                Name = "Squad Alpha", // Propriedade obrigatória
                Description = "Squad de testes", // Propriedade obrigatória
            };
            var application = new ApplicationData("App") { Id = 1, SquadId = squad.Id, Squads = squad };
            await _context.Set<Squad>().AddAsync(squad);
            await _context.Set<ApplicationData>().AddAsync(application);
            await _context.SaveChangesAsync();

            var member1 = new Member
            {
                Id = 1,
                SquadId = squad.Id,
                Squad = squad,
                Name = "John Doe",
                Role = "Developer", 
                Cost = 1000.00m, 
                Email = "johndoe@example.com" 
            };
            var member2 = new Member
            {
                Id = 2,
                SquadId = squad.Id,
                Squad = squad,
                Name = "Jane Doe", 
                Role = "Tester",
                Cost = 800.00m, 
                Email = "janedoe@example.com" 
            };
            await _context.Set<Member>().AddRangeAsync(member1, member2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetMembersByApplicationIdAsync(application.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetMembersByApplicationIdAsyncReturnsEmptyWhenApplicationDoesNotExist()
        {
            // Arrange
            int nonExistentAppId = 999;

            // Act
            var result = await _repository.GetMembersByApplicationIdAsync(nonExistentAppId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetListAsyncSetsPageToOneWhenPageIsZeroOrNegative()
        {
            // Arrange
            var application = _fixture.Build<ApplicationData>().Create();
            await _context.Set<ApplicationData>().AddAsync(application);
            await _context.SaveChangesAsync();

            var incident = _fixture.Build<Incident>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .Create();

            await _context.Set<Incident>().AddAsync(incident);
            await _context.SaveChangesAsync();

            var filter = new IncidentFilter
            {
                Page = 0,
                PageSize = 10,
                ApplicationId = application.Id
            };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(1, result.Page);
        }

        [Fact]
        public async Task GetListAsyncFiltersByIdWhenIdIsGreaterThanZero()
        {
            // Arrange
            var application = _fixture.Build<ApplicationData>().Create();
            await _context.Set<ApplicationData>().AddAsync(application);
            await _context.SaveChangesAsync();

            var incident = _fixture.Build<Incident>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .Create();

            await _context.Set<Incident>().AddAsync(incident);
            await _context.SaveChangesAsync();

            var filter = new IncidentFilter
            {
                Id = incident.Id,
                Page = 1,
                PageSize = 10,
                ApplicationId = application.Id
            };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Single(result.Result);
            Assert.Equal(incident.Id, result.Result.First().Id);
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