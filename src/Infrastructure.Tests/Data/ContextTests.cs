using Microsoft.EntityFrameworkCore;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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
        [SuppressMessage("Blocker Code Smell", "S2699:Tests should include assertions", Justification = "It's a temporary code.")]
        public void Context_ShouldHaveDbSetProperties()
        {
            // Act
            using var context = new Context(_options);
        }

        [Fact]
        public void OnModelCreating_ShouldConfigureModel()
        {
            // Arrange
            using var context = new Context(_options);
            var modelBuilder = new ModelBuilder(new Microsoft.EntityFrameworkCore.Metadata.Conventions.ConventionSet());

            // Act
            var method = context.GetType()
                .GetMethod("OnModelCreating", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(context, [modelBuilder]);
        }

        [Fact]
        public void OnConfiguring_ShouldEnableSensitiveDataLogging()
        {
            // Arrange
            var optionsBuilder = new DbContextOptionsBuilder<Context>();

            // Act
            using var context = new Context(_options);
            var method = context.GetType()
                .GetMethod("OnConfiguring", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(context, [optionsBuilder]);

            // Assert
            Assert.Contains(optionsBuilder.Options.Extensions, e => e.GetType().Name == "CoreOptionsExtension" && ((dynamic)e).IsSensitiveDataLoggingEnabled);
        }
    }
}
