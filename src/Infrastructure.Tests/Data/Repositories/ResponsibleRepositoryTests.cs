using System.Runtime.InteropServices;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class ResponsibleRepositoryTests : IDisposable
    {
        private readonly Context _context;
        private readonly ResponsibleRepository _repository;
        private readonly Fixture _fixture = new();
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

        public ResponsibleRepositoryTests()
        {
            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new ResponsibleRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdExists()
        {
            // Arrange
            Responsible responsible = _fixture.Create<Responsible>();
            await _context.Set<Responsible>().AddAsync(responsible);
            await _context.SaveChangesAsync();

            // Act
            Responsible? result = await _repository.GetByIdAsync(responsible.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(responsible.Id, result.Id);
        }

        [Fact]
        public async Task GetByIdAsyncWhenIdDoesNotExist()
        {
            // Arrange
            int id = _fixture.Create<int>();

            // Act
            Responsible? result = await _repository.GetByIdAsync(id);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetListAsyncWhenCalled()
        {
            // Arrange
            ResponsibleFilter filter = new()
            {
                Page = 1,
                PageSize = 10,
                Name = _fixture.Create<string>(),
                Email = _fixture.Create<string>(),
                AreaId = _fixture.Create<int>()
            };
            const int Count = 10;
            IEnumerable<Responsible> responsibles = _fixture
                .Build<Responsible>()
                .With(x => x.Name, filter.Name)
                .With(x => x.Email, filter.Email)
                .Without(x => x.Area)
                .With(x => x.AreaId, filter.AreaId)
                .CreateMany(Count);

            await _context.Set<Responsible>().AddRangeAsync(responsibles);
            await _context.SaveChangesAsync();

            // Verifique se os dados foram salvos corretamente
            List<Responsible> savedResponsibles = await _context.Set<Responsible>().ToListAsync();
            Assert.Equal(Count, savedResponsibles.Count);

            // Act
            PagedResult<Responsible> result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, result.Total); // Verifica o total de itens
            Assert.Equal(filter.Page, result.Page); // Verifica a página solicitada
            Assert.Equal(filter.PageSize, result.PageSize); // Verifica o tamanho da página retornada
        }

        [Fact]
        public async Task VerifyEmailAlreadyExistsAsyncWhenEmailExists()
        {
            // Arrange
            string email = _fixture.Create<string>();
            Responsible responsible = _fixture.Build<Responsible>().With(x => x.Email, email).Create();
            await _context.Set<Responsible>().AddAsync(responsible);
            await _context.SaveChangesAsync();

            // Act
            bool result = await _repository.VerifyEmailAlreadyExistsAsync(email);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyEmailAlreadyExistsAsyncWhenEmailDoesNotExist()
        {
            // Arrange
            string email = _fixture.Create<string>();

            // Act
            bool result = await _repository.VerifyEmailAlreadyExistsAsync(email);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsyncWhenCalled()
        {
            // Arrange
            Responsible responsible = _fixture.Create<Responsible>();
            await _context.Set<Responsible>().AddAsync(responsible);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(responsible.Id);
            Responsible? result = await _context.Set<Responsible>().FindAsync(responsible.Id);

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
                // free managed resources
                _context?.Dispose();
            }

            // free native resources if there are any.
            if (nativeResource != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(nativeResource);
                nativeResource = IntPtr.Zero;
            }

            isDisposed = true;
        }
    }
}