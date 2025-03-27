using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Domain.Entity;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Infrastructure.Tests.Data.Repositories
{
    public class SquadRepositoryTests
    {
        private readonly DbContextOptions<Context> _dbContextOptions;

        public SquadRepositoryTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "SquadDatabase")
                .Options;
        }

        [Fact]
        public void AddShouldAddSquad()
        {
            using (var context = new Context(_dbContextOptions))
            {
                var repository = new SquadRepository(context);
                var squad = new EntitySquad { Id = Guid.NewGuid(), Name = "TestSquad", Description = "TestDescription" };

                repository.Add(squad);
                context.SaveChanges();

                var addedSquad = context.Squads.Find(squad.Id);
                Assert.NotNull(addedSquad);
                Assert.Equal("TestSquad", addedSquad.Name);
            }
        }

        [Fact]
        public void GetByIdShouldReturnSquadWhenSquadExists()
        {
            using (var context = new Context(_dbContextOptions))
            {
                var squadId = Guid.NewGuid();
                var squad = new EntitySquad { Id = squadId, Name = "TestSquad", Description = "TestDescription" };
                context.Squads.Add(squad);
                context.SaveChanges();

                var repository = new SquadRepository(context);
                var retrievedSquad = repository.GetById(squadId);

                Assert.NotNull(retrievedSquad);
                Assert.Equal("TestSquad", retrievedSquad.Name);
            }
        }

        [Fact]
        public void GetByIdShouldReturnNullWhenSquadDoesNotExist()
        {
            using (var context = new Context(_dbContextOptions))
            {
                var repository = new SquadRepository(context);
                var retrievedSquad = repository.GetById(Guid.NewGuid());

                Assert.Null(retrievedSquad);
            }
        }

        [Fact]
        public void GetByNameShouldReturnSquadWhenSquadExists()
        {
            using (var context = new Context(_dbContextOptions))
            {
                var squad = new EntitySquad { Id = Guid.NewGuid(), Name = "TestSquad", Description = "TestDescription" };
                context.Squads.Add(squad);
                context.SaveChanges();

                var repository = new SquadRepository(context);
                var retrievedSquad = repository.GetByName("TestSquad");

                Assert.NotNull(retrievedSquad);
                Assert.Equal("TestSquad", retrievedSquad.Name);
            }
        }

        [Fact]
        public void GetByNameShouldReturnNullWhenSquadDoesNotExist()
        {
            using (var context = new Context(_dbContextOptions))
            {
                var repository = new SquadRepository(context);
                var retrievedSquad = repository.GetByName("NonExistentSquad");

                Assert.Null(retrievedSquad);
            }
        }
        [Fact]
        public void GetAllShouldReturnAllSquads()
        {
            // Recria o banco de dados em memória para garantir um estado limpo
            var options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new Context(options))
            {
                // Arrange
                var squads = new List<EntitySquad>
        {
            new EntitySquad { Id = Guid.NewGuid(), Name = "Squad1", Description = "Description1" },
            new EntitySquad { Id = Guid.NewGuid(), Name = "Squad2", Description = "Description2" }
        };
                context.Squads.AddRange(squads);
                context.SaveChanges();

                var repository = new SquadRepository(context);

                // Act
                var retrievedSquads = repository.GetAll();

                // Assert
                Assert.NotNull(retrievedSquads);
                Assert.Equal(2, retrievedSquads.Count());
                Assert.Contains(retrievedSquads, s => s.Name == "Squad1");
                Assert.Contains(retrievedSquads, s => s.Name == "Squad2");
            }
        }


        [Fact]
        public void UpdateShouldUpdateSquad()
        {
            using (var context = new Context(_dbContextOptions))
            {
                var squad = new EntitySquad { Id = Guid.NewGuid(), Name = "TestSquad", Description = "TestDescription" };
                context.Squads.Add(squad);
                context.SaveChanges();

                var repository = new SquadRepository(context);
                squad.Name = "UpdatedSquad";
                repository.Update(squad);
                context.SaveChanges();

                var updatedSquad = context.Squads.Find(squad.Id);
                Assert.NotNull(updatedSquad);
                Assert.Equal("UpdatedSquad", updatedSquad.Name);
            }
        }

        [Fact]
        public void DeleteShouldRemoveSquad()
        {
            using (var context = new Context(_dbContextOptions))
            {
                var squad = new EntitySquad { Id = Guid.NewGuid(), Name = "TestSquad", Description = "TestDescription" };
                context.Squads.Add(squad);
                context.SaveChanges();

                var repository = new SquadRepository(context);
                repository.Delete(squad);
                context.SaveChanges();

                var deletedSquad = context.Squads.Find(squad.Id);
                Assert.Null(deletedSquad);
            }
        }
    }
}
