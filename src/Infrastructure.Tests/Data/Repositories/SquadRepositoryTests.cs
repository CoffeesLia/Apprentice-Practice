using Microsoft.EntityFrameworkCore;
using Moq;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Xunit;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Infrastructure.Tests.Data.Repositories
{
    public class SquadRepositoryTests
    {
        private readonly DbContextOptions<Context> _dbContextOptions;

        public SquadRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }

        [Fact]
        public async Task CreateAsyncShouldAddSquad()
        {
            using var context = new Context(_dbContextOptions);
            var repository = new SquadRepository(context);

            var squad = new Squad { Name = "Test Squad" };
            await repository.CreateAsync(squad);

            var createdSquad = await context.Squads.FirstOrDefaultAsync(s => s.Name == "Test Squad");
            Assert.NotNull(createdSquad);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnSquad()
        {
            using var context = new Context(_dbContextOptions);
            var repository = new SquadRepository(context);

            var squad = new Squad { Name = "Test Squad" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            var retrievedSquad = await repository.GetByIdAsync(squad.Id);
            Assert.NotNull(retrievedSquad);
            Assert.Equal(squad.Name, retrievedSquad?.Name);
        }

        [Fact]
        public async Task UpdateAsyncShouldUpdateSquad()
        {
            using var context = new Context(_dbContextOptions);
            var repository = new SquadRepository(context);

            var squad = new Squad { Name = "Test Squad" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            squad.Name = "Updated Squad";
            await repository.UpdateAsync(squad);

            var updatedSquad = await context.Squads.FirstOrDefaultAsync(s => s.Id == squad.Id);
            Assert.NotNull(updatedSquad);
            Assert.Equal("Updated Squad", updatedSquad?.Name);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveSquad()
        {
            using var context = new Context(_dbContextOptions);
            var repository = new SquadRepository(context);

            var squad = new Squad { Name = "Test Squad" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            await repository.DeleteAsync(squad.Id);

            var deletedSquad = await context.Squads.FirstOrDefaultAsync(s => s.Id == squad.Id);
            Assert.Null(deletedSquad);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnTrueIfNameExists()
        {
            using var context = new Context(_dbContextOptions);
            var repository = new SquadRepository(context);

            var squad = new Squad { Name = "Test Squad" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            var exists = await repository.VerifyNameAlreadyExistsAsync("Test Squad");
            Assert.True(exists);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnFilteredSquads()
        {
            using var context = new Context(_dbContextOptions);
            var repository = new SquadRepository(context);

            context.Squads.AddRange(
                new Squad { Name = "Squad A" },
                new Squad { Name = "Squad B" }
            );
            await context.SaveChangesAsync();

            var filter = new SquadFilter { Name = "Squad A" };
            var result = await repository.GetListAsync(filter);

            Assert.Single(result.Result);
            Assert.Equal("Squad A", result.Result.First().Name);
        }

        [Fact]
        public async Task VerifySquadExistsAsyncShouldReturnTrueIfSquadExists()
        {
            using var context = new Context(_dbContextOptions);
            var repository = new SquadRepository(context);

            var squad = new Squad { Name = "Test Squad" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            var exists = await repository.VerifySquadExistsAsync(squad.Id);
            Assert.True(exists);
        }
    }
}
