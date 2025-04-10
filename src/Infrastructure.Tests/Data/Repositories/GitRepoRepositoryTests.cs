using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class GitRepoRepositoryTests : IDisposable
    {
        private bool _disposed;
        private readonly Context _context;
        private readonly GitRepoRepository _repository;
        private readonly Fixture _fixture = new();


        private readonly DbContextOptions<Context> _options;

        public GitRepoRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new Context(_options);
            _repository = new GitRepoRepository(_context);
        }


        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenRepoIsValid()
        {
            var repo = _fixture.Create<GitRepo>();

            var result = await _repository.CreateAsync(repo);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenDbUpdateExceptionIsThrown()
        {
            var repo = _fixture.Create<GitRepo>();
            var contextMock = new Mock<Context>(_options);
            contextMock.Setup(c => c.Set<GitRepo>().AddAsync(It.IsAny<GitRepo>(), default))
                       .ThrowsAsync(new DbUpdateException("DB error"));
            var repoMock = new GitRepoRepository(contextMock.Object);

            var result = await repoMock.CreateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenArgumentExceptionIsThrown()
        {
            var repo = _fixture.Create<GitRepo>();
            var contextMock = new Mock<Context>(_options);
            contextMock.Setup(c => c.Set<GitRepo>().AddAsync(It.IsAny<GitRepo>(), default))
                       .ThrowsAsync(new ArgumentException("Invalid argument"));
            var repoMock = new GitRepoRepository(contextMock.Object);

            var result = await repoMock.CreateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Theory]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(NotSupportedException))]
        public async Task CreateAsyncShouldReturnNotFoundWhenExpectedExceptionIsThrown(Type exceptionType)
        {
            var repo = _fixture.Create<GitRepo>();
            var contextMock = new Mock<Context>(_options);
            var exception = (Exception)Activator.CreateInstance(exceptionType)!;

            contextMock.Setup(c => c.Set<GitRepo>().AddAsync(It.IsAny<GitRepo>(), default))
                       .ThrowsAsync(exception);
            var repoMock = new GitRepoRepository(contextMock.Object);

            var result = await repoMock.CreateAsync(repo);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetRepositoryDetailsAsyncShouldReturnRepoWhenExists()
        {
            var repo = _fixture.Create<GitRepo>();
            await _context.GitRepo.AddAsync(repo);
            await _context.SaveChangesAsync();

            var result = await _repository.GetRepositoryDetailsAsync(repo.Id);

            Assert.NotNull(result);
            Assert.Equal(repo.Id, result!.Id);
        }



        [Fact]
        public async Task GetByIdAsyncShouldReturnCorrectRepo()
        {
            var repo = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(repo.Id);

            Assert.NotNull(result);
            Assert.Equal(repo.Id, result!.Id);
        }

        [Fact]
        public async Task DeleteAsyncWithSaveChangesShouldDeleteRepo()
        {
            var repo = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(repo.Id, true);

            var result = await _context.Set<GitRepo>().FindAsync(repo.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldDeleteRepoAndReturnTrue()
        {
            var repo = _fixture.Create<GitRepo>();
            await _context.GitRepo.AddAsync(repo);
            await _context.SaveChangesAsync();

            var deleted = await _repository.DeleteAsync(repo.Id);

            Assert.True(deleted);
            var result = await _context.GitRepo.FindAsync(repo.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnFalsWhenRepoDoesNotExist()
        {
            var result = await _repository.DeleteAsync(9999);
            Assert.False(result);
        }


        [Fact]
        public async Task IsApplicationDataFromShouldReturnTrueWhenMatchExists()
        {
            var appData = new ApplicationData("Test")
            {
                AreaId = 1,
                ResponsibleId = 2,
                ProductOwner = "Owner",
                ConfigurationItem = "ConfigItem"
            };
            await _context.ApplicationDatas.AddAsync(appData);
            await _context.SaveChangesAsync();

            // Use o ID real do appData salvo
            var result = await _repository.IsApplicationDataFrom(appData.Id, 1);

            Assert.True(result);
        }

        [Fact]
        public async Task IRepositoryEntityBaseDeleteAsyncShouldWork()
        {
            var repo = _fixture.Create<GitRepo>();
            await _context.GitRepo.AddAsync(repo);
            await _context.SaveChangesAsync();

            var interfaceRepo = (IRepositoryEntityBase<GitRepo>)_repository;
            await interfaceRepo.DeleteAsync(repo.Id, true);

            var result = await _context.GitRepo.FindAsync(repo.Id);
            Assert.Null(result);
        }


        [Fact]
        public async Task AnyAsyncShouldReturnTrueWhenMatchingRepoExists()
        {
            var repo = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            var result = await _repository.AnyAsync(x => x.Id == repo.Id);

            Assert.True(result);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldThrowNotImplementedExceptionWhenCalled()
        {
            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                _repository.VerifyNameAlreadyExistsAsync("TestName"));
        }

        [Fact]
        public async Task VerifyUrlAlreadyExistsAsyncShouldReturnTrueWhenUrlExists()
        {
            var repo = _fixture.Build<GitRepo>()
                .With(r => r.Url, new Uri("https://test.com"))
                .Create();
            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            var exists = await _repository.VerifyUrlAlreadyExistsAsync(new Uri("https://test.com"));

            Assert.True(exists);
        }

        [Fact]
        public async Task VerifyAplicationsExistsAsyncShouldReturnTrueWhenApplicationIdExists()
        {
            var repo = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            var exists = await _repository.VerifyAplicationsExistsAsync(repo.ApplicationId);

            Assert.True(exists);
        }

        [Fact]
        public async Task GetListAsyncShouldThrowNotImplementedExceptionWhenCalled()
        {
            // Arrange
            var filter = new GitRepoFilter
            {
                Description = "Test",
                Name = "Test",
                Url = new Uri("https://test.com"),
                ApplicationId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() =>
                _repository.GetListAsync(filter));
        }


        [Fact]
        public async Task ListRepositoriesShouldReturnAllRepositories()
        {
            // Arrange
            var repos = _fixture.CreateMany<GitRepo>(3).ToList();
            await _context.Set<GitRepo>().AddRangeAsync(repos);
            await _context.SaveChangesAsync();

            // Act
            var result = _repository.ListRepositories();

            var resultList = new List<GitRepo>();
            await foreach (var repo in result.ConfigureAwait(false))
            {
                resultList.Add(repo);
            }

            // Assert
            Assert.Equal(3, resultList.Count);
            foreach (var repo in repos)
            {
                Assert.Contains(resultList, r => r.Id == repo.Id);
            }
        }

        [Fact]
        public async Task ListRepositoriesShouldReturnEmptyWhenNoData()
        {
            // Act
            var result = _repository.ListRepositories();
            var resultList = new List<GitRepo>();

            await foreach (var repo in result.ConfigureAwait(false))
            {
                resultList.Add(repo);
            }

            // Assert
            Assert.Empty(resultList);
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

        [Fact]
        public async Task ListRepositoriesShouldPreserveDataIntegrity()
        {
            // Arrange
            var repo = _fixture.Build<GitRepo>()
                .With(r => r.Url, new Uri("https://github.com/example"))
                .With(r => r.ApplicationId, 42)
                .Create();

            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            // Act
            var result = _repository.ListRepositories().GetAsyncEnumerator();
            GitRepo? retrievedRepo = null;

            if (await result.MoveNextAsync())
            {
                retrievedRepo = result.Current;
            }

            // Assert
            Assert.NotNull(retrievedRepo);
            Assert.Equal(repo.Url, retrievedRepo!.Url);
            Assert.Equal(repo.ApplicationId, retrievedRepo.ApplicationId);
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
