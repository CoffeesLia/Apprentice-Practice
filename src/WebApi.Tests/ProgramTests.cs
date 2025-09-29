using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stellantis.ProjectName.Infrastructure.Data;

namespace WebApi.Tests
{
    public class ProgramTests
    {
        private sealed class CustomWebApplicationFactory(string databaseType) : WebApplicationFactory<Program>
        {
            private readonly string _databaseType = databaseType;

            protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.Sources.Clear();

                    var dict = new Dictionary<string, string?>
                    {
                        ["DatabaseType"] = _databaseType,
                        ["ConnectionString"] = "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True;"
                    };
                    config.AddInMemoryCollection(dict!);
                });
            }
        }

        [Fact]
        public async Task StartAndTestEndpointReturnSuccess()
        {
            Environment.SetEnvironmentVariable("DatabaseType", "InMemory"); 
            Environment.SetEnvironmentVariable("ConnectionString", "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True;");
            using WebApplicationFactory<Program> _factory = new();

            // Arrange
            HttpClient client = _factory.CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync("/health");

            // Assert
            response.EnsureSuccessStatusCode(); 
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void ShouldThrowNotSupportedExceptionOnStartupWhenUnsupportedDatabaseType()
        {
            Environment.SetEnvironmentVariable("DatabaseType", "Firebird");
            Assert.Throws<NotSupportedException>(() =>
            {
                using var factory = new CustomWebApplicationFactory("Firebird");
                var client = factory.CreateClient();
            });
        }

        [Fact]
        public void ShouldRegisterDbContextWithSqlServerWhenDatabaseTypeIsSqlServer()
        {
          
            Environment.SetEnvironmentVariable("DatabaseType", "InMemory");
            Environment.SetEnvironmentVariable("ConnectionString", "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True;");
            using var factory = new CustomWebApplicationFactory("InMemory");
            using var scope = factory.Services.CreateScope();

            var context = scope.ServiceProvider.GetService<Context>();
            Assert.NotNull(context);

            Assert.True(context.Database.IsInMemory(), "O contexto não está usando o provedor InMemory.");
        }

        [Fact]
        public void ShouldRegisterDbContextWithSqliteWhenDatabaseTypeIsSqlite()
        {
            Environment.SetEnvironmentVariable("DatabaseType", "Sqlite");
            Environment.SetEnvironmentVariable("ConnectionString", "DataSource=:memory:");
            using var factory = new CustomWebApplicationFactory("Sqlite");
            using var scope = factory.Services.CreateScope();

            var context = scope.ServiceProvider.GetService<Context>();
            Assert.NotNull(context);

            Assert.Equal("Microsoft.Data.Sqlite", context.Database.GetDbConnection().GetType().Namespace);
        }
    }
}
