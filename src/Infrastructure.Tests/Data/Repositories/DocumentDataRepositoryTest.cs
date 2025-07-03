using System.Runtime.InteropServices;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class DocumentDataRepositoryTests : IDisposable
    {
        private readonly Context _context;
        private readonly DocumentDataRepository _repository;
        private readonly Fixture _fixture = new();
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

        public DocumentDataRepositoryTests()
        {
            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new DocumentDataRepository(_context);
            _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task GetListAsyncWhenCalled()
        {
            // Arrange
            var applicationId = 1;
            var name = _fixture.Create<string>();
            var url = _fixture.Create<Uri>();

            DocumentDataFilter filter = new()
            {

                Name = name,
                Url = url,
                ApplicationId = applicationId
            };


            await _context.SaveChangesAsync();

            // Act
            PagedResult<DocumentData> result = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(filter.Page, result.Page);
            Assert.Equal(filter.PageSize, result.PageSize);
        }

        [Fact]
        public async Task GetListAsyncShouldThrowIfFilterIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetListAsync(null!));
        }


        [Fact]
        public async Task CreateAndGetByIdAsyncShouldPersistAndReturnDocument()
        {
            // Arrange
            var document = new DocumentData
            {
                Name = _fixture.Create<string>(),
                Url = new Uri("https://example.com/" + _fixture.Create<string>()),
                ApplicationId = 1
            };

            // Act
            await _repository.CreateAsync(document);
            var result = await _repository.GetByIdAsync(document.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(document.Name, result!.Name);
            Assert.Equal(document.Url, result.Url);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveDocument()
        {
            // Arrange
            var document = new DocumentData
            {
                Name = _fixture.Create<string>(),
                Url = new Uri("https://example.com/" + _fixture.Create<string>()),
                ApplicationId = 1
            };
            await _repository.CreateAsync(document);

            // Act
            await _repository.DeleteAsync(document.Id);
            var result = await _repository.GetByIdAsync(document.Id);

            // Assert
            Assert.Null(result);
        }





        [Fact]
        public async Task IsDocumentNameUniqueAsyncShouldReturnFalseIfExists()
        {
            // Arrange
            var name = _fixture.Create<string>();
            var applicationId = _fixture.Create<int>();

            var document = new DocumentData
            {
                Name = name,
                Url = new Uri("https://example.com/" + _fixture.Create<string>()),
                ApplicationId = 1
            };
            await _repository.CreateAsync(document);

            // Act
            var exists = await _repository.NameAlreadyExists(name, applicationId);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task IsDocumentNameUniqueAsyncShouldReturnFalseIfNotExists()
        {
            // Arrange
            var name = _fixture.Create<string>();
            var applicationId = _fixture.Create<int>();


            // Act
            var exists = await _repository.NameAlreadyExists(name, applicationId);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task IsUrlUniqueAsyncShouldReturnTrueIfExists()
        {
            // Arrange
            var applicationId = _fixture.Create<int>();
            var url = new Uri("https://example.com/" + _fixture.Create<string>());
            var document = new DocumentData
            {
                Name = _fixture.Create<string>(),
                Url = url,
                ApplicationId = applicationId // Use o mesmo applicationId
            };
            await _repository.CreateAsync(document);

            // Act
            var exists = await _repository.UrlAlreadyExists(url, applicationId);

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task IsUrlUniqueAsyncShouldReturnFalseIfNotExists()
        {
            // Arrange
            var name = _fixture.Create<string>();
            var applicationId = _fixture.Create<int>();
            var url = new Uri("https://example.com/" + _fixture.Create<string>());

            // Act
            var exists = await _repository.NameAlreadyExists(name, applicationId);

            // Assert
            Assert.False(exists);
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
