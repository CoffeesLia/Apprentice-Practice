using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Application.Models.Filters; 

namespace Infrastructure.Tests.Data.Repositories
{
    public class MemberRepositoryTests
    {
        private readonly Context _context;
        private readonly MemberRepository _repository;
        private readonly Fixture _fixture = new();

        public MemberRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: _fixture.Create<string>())
                .Options;
            _context = new Context(options);
            _repository = new MemberRepository(_context);
        }

        [Fact]
        public async Task GetListAsyncShouldApplyFiltersCorrectly()
        {
            // Arrange
            var members = _fixture.CreateMany<Member>(10).ToList();
            members[0].Name = "TestName";
            members[0].Role = "Admin";
            members[0].Email = "test@example.com";
            members[0].Cost = 100;

            _context.Members.AddRange(members);
            await _context.SaveChangesAsync();

            var filter = new MemberFilter
            {
                Name = "TestName",
                Role = "Admin",
                Email = "test@example.com",
                Cost = 100
            };

            // Act
            var result = await _repository.GetListAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Result); // Changed from result.Items to result.Result
            Assert.Equal("TestName", result.Result.First().Name);
            Assert.Equal("Admin", result.Result.First().Role);
            Assert.Equal("test@example.com", result.Result.First().Email);
            Assert.Equal(100, result.Result.First().Cost);
        }

        [Fact]
        public async Task CreateAsyncShouldAddMember() // Remove underscores
        {
            // Arrange
            var member = _fixture.Create<Member>();

            // Act
            await _repository.CreateAsync(member);
            await _context.SaveChangesAsync();

            // Assert
            var createdMember = await _context.Set<Member>().FindAsync(member.Id);
            Assert.NotNull(createdMember);
            Assert.Equal(member.Name, createdMember.Name);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddMember()
        {
            // Arrange
            var member = _fixture.Create<Member>();

            // Act
            await _repository.CreateAsync(member);
            await _context.SaveChangesAsync();

            // Assert
            var createdMember = await _context.Set<Member>().FindAsync(member.Id);
            Assert.NotNull(createdMember);
            Assert.Equal(member.Name, createdMember.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnMember()
        {
            // Arrange
            var member = _fixture.Create<Member>();
            await _repository.CreateAsync(member);
            await _context.SaveChangesAsync();

            // Act
            var retrievedMember = await _repository.GetByIdAsync(member.Id);

            // Assert
            Assert.NotNull(retrievedMember);
            Assert.Equal(member.Id, retrievedMember.Id);
        }

        [Fact]
        public async Task IsEmailUnique_ShouldReturnFalse_WhenEmailExists()
        {
            // Arrange
            var member = _fixture.Create<Member>();
            await _repository.CreateAsync(member);
            await _context.SaveChangesAsync();

            // Act
            var isUnique = await _repository.IsEmailUnique(member.Email);

            // Assert
            Assert.False(isUnique);
        }

        [Fact]
        public async Task IsEmailUnique_ShouldReturnTrue_WhenEmailDoesNotExist()
        {
            // Act
            var isUnique = await _repository.IsEmailUnique("nonexistentemail@example.com");

            // Assert
            Assert.True(isUnique);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveMember()
        {
            // Arrange
            var member = _fixture.Create<Member>();
            await _repository.CreateAsync(member);
            await _context.SaveChangesAsync();

            // Act
            await _repository.DeleteAsync(member.Id);
            await _context.SaveChangesAsync();

            // Assert
            var deletedMember = await _context.Set<Member>().FindAsync(member.Id);
            Assert.Null(deletedMember);
        }
    }
}