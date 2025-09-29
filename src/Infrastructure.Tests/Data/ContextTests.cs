using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
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
        public void ContextShouldCreateDatabase()
        {
            // Act
            using Context context = new(_options);

            // Assert
            Assert.True(context.Database.EnsureCreated());
        }

        [Fact]
        public void ContextShouldHaveDbSetProperties()
        {
            // Act
            using Context context = new(_options);
        }

        [Fact]
        public void OnModelCreatingShouldConfigureModel()
        {
            // Arrange
            using Context context = new(_options);
            ModelBuilder modelBuilder = new(new Microsoft.EntityFrameworkCore.Metadata.Conventions.ConventionSet());

            // Act
            MethodInfo? method = context.GetType()
                .GetMethod("OnModelCreating", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(context, [modelBuilder]);
        }

        [Fact]
        public void OnConfiguringShouldEnableSensitiveDataLogging()
        {
            // Arrange
            DbContextOptionsBuilder<Context> optionsBuilder = new();

            // Act
            using Context context = new(_options);
            MethodInfo? method = context.GetType()
                .GetMethod("OnConfiguring", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(context, [optionsBuilder]);

            // Assert
            Assert.Contains(optionsBuilder.Options.Extensions, e => e.GetType().Name == "CoreOptionsExtension" && ((dynamic)e).IsSensitiveDataLoggingEnabled);
        }
    }
}
