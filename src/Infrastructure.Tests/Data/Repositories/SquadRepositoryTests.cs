using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;
using Xunit;

namespace Infrastructure.Tests.Data.Repositories
{
    public class SquadRepositoryTests
    {
        private static DbContextOptions<Context> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
        }


        [Fact]
        public async Task CreateAsyncShouldAddSquad()
        {
            var options = CreateNewContextOptions();
            using var context = new Context(options);
            var repository = new SquadRepository(context);

            var squad = new Squad { Name = "Test Squad", Description = "Test Description" };
            await repository.CreateAsync(squad);

            var created = await context.Squads.FirstOrDefaultAsync(s => s.Name == "Test Squad");
            Assert.NotNull(created);
            Assert.Equal("Test Description", created.Description);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnSquad()
        {
            var options = CreateNewContextOptions();
            using var context = new Context(options);
            var repository = new SquadRepository(context);

            var squad = new Squad { Name = "Test Squad", Description = "Test Description" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            var result = await repository.GetByIdAsync(squad.Id);
            Assert.NotNull(result);
            Assert.Equal(squad.Name, result.Name);
        }

        [Fact]
        public async Task UpdateAsyncShouldUpdateSquad()
        {
            var options = CreateNewContextOptions();
            using var context = new Context(options);
            var repository = new SquadRepository(context);

            var squad = new Squad { Name = "Test Squad", Description = "Test Description" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            squad.Name = "Updated Squad";
            await repository.UpdateAsync(squad);

            var updated = await repository.GetByIdAsync(squad.Id);
            Assert.NotNull(updated);
            Assert.Equal("Updated Squad", updated.Name);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveSquad()
        {
            var options = CreateNewContextOptions();
            using var context = new Context(options);
            var repository = new SquadRepository(context);

            var squad = new Squad { Name = "Test Squad", Description = "Test Description" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            await repository.DeleteAsync(squad.Id);

            var deleted = await repository.GetByIdAsync(squad.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnTrueIfExists()
        {
            var options = CreateNewContextOptions();
            using var context = new Context(options);
            var repository = new SquadRepository(context);

            var squad = new Squad { Name = "Test Squad", Description = "Test Description" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            var exists = await repository.VerifyNameAlreadyExistsAsync("Test Squad");
            Assert.True(exists);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnFalseIfNotExists()
        {
            var options = CreateNewContextOptions();
            using var context = new Context(options);
            var repository = new SquadRepository(context);

            var exists = await repository.VerifyNameAlreadyExistsAsync("Nonexistent Squad");
            Assert.False(exists);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnFilteredSquads()
        {
            var options = CreateNewContextOptions();
            using var context = new Context(options);
            var repository = new SquadRepository(context);

            context.Squads.AddRange(
                new Squad { Name = "Squad A", Description = "Description A" },
                new Squad { Name = "Squad B", Description = "Description B" }
            );
            await context.SaveChangesAsync();

            var filter = new SquadFilter { Name = "Squad A" };
            var result = await repository.GetListAsync(filter);

            Assert.Single(result.Result);
            Assert.Equal("Squad A", result.Result.First().Name);
        }

        [Fact]
        public async Task VerifySquadExistsAsyncShouldReturnTrueIfExists()
        {
            var options = CreateNewContextOptions();
            using var context = new Context(options);
            var repository = new SquadRepository(context);

            var squad = new Squad { Name = "Test Squad", Description = "Test Description" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            var exists = await repository.VerifySquadExistsAsync(squad.Id);
            Assert.True(exists);
        }

        [Fact]
        public async Task VerifySquadExistsAsyncShouldReturnFalseIfNotExists()
        {
            var options = CreateNewContextOptions();
            using var context = new Context(options);
            var repository = new SquadRepository(context);

            var exists = await repository.VerifySquadExistsAsync(999);
            Assert.False(exists);
        }
    }
}
