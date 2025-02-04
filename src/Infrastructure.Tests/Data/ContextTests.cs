using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Infrastructure.Tests.Data
{
    public class ContextTests
    {
        private readonly DbContextOptions<Context> _options;

        public ContextTests()
        {
            _options = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: "ContextTests")
                .Options;
        }

        [Fact]
        public void Context_ShouldCreateDatabase()
        {
            // Act
            using var context = new Context(_options);

            // Assert
            Assert.True(context.Database.EnsureCreated());
        }

        [Fact]
        public void Context_ShouldHaveDbSetProperties()
        {
            // Act
            using var context = new Context(_options);

            // Assert
            Assert.NotNull(context.PartNumbers);
            Assert.NotNull(context.Suppliers);
            Assert.NotNull(context.Vehicles);
            Assert.NotNull(context.PartNumberSuppliers);
            Assert.NotNull(context.VehiclePartNumbers);
        }

        [Fact]
        public void OnModelCreating_ShouldConfigureModel()
        {
            // Arrange
            using var context = new Context(_options);
            var modelBuilder = new ModelBuilder(new Microsoft.EntityFrameworkCore.Metadata.Conventions.ConventionSet());

            // Act
            context.GetType().GetMethod("OnModelCreating", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(context, [modelBuilder]);

            // Assert
            Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(PartNumber)));
            Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(Supplier)));
            Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(Vehicle)));
            Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(PartNumberSupplier)));
            Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(VehiclePartNumber)));
        }

        [Fact]
        public void OnConfiguring_ShouldEnableSensitiveDataLogging()
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder<Context>();

            // Act
            using var context = new Context(_options);
            context.GetType().GetMethod("OnConfiguring", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(context, [optionsBuilder]);

            // Assert
            Assert.True(optionsBuilder.Options.Extensions.Any(e => e.GetType().Name == "CoreOptionsExtension" && ((dynamic)e).IsSensitiveDataLoggingEnabled));
        }
    }
}
