using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using System.Runtime.InteropServices;

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

            IncidentFilter filter = new()
            {
                Page = 1,
                PageSize = 10,
                Title = _fixture.Create<string>(),
                ApplicationId = application.Id,
                Status = null
            };
            const int Count = 10;
            IEnumerable<Incident> incidents = _fixture
                .Build<Incident>()
                .With(x => x.Title, filter.Title)
                .With(x => x.ApplicationId, application.Id)
                .With(x => x.Application, application)
                .With(x => x.Status, IncidentStatus.Aberto)
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
        [InlineData(IncidentStatus.Aberto)]
        [InlineData(IncidentStatus.EmAtendimento)]
        [InlineData(IncidentStatus.Cancelado)]
        [InlineData(IncidentStatus.Fechado)]
        [InlineData(IncidentStatus.Reaberto)]
        public async Task GetListAsync_FilterByStatus_ReturnsOnlyMatchingStatus(IncidentStatus status)
        {
            // Arrange
            var application = _fixture.Build<ApplicationData>()
                .With(a => a.Id, 0)
                .Create();
            await _context.Set<ApplicationData>().AddAsync(application);
            await _context.SaveChangesAsync();

            // Cria 3 incidentes com o status desejado
            var matchingIncidents = _fixture.Build<Incident>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .With(i => i.Status, status)
                .CreateMany(3)
                .ToList();

            // Cria 2 incidentes com status diferente
            var otherIncidents = _fixture.Build<Incident>()
                .With(i => i.ApplicationId, application.Id)
                .With(i => i.Application, application)
                .With(i => i.Status, status == IncidentStatus.Aberto ? IncidentStatus.Fechado : IncidentStatus.Aberto)
                .CreateMany(2)
                .ToList();

            await _context.Set<Incident>().AddRangeAsync(matchingIncidents.Concat(otherIncidents));
            await _context.SaveChangesAsync();

            var filter = new IncidentFilter
            {
                Page = 1,
                PageSize = 10,
                Status = status,
                ApplicationId = application.Id
            };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(3, result.Total);
            Assert.All(result.Result, i => Assert.Equal(status, i.Status));
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

            var incidents = _fixture.Build<Incident>()
                .With(x => x.ApplicationId, application.Id)
                .With(x => x.Application, application)
                .CreateMany(3)
                .ToList();

            await _context.Set<Incident>().AddRangeAsync(incidents);
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
            var incident = _fixture.Build<Incident>()
                .With(i => i.Members, new List<Member> { member })
                .Create();

            await _context.Set<Incident>().AddAsync(incident);
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
            var status = IncidentStatus.Aberto;
            var incidents = _fixture.Build<Incident>()
                .With(i => i.Status, status)
                .CreateMany(2)
                .ToList();

            await _context.Set<Incident>().AddRangeAsync(incidents);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByStatusAsync(status);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, i => Assert.Equal(status, i.Status));
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
                .With(i => i.Members, new List<Member>()) // <-- Sem membros
                .Create();

            await _context.Set<Incident>().AddAsync(incident);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(incident.Id);
            Incident? result = await _context.Set<Incident>().FindAsync(incident.Id);

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