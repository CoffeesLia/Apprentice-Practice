using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
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
            using Context context = new(_dbContextOptions);
            SquadRepository repository = new(context);

            Squad squad = new() { Name = "Test Squad", Description = "Test Description" };
            await repository.CreateAsync(squad);

            Squad? createdSquad = await context.Squads.FirstOrDefaultAsync(s => s.Name == "Test Squad");
            Assert.NotNull(createdSquad);
        }

        [Fact]
        public async Task GetByIdAsyncShouldReturnSquad()
        {
            using Context context = new(_dbContextOptions);
            SquadRepository repository = new(context);

            Squad squad = new() { Name = "Test Squad", Description = "Test Description" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            Squad? retrievedSquad = await repository.GetByIdAsync(squad.Id);
            Assert.NotNull(retrievedSquad);
            Assert.Equal(squad.Name, retrievedSquad?.Name);
        }

        [Fact]
        public async Task UpdateAsyncShouldUpdateSquad()
        {
            using Context context = new(_dbContextOptions);
            SquadRepository repository = new(context);

            Squad squad = new() { Name = "Test Squad", Description = "Test Description" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            // Desanexar a entidade para simular uma nova leitura do banco de dados
            context.Entry(squad).State = EntityState.Detached;

            // Atualizar a entidade
            squad.Name = "Updated Squad";
            await repository.UpdateAsync(squad);
            await context.SaveChangesAsync();

            // Desanexar a entidade para simular uma nova leitura do banco de dados
            context.Entry(squad).State = EntityState.Detached;

            Squad? updatedSquad = await repository.GetByIdAsync(squad.Id); // Use repository to get the updated entity
            Assert.NotNull(updatedSquad);
            Assert.Equal("Updated Squad", updatedSquad?.Name);
        }

        [Fact]
        public async Task VerifyNameAlreadyExistsAsyncShouldReturnTrueIfNameExists()
        {
            using Context context = new(_dbContextOptions);
            SquadRepository repository = new(context);

            Squad squad = new() { Name = "Test Squad", Description = "Test Description" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            bool exists = await repository.VerifyNameAlreadyExistsAsync("Test Squad");
            Assert.True(exists);
        }

        [Fact]
        public async Task GetListAsyncShouldReturnFilteredSquads()
        {
            using Context context = new(_dbContextOptions);
            SquadRepository repository = new(context);

            context.Squads.AddRange(
                new Squad { Name = "Squad A", Description = "Description A" },
                new Squad { Name = "Squad B", Description = "Description B" }
            );
            await context.SaveChangesAsync();

            SquadFilter filter = new() { Name = "Squad A" };
            PagedResult<Squad> result = await repository.GetListAsync(filter);

            Assert.Single(result.Result);
            Assert.Equal("Squad A", result.Result.First().Name);
        }

        [Fact]
        public async Task VerifySquadExistsAsyncShouldReturnTrueIfSquadExists()
        {
            using Context context = new(_dbContextOptions);
            SquadRepository repository = new(context);

            Squad squad = new() { Name = "Test Squad", Description = "Test Description" };
            context.Squads.Add(squad);
            await context.SaveChangesAsync();

            bool exists = await repository.VerifySquadExistsAsync(squad.Id);
            Assert.True(exists);
        }

        [Fact]
        public async Task DeleteAsyncShouldRemoveSquad()
        {
            using Context context = new(_dbContextOptions);
            SquadRepository repository = new(context);

            Squad squad = new() { Name = "Test Squad", Description = "Test Description" };
            await repository.CreateAsync(squad);
            await context.SaveChangesAsync();

            Squad? addedSquad = await repository.GetByIdAsync(squad.Id);
            Assert.NotNull(addedSquad);

            await repository.DeleteAsync(squad.Id);
            await context.SaveChangesAsync();

            Squad? deletedSquad = await repository.GetByIdAsync(squad.Id);
            Assert.Null(deletedSquad);

            bool exists = await context.Squads.AnyAsync(s => s.Id == squad.Id);
            Assert.False(exists);
        }
    }
}
