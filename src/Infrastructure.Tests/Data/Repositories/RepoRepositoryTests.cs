using System.Runtime.InteropServices;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class RepoRepositoryTests : IDisposable
    {
        private readonly Context _context;
        private readonly RepoRepository _repository;
        private readonly Fixture _fixture = new();
        private bool isDisposed;
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);

        public RepoRepositoryTests()
        {
            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new RepoRepository(_context);
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

            RepoFilter filter = new()
            {

                Name = name,
                Url = url,
                ApplicationId = applicationId
            };


            await _context.SaveChangesAsync();

            // Act
            PagedResult<Repo> result = await _repository.GetListAsync(filter);

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
        public async Task CreateAndGetByIdAsyncShouldPersistAndReturnRepo()
        {
            // Arrange
            var repo = new Repo
            {
                Name = _fixture.Create<string>(),
                Url = new Uri("https://example.com/" + _fixture.Create<string>()),
                ApplicationId = 1
            };

            // Act
            await _repository.CreateAsync(repo);
            var result = await _repository.GetByIdAsync(repo.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(repo.Name, result!.Name);
            Assert.Equal(repo.Url, result.Url);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveRepo()
        {
            // Arrange
            var repo = new Repo
            {
                Name = _fixture.Create<string>(),
                Url = new Uri("https://example.com/" + _fixture.Create<string>()),
                ApplicationId = 1
            };
            await _repository.CreateAsync(repo);

            // Act
            await _repository.DeleteAsync(repo.Id);
            var result = await _repository.GetByIdAsync(repo.Id);

            // Assert
            Assert.Null(result);
        }





        [Fact]
        public async Task IsRepoNameUniqueAsyncShouldReturnFalseIfExists()
        {
            // Arrange
            var name = _fixture.Create<string>();
            var applicationId = _fixture.Create<int>();

            var repo = new Repo
        {
                Name = name,
                Url = new Uri("https://example.com/" + _fixture.Create<string>()),
                ApplicationId = 1
            };
            await _repository.CreateAsync(repo);    

            // Act
            var exists = await _repository.IsRepoNameUniqueAsync(name, applicationId);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task IsRepoNameUniqueAsyncShouldReturnFalseIfNotExists()
        {
            // Arrange
            var name = _fixture.Create<string>();
            var applicationId = _fixture.Create<int>();


            // Act
            var exists = await _repository.IsRepoNameUniqueAsync(name, applicationId);

            // Assert
            Assert.False(exists);
        }

        [Fact]
        public async Task IsUrlUniqueAsyncShouldReturnTrueIfExists()
        {
            // Arrange
            var applicationId = _fixture.Create<int>();
            var url = new Uri("https://example.com/" + _fixture.Create<string>());
            var repo = new Repo
            {
                Name = "Rep2",
                Description = "ValidDescription",
                Url = new Uri("https://example.com/rep2"),
                ApplicationId = 2
            };
            await _repository.CreateAsync(repo);

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
            var exists = await _repository.IsRepoNameUniqueAsync(name, applicationId);

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


//[Fact]
//public async Task VerifyDescriptionExistsAsyncShouldReturnTrueWhenDescriptionExists()
//{
//    // Arrange  
//    Repo repo = _fixture.Create<Repo>();
//    await _context.Set<Repo>().AddAsync(repo);
//    await _context.SaveChangesAsync();

//    // Act  
//    bool result = await _repository.VerifyDescriptionExistsAsync(repo.Description);

//    // Assert  
//    Assert.True(result);
//}

