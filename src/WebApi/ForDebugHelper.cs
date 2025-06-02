#if DEBUG
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi
{
    internal static class ForDebugHelper
    {
        internal static async Task LoadDataForTestsAsync(string databaseType, WebApplication app)
        {
            if (databaseType == "InMemory")
            {
                // Popula o banco com dados de teste
                using IServiceScope scope = app.Services.CreateScope();
                Context context = scope.ServiceProvider.GetRequiredService<Context>();
                await context.Database.EnsureCreatedAsync().ConfigureAwait(false);

                // Áreas
                context.Areas.AddRange(
                    new Area("AMS") { ManagerId = 1 },
                    new Area("Architecture") { ManagerId = 2 },
                    new Area("Commercial") { ManagerId = 3 },
                    new Area("Engineering") { ManagerId = 4 },
                    new Area("EngenChain") { ManagerId = 5 },
                    new Area("Engeniring") { ManagerId = 6 },
                    new Area("Finance") { ManagerId = 7 },
                    new Area("Human Resources") { ManagerId = 8 },
                    new Area("Integration") { ManagerId = 9 },
                    new Area("Manufacturing") { ManagerId = 10 },
                    new Area("Marketing") { ManagerId = 11 },
                    new Area("Product Development") { ManagerId = 12 },
                    new Area("Quality") { ManagerId = 13 },
                    new Area("Quality control") { ManagerId = 14 },
                    new Area("Sales") { ManagerId = 15 },
                    new Area("Security") { ManagerId = 16 },
                    new Area("Supplier Chain") { ManagerId = 17 }
                );

                // Gerentes
                context.Managers.AddRange(
                    new Manager { Name = "Luciene Miranda", Email = "luciene@stellantis.com" },
                    new Manager { Name = "Alexia Lima", Email = "alexia@stellantis.com" },
                    new Manager { Name = "Welton Duarte", Email = "welton@stellantis.com" },
                    new Manager { Name = "Said Debien", Email = "said@stellantis.com" },
                    new Manager { Name = "Matheus Fernandes", Email = "matheus@stellantis.com" },
                    new Manager { Name = "Jardel Reyes", Email = "jardel@stellantis.com" },
                    new Manager { Name = "Patricia Fernanda", Email = "patricia@stellantis.com" },
                    new Manager { Name = "Vitoria Eshiley", Email = "vitoria@stellantis.com" },
                    new Manager { Name = "Ana Raquel", Email = "anaraquel@stellantis.com" },
                    new Manager { Name = "Cecilia Melgaco", Email = "cecilia@stellantis.com" },
                    new Manager { Name = "Andryel Passos", Email = "andryel@stellantis.com" },
                    new Manager { Name = "Vin Diesel", Email = "toretto@stellantis.com" },
                    new Manager { Name = "Michael Jackson", Email = "michael@stellantis.com" },
                    new Manager { Name = "Elvis Presley", Email = "elvis@stellantis.com" },
                    new Manager { Name = "Ariana Grande", Email = "ariana@stellantis.com" },
                    new Manager { Name = "Lana del Rey", Email = "lanadelrey@stellantis.com" },
                    new Manager { Name = "Marshall Mathers", Email = "Eminem@stellantis.com" }
                );

                // Responsáveis
                context.Responsibles.AddRange(
                    new Responsible { AreaId = 1, Name = "Luciene Miranda", Email = "lu@stellantis.com" },
                    new Responsible { AreaId = 2, Name = "Gabriel Torres", Email = "grabriel@stellantis.com" },
                    new Responsible { AreaId = 3, Name = "Alexandro Mendes", Email = "alexandro@stellantis.com" },
                    new Responsible { AreaId = 4, Name = "Leonardo Souza", Email = "leo@stellantis.com" },
                    new Responsible { AreaId = 5, Name = "Mariana Silva", Email = "mariana@stellantis.com" },
                    new Responsible { AreaId = 6, Name = "Carlos Eduardo", Email = "carlos@stellantis.com" },
                    new Responsible { AreaId = 7, Name = "Fernanda Oliveira", Email = "fernanda@stellantis.com" },
                    new Responsible { AreaId = 8, Name = "Rafael Lima", Email = "rafael@stellantis.com" },
                    new Responsible { AreaId = 9, Name = "Juliana Costa", Email = "juliana@stellantis.com" },
                    new Responsible { AreaId = 10, Name = "Bruno Almeida", Email = "bruno@stellantis.com" },
                    new Responsible { AreaId = 11, Name = "Patrícia Santos", Email = "patricia@stellantis.com" },
                    new Responsible { AreaId = 12, Name = "Thiago Pereira", Email = "thiago@stellantis.com" },
                    new Responsible { AreaId = 13, Name = "Ana Paula", Email = "ana.paula@stellantis.com" },
                    new Responsible { AreaId = 14, Name = "Rodrigo Martins", Email = "rodrigo@stellantis.com" }
                );

                // Applications
                context.Applications.AddRange(
                    new ApplicationData("Portal AMS") { AreaId = 1, ResponsibleId = 1, ProductOwner = "", ConfigurationItem = "" },
                    new ApplicationData("eLog") { AreaId = 2, ResponsibleId = 2, ProductOwner = "", ConfigurationItem = "" },
                    new ApplicationData("Suite PD") { AreaId = 4, ResponsibleId = 4, ProductOwner = "", ConfigurationItem = "" },
                    new ApplicationData("Finance Tracker") { AreaId = 6, ResponsibleId = 6, ProductOwner = "Carlos Silva", ConfigurationItem = "FT-Config" },
                    new ApplicationData("HR Portal") { AreaId = 8, ResponsibleId = 8, ProductOwner = "Fernanda Oliveira", ConfigurationItem = "HRP-Config" },
                    new ApplicationData("Marketing Dashboard") { AreaId = 10, ResponsibleId = 10, ProductOwner = "Bruno Almeida", ConfigurationItem = "MD-Config" },
                    new ApplicationData("Sales CRM") { AreaId = 14, ResponsibleId = 14, ProductOwner = "Rodrigo Martins", ConfigurationItem = "CRM-Config" },
                    new ApplicationData("Quality Control System") { AreaId = 13, ResponsibleId = 13, ProductOwner = "Ana Paula", ConfigurationItem = "QCS-Config" },
                    new ApplicationData("Supplier Chain Manager") { AreaId = 16, ResponsibleId = 5, ProductOwner = "Mariana Silva", ConfigurationItem = "SCM-Config" },
                    new ApplicationData("Engineering Tools") { AreaId = 4, ResponsibleId = 4, ProductOwner = "Leonardo Souza", ConfigurationItem = "ET-Config" },
                    new ApplicationData("Integration Hub") { AreaId = 9, ResponsibleId = 9, ProductOwner = "Juliana Costa", ConfigurationItem = "IH-Config" },
                    new ApplicationData("AMS Portal") { AreaId = 1, ResponsibleId = 1, ProductOwner = "Luciene Miranda", ConfigurationItem = "AMS-Config" },
                    new ApplicationData("Architecture Planner") { AreaId = 2, ResponsibleId = 2, ProductOwner = "Gabriel Torres", ConfigurationItem = "AP-Config" }
                );

                // Services
                context.Services.AddRange(
                    new ServiceData { Name = "WebApi", ApplicationId = 1 },
                    new ServiceData { Name = "Integration Services", ApplicationId = 2 },
                    new ServiceData { Name = "Conector", ApplicationId = 2 },
                    new ServiceData { Name = "WebApi", ApplicationId = 3 },
                    new ServiceData { Name = "Finance API", ApplicationId = 4 },
                    new ServiceData { Name = "HR Management Service", ApplicationId = 5 },
                    new ServiceData { Name = "Marketing Analytics", ApplicationId = 6 },
                    new ServiceData { Name = "Sales Tracker", ApplicationId = 7 },
                    new ServiceData { Name = "Quality Assurance Tool", ApplicationId = 8 },
                    new ServiceData { Name = "Supplier Chain Integration", ApplicationId = 9 },
                    new ServiceData { Name = "Engineering Workflow", ApplicationId = 10 },
                    new ServiceData { Name = "AMS Dashboard", ApplicationId = 7 },
                    new ServiceData { Name = "Architecture Planner API", ApplicationId = 8 },
                    new ServiceData { Name = "Integration Hub Service", ApplicationId = 9 }
                );

                context.Repositories.AddRange(
                    new GitRepo("elog") { ApplicationId = 2, Name = "eLog", Description = "Site da plataforma eLog.", Url = new Uri("https://gitlab.fcalatam.com/fca/supply-chain/ELOG") },
                    new GitRepo("Cities's Web API") { ApplicationId = 2, Name = "Cities's Web API", Description = "Web API que retorna os dados das cidades.", Url = new Uri("https://gitlab.fcalatam.com/fca/supply-chain/e-log/city-api") },
                    new GitRepo("finance-tracker") { ApplicationId = 4, Name = "Finance Tracker", Description = "Repositório do sistema de rastreamento financeiro.", Url = new Uri("https://gitlab.fcalatam.com/fca/finance/finance-tracker") },
                    new GitRepo("hr-portal") { ApplicationId = 5, Name = "HR Portal", Description = "Portal de gerenciamento de recursos humanos.", Url = new Uri("https://gitlab.fcalatam.com/fca/hr/hr-portal") },
                    new GitRepo("marketing-dashboard") { ApplicationId = 6, Name = "Marketing Dashboard", Description = "Painel de análise de marketing.", Url = new Uri("https://gitlab.fcalatam.com/fca/marketing/marketing-dashboard") },
                    new GitRepo("sales-crm") { ApplicationId = 7, Name = "Sales CRM", Description = "Sistema de gerenciamento de relacionamento com clientes.", Url = new Uri("https://gitlab.fcalatam.com/fca/sales/sales-crm") },
                    new GitRepo("quality-control-system") { ApplicationId = 8, Name = "Quality Control System", Description = "Ferramenta de controle de qualidade.", Url = new Uri("https://gitlab.fcalatam.com/fca/quality/quality-control-system") },
                    new GitRepo("supplier-chain-manager") { ApplicationId = 9, Name = "Supplier Chain Manager", Description = "Gerenciador da cadeia de fornecedores.", Url = new Uri("https://gitlab.fcalatam.com/fca/supply-chain/supplier-chain-manager") },
                    new GitRepo("engineering-tools") { ApplicationId = 10, Name = "Engineering Tools", Description = "Ferramentas para suporte à engenharia.", Url = new Uri("https://gitlab.fcalatam.com/fca/engineering/engineering-tools") },
                    new GitRepo("ams-portal") { ApplicationId = 11, Name = "AMS Portal", Description = "Portal de gerenciamento AMS.", Url = new Uri("https://gitlab.fcalatam.com/fca/ams/ams-portal") },
                    new GitRepo("architecture-planner") { ApplicationId = 12, Name = "Architecture Planner", Description = "Planejador de arquitetura.", Url = new Uri("https://gitlab.fcalatam.com/fca/architecture/architecture-planner") },
                    new GitRepo("integration-hub") { ApplicationId = 13, Name = "Integration Hub", Description = "Hub de integração para serviços.", Url = new Uri("https://gitlab.fcalatam.com/fca/integration/integration-hub") }
                );

                // Squads
                context.Squads.AddRange(
                    new Squad { Name = "All Java", Description = "Reposnável pelos projetos em Java." },
                    new Squad { Name = "Barramento", Description = "Reposnável pelas integrações." },
                    new Squad { Name = "Elite Debuggers", Description = "Jovens Aprendizes." },
                    new Squad { Name = "Frontend Masters", Description = "Especialistas em desenvolvimento frontend." },
                    new Squad { Name = "Backend Builders", Description = "Focados em soluções backend robustas." },
                    new Squad { Name = "Data Wizards", Description = "Responsáveis por análise e manipulação de dados." },
                    new Squad { Name = "Cloud Ninjas", Description = "Especialistas em soluções baseadas na nuvem." },
                    new Squad { Name = "DevOps Gurus", Description = "Squad dedicado à automação e infraestrutura." },
                    new Squad { Name = "Security Experts", Description = "Focados em segurança e proteção de dados." },
                    new Squad { Name = "Mobile Mavericks", Description = "Especialistas em desenvolvimento mobile." },
                    new Squad { Name = "AI Innovators", Description = "Trabalham com inteligência artificial e aprendizado de máquina." },
                    new Squad { Name = "UX Designers", Description = "Focados em design e experiência do usuário." },
                    new Squad { Name = "Full Stack Heroes", Description = "Squad versátil com habilidades em frontend e backend." }
                );

                // Membros
                context.Members.AddRange(
                    new Member { Name = "Matheus", Email = "matheus@stellantis.com", Role = "Developer", Cost = 1000 },
                    new Member { Name = "Patricia", Email = "patricia@stellantis.com", Role = "Developer", Cost = 1000 },
                    new Member { Name = "Jardel", Email = "jardel@stellantis.com", Role = "Developer", Cost = 1000 },
                    new Member { Name = "Vitória", Email = "vitoria@stellantis.com", Role = "Developer", Cost = 1000 },
                    new Member { Name = "Andryel", Email = "andryel@stellantis.com", Role = "Developer", Cost = 1000 },
                    new Member { Name = "Ana", Email = "ana@stellantis.com", Role = "Developer", Cost = 1000 },
                    new Member { Name = "Cecília", Email = "cecilia@stellantis.com", Role = "Developer", Cost = 1000 },
                    new Member { Name = "João Silva", Email = "joao.silva@stellantis.com", Role = "Developer", Cost = 1200 },
                    new Member { Name = "Maria Oliveira", Email = "maria.oliveira@stellantis.com", Role = "Tester", Cost = 1100 },
                    new Member { Name = "Pedro Santos", Email = "pedro.santos@stellantis.com", Role = "Scrum Master", Cost = 1500 },
                    new Member { Name = "Ana Costa", Email = "ana.costa@stellantis.com", Role = "Product Owner", Cost = 1600 },
                    new Member { Name = "Lucas Almeida", Email = "lucas.almeida@stellantis.com", Role = "Developer", Cost = 1300 },
                    new Member { Name = "Carla Mendes", Email = "carla.mendes@stellantis.com", Role = "UX Designer", Cost = 1400 },
                    new Member { Name = "Rafael Lima", Email = "rafael.lima@stellantis.com", Role = "DevOps Engineer", Cost = 1500 },
                    new Member { Name = "Fernanda Souza", Email = "fernanda.souza@stellantis.com", Role = "Tester", Cost = 1100 },
                    new Member { Name = "Bruno Rocha", Email = "bruno.rocha@stellantis.com", Role = "Developer", Cost = 1200 },
                    new Member { Name = "Juliana Martins", Email = "juliana.martins@stellantis.com", Role = "Business Analyst", Cost = 1400 }
                );

                // Incidents
                context.Incidents.AddRange(
                    new Incident
                    {
                        Title = "Erro no Portal AMS",
                        Description = "Erro ao acessar o portal AMS.",
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        Status = IncidentStatus.Open,
                        ApplicationId = 1
                    },
                    new Incident
                    {
                        Title = "Falha no eLog",
                        Description = "O sistema eLog está fora do ar.",
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        Status = IncidentStatus.InProgress,
                        ApplicationId = 2
                    },
                    new Incident
                    {
                        Title = "Problema no Finance Tracker",
                        Description = "Erro ao gerar relatórios financeiros.",
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        ClosedAt = DateTime.UtcNow.AddDays(-1),
                        Status = IncidentStatus.Closed,
                        ApplicationId = 4
                    }
                );
                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
#endif