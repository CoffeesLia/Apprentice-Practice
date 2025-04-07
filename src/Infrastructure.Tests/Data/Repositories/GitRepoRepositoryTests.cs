using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Infrastructure.Tests.Data.Repositories
{
    public class GitRepoRepositoryTests : IDisposable
    {
        private bool _disposed;

        private readonly Context _context;
        private readonly GitRepoRepository _repository;
        private readonly Fixture _fixture = new();

        public GitRepoRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new GitRepoRepository(_context);
        }

        [Fact]
        public async Task GetListAsyncByName()
        {
            // Arrange
            const int Count = 10;
            var gitRepoList = Enumerable.Range(1, Count).Select(i => new GitRepo($"Repo{i}")
            {
                Name = "Repo1",
                Description = "Description",
                Url = new Uri($"http://repo{i}.com"),
            }).ToList();

            await _context.Set<GitRepo>().AddRangeAsync(gitRepoList);
            await _context.SaveChangesAsync();

            var filter = new GitRepoFilter
            {
                Name = "Repo1",
                Description = "Description",
                Url = new Uri($"http://repo.com"),
            };

            var addedData = await _context.Set<GitRepo>().Where(x => x.Name.Contains(filter.Name)).ToListAsync();
            Assert.Equal(Count, addedData.Count);

            // Act
            var list = await _repository.GetListAsync(filter);

            // Assert
            Assert.Equal(Count, list.Total);
            Assert.All(list.Result, item => Assert.Contains(filter.Name, item.Name, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnEntity()
        {
            // Arrange
            var entity = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(entity.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveEntity()
        {
            // Arrange
            var entity = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(entity.Id);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Set<GitRepo>().FindAsync(entity.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task VerifyUrlAlreadyExistsAsyncShouldReturnTrueIfUrlExists()
        {
            // Arrange
            var entity = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.VerifyUrlAlreadyExistsAsync(entity.Url);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyAplicationsExistsAsyncShouldReturnTrueIfApplicationIdExists()
        {
            // Arrange
            var entity = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.VerifyAplicationsExistsAsync(entity.ApplicationId);

            // Assert
            Assert.True(result);
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

        ~GitRepoRepositoryTests()
        {
            Dispose(false);
        }
    }
}


