using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Stellantis.ProjectName.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;

namespace Infrastructure.Tests.Data.Repositories
{
    public class DocumentDataRepositoryTest : IDisposable
    {
        private bool _disposed;

        private readonly Context _context;
        private readonly DocumentDataRepository _repository;
        private readonly Fixture _fixture = new();

        public DocumentDataRepositoryTest()
        {
            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new DocumentDataRepository(_context);
        }

        [Fact]
        public async Task CreateAndGetByIdAsync_ShouldPersistAndReturnDocument()
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
        public async Task DeleteAsync_ShouldRemoveDocument()
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
            var exists = await _repository.IsDocumentNameUniqueAsync(name, applicationId);

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
            var exists = await _repository.IsDocumentNameUniqueAsync(name, applicationId);

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
            var exists = await _repository.IsUrlUniqueAsync(url, applicationId);

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
            var exists = await _repository.IsDocumentNameUniqueAsync(name, applicationId);

            // Assert
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

        ~DocumentDataRepositoryTest()
        {
            Dispose(false);
        }
    }
}
