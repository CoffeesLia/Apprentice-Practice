#if DEBUG
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;

namespace Stellantis.ProjectName.WebApi
{
    internal static class ForDebugHelper
    {
        internal static async Task LoadDataForTestsAsync(string databaseType, WebApplication app)
        {
            if (databaseType == "InMemory")
            {
                // Popula o banco com dados de teste
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<Context>();
                    await context.Database.EnsureCreatedAsync().ConfigureAwait(false);

                    // Áreas
                    context.Areas.AddRange(
                        new Area("AMS"),
                        new Area("Supplier Chain"),
                        new Area("Quality"),
                        new Area("Engeniring")
                    );

                    // Responsáveis
                    context.Responsibles.AddRange(
                        new Responsible { AreaId = 1, Name = "Luciene Miranda", Email = "lu@stellantis.com" },
                        new Responsible { AreaId = 2, Name = "Gabriel Torres", Email = "grabriel@stellantis.com" },
                        new Responsible { AreaId = 3, Name = "Alexandro Mendes", Email = "alexandro@stellantis.com" },
                        new Responsible { AreaId = 4, Name = "Leonardo Souza", Email = "leo@stellantis.com" }
                    );

                    // Applications
                    var app1 = new ApplicationData("Portal AMS") { AreaId = 1, ResponsibleId = 1, ProductOwner = "", ConfigurationItem = "" };
                    var app2 = new ApplicationData("eLog") { AreaId = 2, ResponsibleId = 2, ProductOwner = "", ConfigurationItem = "" };
                    var app3 = new ApplicationData("Suite PD") { AreaId = 4, ResponsibleId = 4, ProductOwner = "", ConfigurationItem = "" };
                    context.Applications.AddRange(
                        app1, app2, app3
                    );

                    // Services
                    context.Services.AddRange(
                        new ServiceData { Name = "WebApi", ApplicationId = 1 },
                        new ServiceData { Name = "Integration Services", ApplicationId = 2 },
                        new ServiceData { Name = "Conector", ApplicationId = 2 },
                        new ServiceData { Name = "WebApi", ApplicationId = 3 }
                    );
#pragma warning disable S1075 // URIs should not be hardcoded
                    context.Repositories.AddRange(
                        new GitRepo("elog") { ApplicationId = 2, Name = "eLog", Description = "Site da plataforma eLog.", Url = new Uri("https://gitlab.fcalatam.com/fca/supply-chain/ELOG") },
                        new GitRepo("Cities's Web API") { ApplicationId = 2, Name = "Cities's Web API", Description = "Web API que retorna os dados das cidades.", Url = new Uri("https://gitlab.fcalatam.com/fca/supply-chain/e-log/city-api") }
                    );
#pragma warning restore S1075 // URIs should not be hardcoded

                    // Squads
                    context.Squads.AddRange(
                        new Squad { Name = "All Java", Description = "Reposnável pelos projetos em Java." },
                        new Squad { Name = "Barramento", Description = "Reposnável pelas integrações." },
                        new Squad { Name = "Elite Debuggers", Description = "Jovens Aprendizes." }
                    );

                    // Membros
                    context.Members.AddRange(
                        new Member { Name = "Matheus", Email = "matheus@stellantis.com", Role = "Developer", Cost = 1000 },
                        new Member { Name = "Patricia", Email = "patricia@stellantis.com", Role = "Developer", Cost = 1000 },
                        new Member { Name = "Jardel", Email = "jardel@stellantis.com", Role = "Developer", Cost = 1000 },
                        new Member { Name = "Vitória", Email = "vitoria@stellantis.com", Role = "Developer", Cost = 1000 },
                        new Member { Name = "Andryel", Email = "andryel@stellantis.com", Role = "Developer", Cost = 1000 },
                        new Member { Name = "Ana", Email = "ana@stellantis.com", Role = "Developer", Cost = 1000 },
                        new Member { Name = "Cecília", Email = "cecilia@stellantis.com", Role = "Developer", Cost = 1000 }
                    );

                    await context.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
#endif