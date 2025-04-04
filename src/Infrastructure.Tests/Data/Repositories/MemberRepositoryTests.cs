using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Stellantis.ProjectName.Domain.Entities;

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