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
            _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task CreateAsyncShouldReturnSuccessWhenRepoIsValid()
        {
            GitRepo repo = _fixture.Create<GitRepo>();

            OperationResult result = await _repository.CreateAsync(repo);

            Assert.Equal(OperationStatus.Success, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenDbUpdateExceptionIsThrown()
        {
            GitRepo repo = _fixture.Create<GitRepo>();
            Mock<Context> contextMock = new(_options);
            contextMock.Setup(c => c.Set<GitRepo>().AddAsync(It.IsAny<GitRepo>(), default))
                       .ThrowsAsync(new DbUpdateException("DB error"));
            GitRepoRepository repoMock = new(contextMock.Object);

            OperationResult result = await repoMock.CreateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnConflictWhenArgumentExceptionIsThrown()
        {
            GitRepo repo = _fixture.Create<GitRepo>();
            Mock<Context> contextMock = new(_options);
            contextMock.Setup(c => c.Set<GitRepo>().AddAsync(It.IsAny<GitRepo>(), default))
                       .ThrowsAsync(new ArgumentException("Invalid argument"));
            GitRepoRepository repoMock = new(contextMock.Object);

            OperationResult result = await repoMock.CreateAsync(repo);

            Assert.Equal(OperationStatus.Conflict, result.Status);
        }

        [Theory]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(NotSupportedException))]
        public async Task CreateAsyncShouldReturnNotFoundWhenExpectedExceptionIsThrown(Type exceptionType)
        {
            GitRepo repo = _fixture.Create<GitRepo>();
            Mock<Context> contextMock = new(_options);
            Exception exception = (Exception)Activator.CreateInstance(exceptionType)!;

            contextMock.Setup(c => c.Set<GitRepo>().AddAsync(It.IsAny<GitRepo>(), default))
                       .ThrowsAsync(exception);
            GitRepoRepository repoMock = new(contextMock.Object);

            OperationResult result = await repoMock.CreateAsync(repo);

            Assert.Equal(OperationStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetRepositoryDetailsAsyncShouldReturnRepoWhenExists()
        {
            GitRepo repo = _fixture.Create<GitRepo>();
            await _context.Repositories.AddAsync(repo);
            await _context.SaveChangesAsync();

            GitRepo? result = await _repository.GetRepositoryDetailsAsync(repo.Id);

            Assert.NotNull(result);
            Assert.Equal(repo.Id, result!.Id);
        }



        [Fact]
        public async Task GetByIdAsyncShouldReturnCorrectRepo()
        {
            GitRepo repo = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            GitRepo? result = await _repository.GetByIdAsync(repo.Id);

            Assert.NotNull(result);
            Assert.Equal(repo.Id, result!.Id);
        }

        [Fact]
        public async Task DeleteAsyncWithSaveChangesShouldDeleteRepo()
        {
            GitRepo repo = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            await _repository.DeleteAsync(repo.Id, true);

            GitRepo? result = await _context.Set<GitRepo>().FindAsync(repo.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldDeleteRepoAndReturnTrue()
        {
            GitRepo repo = _fixture.Create<GitRepo>();
            await _context.Repositories.AddAsync(repo);
            await _context.SaveChangesAsync();

            bool deleted = await _repository.DeleteAsync(repo.Id);

            Assert.True(deleted);
            GitRepo? result = await _context.Repositories.FindAsync(repo.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsyncShouldReturnFalseWhenRepoDoesNotExist()
        {
            bool result = await _repository.DeleteAsync(9999);
            Assert.False(result);
        }


        [Fact]
        public async Task IsApplicationDataShouldReturnTrueWhenMatchExists()
        {
            ApplicationData appData = new("Test")
            {
                AreaId = 1,
                ResponsibleId = 2,
                ProductOwner = "Owner",
                ConfigurationItem = "ConfigItem"
            };
            await _context.Applications.AddAsync(appData);
            await _context.SaveChangesAsync();

            bool result = await _repository.IsApplicationDataFrom(appData.Id, 1);

            Assert.True(result);
        }

        [Fact]
        public async Task IRepositoryEntityBaseDeleteAsyncShouldWork()
        {
            GitRepo repo = _fixture.Create<GitRepo>();
            await _context.Repositories.AddAsync(repo);
            await _context.SaveChangesAsync();

            IRepositoryEntityBase<GitRepo> interfaceRepo = _repository;
            await interfaceRepo.DeleteAsync(repo.Id, true);

            GitRepo? result = await _context.Repositories.FindAsync(repo.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task VerifyUrlAlreadyExistsAsyncShouldReturnTrueWhenUrlExists()
        {
            GitRepo repo = _fixture.Build<GitRepo>()
                .With(r => r.Url, new Uri("https://test.com"))
                .Create();
            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            bool exists = await _repository.VerifyUrlAlreadyExistsAsync(new Uri("https://test.com"));

            Assert.True(exists);
        }

        [Fact]
        public async Task VerifyAplicationsExistsAsyncShouldReturnTrueWhenApplicationIdExists()
        {
            GitRepo repo = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            bool exists = await _repository.VerifyNameExistsAsync(repo.ApplicationId);

            Assert.True(exists);
        }

        [Fact]
        public async Task VerifyDescriptionExistsAsyncShouldReturnTrueWhenDescriptionExists()
        {
            // Arrange  
            GitRepo repo = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            // Act  
            bool result = await _repository.VerifyDescriptionExistsAsync(repo.Description);

            // Assert  
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyNameExistsAsyncShouldReturnTrueWhenNameExists()
        {
            // Arrange  
            GitRepo repo = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            // Act  
            bool result = await _repository.VerifyNameExistsAsync(repo.Name);

            // Assert  
            Assert.True(result);
        }

        [Fact]
        public async Task VerifyApplicationIdExistsAsyncShouldReturnTrueWhenApplicationIdExists()
        {
            // Arrange  
            GitRepo repo = _fixture.Create<GitRepo>();
            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            // Act  
            bool result = await _repository.VerifyApplicationIdExistsAsync(repo.ApplicationId);

            // Assert  
            Assert.True(result);
        }

        [Fact]
        public async Task GetListAsyncShouldApplyMultipleFiltersCorrectly()
        {
            GitRepo repo = _fixture.Build<GitRepo>()
                .With(r => r.Name, "SuperRepo")
                .With(r => r.Description, "Descrição bacana")
                .With(r => r.Url, new Uri("https://meurepo.com"))
                .Create();

            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            GitRepoFilter filter = new()
            {
                Name = "SuperRepo",
                Description = "Descrição bacana",
                Url = new Uri("https://meurepo.com")
            };

            PagedResult<GitRepo> result = await _repository.GetListAsync(filter);

            Assert.Single(result.Result);
            Assert.Equal("SuperRepo", result.Result.First().Name);

        }

        [Fact]
        public async Task GetListAsyncShouldThrowArgumentNullExceptionWhenFilterIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.GetListAsync(null!));
        }


        [Fact]
        public async Task ListRepositoriesShouldReturnAllRepositories()
        {
            // Arrange
            List<GitRepo> repos = [.. _fixture.CreateMany<GitRepo>(3)];
            await _context.Set<GitRepo>().AddRangeAsync(repos);
            await _context.SaveChangesAsync();

            // Act
            IAsyncEnumerable<GitRepo> result = _repository.ListRepositories();

            List<GitRepo> resultList = [];
            await foreach (GitRepo? repo in result.ConfigureAwait(false))
            {
                resultList.Add(repo);
            }

            // Assert
            Assert.Equal(3, resultList.Count);
            foreach (GitRepo? repo in repos)
            {
                Assert.Contains(resultList, r => r.Id == repo.Id);
            }
        }

        [Fact]
        public async Task ListRepositoriesShouldReturnEmptyWhenNoData()
        {
            // Act
            IAsyncEnumerable<GitRepo> result = _repository.ListRepositories();
            List<GitRepo> resultList = [];

            await foreach (GitRepo? repo in result.ConfigureAwait(false))
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
            GitRepo repo = _fixture.Build<GitRepo>()
                .With(r => r.Url, new Uri("https://github.com/example"))
                .With(r => r.ApplicationId, 42)
                .Create();

            await _context.Set<GitRepo>().AddAsync(repo);
            await _context.SaveChangesAsync();

            // Act
            IAsyncEnumerator<GitRepo> result = _repository.ListRepositories().GetAsyncEnumerator();
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
