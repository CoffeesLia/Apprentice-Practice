using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class MemberRepositoryTests : IDisposable
    {
        private bool _disposed;

        private readonly Context _context;
        private readonly MemberRepository _repository;
        private readonly Fixture _fixture = new();

        public MemberRepositoryTests()
        {
            DbContextOptions<Context> options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new MemberRepository(_context);
        }

        [Fact]
        public async Task GetListAsyncShouldApplyFiltersCorrectly()
        {
            // Arrange
            List<Member> members = [.. _fixture.CreateMany<Member>(10)];
            members[0].Name = "TestName";
            members[0].Role = "Admin";
            members[0].Email = "test@example.com";
            members[0].Cost = 100;

            _context.Members.AddRange(members);
            await _context.SaveChangesAsync();

            MemberFilter filter = new()
            {
                Name = "TestName",
                Role = "Admin",
                Email = "test@example.com",
                Cost = 100
            };

            // Act
            PagedResult<Member> result = await _repository.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Result); // Changed from result.Items to result.Result
            Assert.Equal("TestName", result.Result.First().Name);
            Assert.Equal("Admin", result.Result.First().Role);
            Assert.Equal("test@example.com", result.Result.First().Email);
            Assert.Equal(100, result.Result.First().Cost);
        }


        [Fact]
        public async Task CreateAsyncShouldAddMember()
        {
            // Arrange
            Member member = _fixture.Create<Member>();

            // Act
            await _repository.CreateAsync(member);
            await _context.SaveChangesAsync();

            // Assert
            Member? createdMember = await _context.Set<Member>().FindAsync(member.Id);
            Assert.NotNull(createdMember);
            Assert.Equal(member.Name, createdMember.Name);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnMember()
        {
            // Arrange
            Member member = _fixture.Create<Member>();
            await _repository.CreateAsync(member);
            await _context.SaveChangesAsync();

            // Act
            Member? retrievedMember = await _repository.GetByIdAsync(member.Id);

            // Assert
            Assert.NotNull(retrievedMember);
            Assert.Equal(member.Id, retrievedMember.Id);
        }

        [Fact]
        public async Task IsEmailUniqueWhenEmailExists()
        {
            // Arrange
            Member member = _fixture.Create<Member>();
            await _repository.CreateAsync(member);
            await _context.SaveChangesAsync();

            // Act
            bool isUnique = await _repository.IsEmailUnique(member.Email);

            // Assert
            Assert.False(isUnique);
        }

        [Fact]
        public async Task IsEmailUniqueWhenEmailDoesNotExist()
        {
            // Act
            bool isUnique = await _repository.IsEmailUnique("nonexistentemail@example.com");

            // Assert
            Assert.True(isUnique);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveMember()
        {
            // Arrange
            Member member = _fixture.Create<Member>();
            await _repository.CreateAsync(member);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(member.Id);
            await _context.SaveChangesAsync();

            // Assert
            Member? deletedMember = await _context.Set<Member>().FindAsync(member.Id);
            Assert.Null(deletedMember);
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

        ~MemberRepositoryTests()
        {
            Dispose(false);
        }
    }
}