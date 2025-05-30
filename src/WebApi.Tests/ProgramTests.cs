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
        private class CustomWebApplicationFactory : WebApplicationFactory<Program>
        {
            private readonly string _databaseType;
            public CustomWebApplicationFactory(string databaseType)
            {
                _databaseType = databaseType;
            }

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
            Environment.SetEnvironmentVariable("DatabaseType", "SqlServer");
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
            Environment.SetEnvironmentVariable("DatabaseType", "SqlServer");
            Environment.SetEnvironmentVariable("ConnectionString", "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True;");
            using var factory = new CustomWebApplicationFactory("SqlServer");
            using var scope = factory.Services.CreateScope();

            var context = scope.ServiceProvider.GetService<Context>();
            Assert.NotNull(context);

            var connectionString = context.Database.GetDbConnection().ConnectionString;
            Assert.Contains("TestDb", connectionString, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("Microsoft.Data.SqlClient", context.Database.GetDbConnection().GetType().Namespace);
        }

        [Fact]
        public void ShouldRegisterDbContextWithInMemoryWhenDatabaseTypeIsInMemory()
        {
            Environment.SetEnvironmentVariable("DatabaseType", "InMemory");
            using var factory = new CustomWebApplicationFactory("InMemory");
            using var scope = factory.Services.CreateScope();

            var context = scope.ServiceProvider.GetService<Context>();
            Assert.NotNull(context);

            Assert.True(context.Database.IsInMemory(), "O contexto não está usando o provedor InMemory.");
        }
    }
}
