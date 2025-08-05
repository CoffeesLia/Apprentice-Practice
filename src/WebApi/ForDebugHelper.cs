#if DEBUG
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.WebApi.Hubs;

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


                // Mensagens de chat
                context.ChatMessages.AddRange(
                    new Chat
                    {
                        Id = Guid.NewGuid(),
                        User = "Matheus",
                        Message = "Bom dia, Elite Debuggers!",
                        SentAt = DateTime.UtcNow.AddMinutes(-10)
                    },
                    new Chat
                    {
                        Id = Guid.NewGuid(),
                        User = "Matheus",
                        Message = "O Squad está quebrando a develop!",
                        SentAt = DateTime.UtcNow.AddMinutes(-8)
                    },
                    new Chat
                    {
                        Id = Guid.NewGuid(),
                        User = "Jardel",
                        Message = "Oh my God!",
                        SentAt = DateTime.UtcNow.AddMinutes(-8)
                    },
                    new Chat
                    {
                        Id = Guid.NewGuid(),
                        User = "Matheus",
                        Message = "API: https://gitlab.fcalatam.com/fca/ams/portal/api",
                        SentAt = DateTime.UtcNow.AddMinutes(-8)
                    },
                    new Chat
                    {
                        Id = Guid.NewGuid(),
                        User = "Matheus",
                        Message = "WEB: https://gitlab.fcalatam.com/fca/ams/portal/web",
                        SentAt = DateTime.UtcNow.AddMinutes(-8)
                    },
                    new Chat
                    {
                        Id = Guid.NewGuid(),
                        User = "Welton",
                        Message = "Amo vocês, meus Young Talents!",
                        SentAt = DateTime.UtcNow.AddMinutes(-10)
                    }
                );

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
                    new Manager { Name = "Andryel Passos", Email = "andryel@stellantis.com" },
                    new Manager { Name = "Patricia Fernanda", Email = "patricia@stellantis.com" },
                    new Manager { Name = "Vitoria Eshiley", Email = "vitoria@stellantis.com" },
                    new Manager { Name = "Ana Raquel", Email = "anaraquel@stellantis.com" },
                    new Manager { Name = "Cecilia Melgaco", Email = "cecilia@stellantis.com" },
                    new Manager { Name = "Vin Diesel", Email = "toretto@stellantis.com" },
                    new Manager { Name = "Michael Jackson", Email = "rusbe@stellantis.com" },
                    new Manager { Name = "Keanu Reeves", Email = "johnwick@stellantis.com" },
                    new Manager { Name = "Abel Tesfaye", Email = "theweeknd@stellantis.com" },
                    new Manager { Name = "Lana del Rey", Email = "lanadelrey@stellantis.com" },
                    new Manager { Name = "Marshall Mathers", Email = "eminem@stellantis.com" }
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
                    new ApplicationData("Portal AMS") { AreaId = 7, ResponsibleId = 1, ConfigurationItem = "", SquadId = 1 },
                    new ApplicationData("eLog") { AreaId = 2, ResponsibleId = 2, ConfigurationItem = "", SquadId = 2 },
                    new ApplicationData("Suite PD") { AreaId = 4, ResponsibleId = 4, ConfigurationItem = "", SquadId = 3 },
                    new ApplicationData("Finance Tracker") { AreaId = 6, ResponsibleId = 6, ConfigurationItem = "FT-Config", SquadId = 4 },
                    new ApplicationData("HR Portal") { AreaId = 8, ResponsibleId = 8, ConfigurationItem = "HRP-Config" },
                    new ApplicationData("Marketing Dashboard") { AreaId = 10, ResponsibleId = 10, ConfigurationItem = "MD-Config" },
                    new ApplicationData("Sales CRM") { AreaId = 14, ResponsibleId = 14, ConfigurationItem = "CRM-Config" },
                    new ApplicationData("Quality Control System") { AreaId = 1, ResponsibleId = 13, ConfigurationItem = "QCS-Config" },
                    new ApplicationData("Supplier Chain Manager") { AreaId = 16, ResponsibleId = 5, ConfigurationItem = "SCM-Config" },
                    new ApplicationData("Engineering Tools") { AreaId = 4, ResponsibleId = 4, ConfigurationItem = "ET-Config" },
                    new ApplicationData("Integration Hub") { AreaId = 9, ResponsibleId = 9, ConfigurationItem = "IH-Config" },
                    new ApplicationData("AMS Portal") { AreaId = 1, ResponsibleId = 1, ConfigurationItem = "AMS-Config" },
                    new ApplicationData("Architecture Planner") { AreaId = 2, ResponsibleId = 2, ConfigurationItem = "AP-Config" }
                );

                // Serviços
                context.Services.AddRange(
                    new ServiceData { Name = "WebApi", Description = "API principal para acesso e gerenciamento dos dados do Portal AMS.", ApplicationId = 1 },
                    new ServiceData { Name = "Integration Services", Description = "Serviços responsáveis pela integração entre sistemas internos e externos.", ApplicationId = 2 },
                    new ServiceData { Name = "Conector", Description = "Módulo de conexão para troca de informações entre aplicações.", ApplicationId = 2 },
                    new ServiceData { Name = "WebApi", Description = "API dedicada ao gerenciamento da Suite PD.", ApplicationId = 3 },
                    new ServiceData { Name = "Finance API", Description = "API para operações e relatórios financeiros.", ApplicationId = 4 },
                    new ServiceData { Name = "HR Management Service", Description = "Serviço para gerenciamento de recursos humanos.", ApplicationId = 5 },
                    new ServiceData { Name = "Marketing Analytics", Description = "Serviço de análise e relatórios de marketing.", ApplicationId = 6 },
                    new ServiceData { Name = "Sales Tracker", Description = "Ferramenta para acompanhamento de vendas.", ApplicationId = 7 },
                    new ServiceData { Name = "Quality Assurance Tool", Description = "Ferramenta para controle e garantia de qualidade.", ApplicationId = 8 },
                    new ServiceData { Name = "Supplier Chain Integration", Description = "Integração de processos da cadeia de fornecedores.", ApplicationId = 9 },
                    new ServiceData { Name = "Engineering Workflow", Description = "Gerenciamento de fluxos de trabalho de engenharia.", ApplicationId = 10 },
                    new ServiceData { Name = "AMS Dashboard", Description = "Painel de controle para visualização de dados do AMS.", ApplicationId = 7 },
                    new ServiceData { Name = "Architecture Planner API", Description = "API para planejamento e gestão de arquitetura.", ApplicationId = 8 },
                    new ServiceData { Name = "Integration Hub Service", Description = "Central de integração para orquestração de serviços.", ApplicationId = 9 }
                );

                context.Repositories.AddRange(
                    new Repo { ApplicationId = 2, Name = "eLog", Description = "Site da plataforma eLog.", Url = new Uri("https://gitlab.fcalatam.com/fca/supply-chain/ELOG") },
                    new Repo { ApplicationId = 2, Name = "Cities's Web API", Description = "Web API que retorna os dados das cidades.", Url = new Uri("https://gitlab.fcalatam.com/fca/supply-chain/e-log/city-api") },
                    new Repo { ApplicationId = 4, Name = "Finance Tracker", Description = "Repositório do sistema de rastreamento financeiro.", Url = new Uri("https://gitlab.fcalatam.com/fca/finance/finance-tracker") },
                    new Repo { ApplicationId = 5, Name = "HR Portal", Description = "Portal de gerenciamento de recursos humanos.", Url = new Uri("https://gitlab.fcalatam.com/fca/hr/hr-portal") },
                    new Repo { ApplicationId = 6, Name = "Marketing Dashboard", Description = "Painel de análise de marketing.", Url = new Uri("https://gitlab.fcalatam.com/fca/marketing/marketing-dashboard") },
                    new Repo { ApplicationId = 7, Name = "Sales CRM", Description = "Sistema de gerenciamento de relacionamento com clientes.", Url = new Uri("https://gitlab.fcalatam.com/fca/sales/sales-crm") },
                    new Repo { ApplicationId = 8, Name = "Quality Control System", Description = "Ferramenta de controle de qualidade.", Url = new Uri("https://gitlab.fcalatam.com/fca/quality/quality-control-system") },
                    new Repo { ApplicationId = 9, Name = "Supplier Chain Manager", Description = "Gerenciador da cadeia de fornecedores.", Url = new Uri("https://gitlab.fcalatam.com/fca/supply-chain/supplier-chain-manager") },
                    new Repo { ApplicationId = 10, Name = "Engineering Tools", Description = "Ferramentas para suporte à engenharia.", Url = new Uri("https://gitlab.fcalatam.com/fca/engineering/engineering-tools") },
                    new Repo { ApplicationId = 11, Name = "AMS Portal", Description = "Portal de gerenciamento AMS.", Url = new Uri("https://gitlab.fcalatam.com/fca/ams/ams-portal") },
                    new Repo { ApplicationId = 12, Name = "Architecture Planner", Description = "Planejador de arquitetura.", Url = new Uri("https://gitlab.fcalatam.com/fca/architecture/architecture-planner") },
                    new Repo { ApplicationId = 13, Name = "Integration Hub", Description = "Hub de integração para serviços.", Url = new Uri("https://gitlab.fcalatam.com/fca/integration/integration-hub") }
                );


                // Squads
                context.Squads.AddRange(
                    new Squad { Name = "Elite Debuggers", Description = "Jovens Aprendizes." },
                    new Squad { Name = "All Java", Description = "Reposnável pelos projetos em Java." },
                    new Squad { Name = "Barramento", Description = "Reposnável pelas integrações." },
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

                // Integrações
                context.Integrations.AddRange(
                    new Integration("Tralalero Tralala", "Bombardiro crocodilo") { ApplicationDataId = 1 },
                    new Integration("Finance Sync", "Integração entre sistemas financeiros e ERP") { ApplicationDataId = 4 },
                    new Integration("HR Connector", "Sincronização de dados de RH com sistemas externos") { ApplicationDataId = 5 },
                    new Integration("Salesforce Bridge", "Integração de oportunidades de vendas com Salesforce") { ApplicationDataId = 7 },
                    new Integration("Marketing Data Feed", "Importação de leads de campanhas de marketing") { ApplicationDataId = 6 },
                    new Integration("Supplier API", "Integração com fornecedores externos para atualização de estoque") { ApplicationDataId = 9 },
                    new Integration("Quality Gateway", "Envio de dados de qualidade para sistemas de auditoria") { ApplicationDataId = 8 },
                    new Integration("AMS Notifier", "Notificações automáticas do Portal AMS para usuários") { ApplicationDataId = 1 },
                    new Integration("Engineering Sync", "Integração de ferramentas de engenharia com repositórios") { ApplicationDataId = 10 },
                    new Integration("eLog Importer", "Importação de dados logísticos do sistema eLog") { ApplicationDataId = 2 }
                );

                // Membros
                context.Members.AddRange(
                    new Member { Name = "Matheus", Email = "matheus.silva3@stellantis.com", Role = "Developer", Cost = 1000, SquadId = 1 },
                    new Member { Name = "Patricia", Email = "patricia@stellantis.com", Role = "Developer", Cost = 1000, SquadId = 1 },
                    new Member { Name = "Jardel", Email = "jardel@stellantis.com", Role = "Developer", Cost = 1000, SquadId = 4 },
                    new Member { Name = "Vitória", Email = "vitoria@stellantis.com", Role = "Developer", Cost = 1000, SquadId = 3 },
                    new Member { Name = "Andryel", Email = "andryel@stellantis.com", Role = "Developer", Cost = 1000, SquadId = 5 },
                    new Member { Name = "Ana", Email = "ana@stellantis.com", Role = "Developer", Cost = 1000, SquadId = 6 },
                    new Member { Name = "Cecília", Email = "cecilia@stellantis.com", Role = "Developer", Cost = 1000, SquadId = 2 },
                    new Member { Name = "João Silva", Email = "joao.silva@stellantis.com", Role = "Developer", Cost = 1200, SquadId = 1 },
                    new Member { Name = "Maria Oliveira", Email = "maria.oliveira@stellantis.com", Role = "Tester", Cost = 1100, SquadId = 4 },
                    new Member { Name = "Pedro Santos", Email = "pedro.santos@stellantis.com", Role = "Scrum Master", Cost = 1500, SquadId = 3 }, //10
                    new Member { Name = "Ana Costa", Email = "ana.costa@stellantis.com", Role = "Product Owner", Cost = 1600, SquadId = 5 },
                    new Member { Name = "Lucas Almeida", Email = "lucas.almeida@stellantis.com", Role = "Leader Squad", Cost = 1300, SquadId = 6 },
                    new Member { Name = "Carla Mendes", Email = "carla.mendes@stellantis.com", Role = "UX Designer", Cost = 1400, SquadId = 2 },
                    new Member { Name = "Rafael Lima", Email = "rafael.lima@stellantis.com", Role = "DevOps Engineer", Cost = 1500, SquadId = 1 },
                    new Member { Name = "Fernanda Souza", Email = "fernanda.souza@stellantis.com", Role = "Tester", Cost = 1100, SquadId = 4 },
                    new Member { Name = "Bruno Rocha", Email = "bruno.rocha@stellantis.com", Role = "Developer", Cost = 1200, SquadId = 3 },
                    new Member { Name = "Juliana Martins", Email = "juliana.martins@stellantis.com", Role = "Business Analyst", Cost = 1400, SquadId = 5 }
                );
                await context.SaveChangesAsync().ConfigureAwait(false);

                context.Feedbacks.AddRange(
                     new Feedback
                     {
                         Title = "Melhora no design do Portal AMS",
                         Description = "O design do site está muito simples, talvez modernizar as cores e fontes ajudasse.",
                         CreatedAt = DateTime.UtcNow.AddDays(-7),
                         ApplicationId = 1,
                         Status = FeedbackStatus.Open,
                         Members = [.. context.Members.Where(m => m.SquadId == 1)]
                     },
                     new Feedback
                     {
                         Title = "Dificuldade em encontrar funcionalidades no eLog",
                         Description = "Sinto que algumas funções importantes estão escondidas ou não são intuitivas de achar no menu.",
                         CreatedAt = DateTime.UtcNow.AddDays(-5),
                         ApplicationId = 2, 
                         Status = FeedbackStatus.InProgress,
                         Members = [.. context.Members.Where(m => m.SquadId == 2)]
                     },
                     new Feedback
                     {
                         Title = "Finance Tracker muito rápido, excelente experiência!",
                         Description = "Gostei muito da velocidade de carregamento e da fluidez ao navegar no Finance Tracker. Parabéns!",
                         CreatedAt = DateTime.UtcNow.AddDays(-3),
                         ClosedAt = DateTime.UtcNow.AddDays(-2),
                         ApplicationId = 4, 
                         Status = FeedbackStatus.Closed,
                         Members = [.. context.Members.Where(m => m.SquadId == 4)]
                     },
                     new Feedback
                     {
                         Title = "Sugestão de nova funcionalidade: integração com calendário no HR Portal",
                         Description = "Seria ótimo se pudéssemos integrar com nosso calendário para agendar tarefas diretamente do HR Portal.",
                         CreatedAt = DateTime.UtcNow.AddDays(-2),
                         ApplicationId = 5, 
                         Status = FeedbackStatus.Reopened,

                         Members = [.. context.Members.Where(m => m.SquadId == 5)]
                     },
                     new Feedback
                     {
                         Title = "Conteúdo desatualizado em algumas seções do Marketing Dashboard",
                         Description = "Notei que algumas informações nas seções de 'Sobre Nós' e 'FAQ' no Marketing Dashboard parecem estar desatualizadas.",
                         CreatedAt = DateTime.UtcNow.AddDays(-1),
                         ClosedAt = DateTime.UtcNow,
                         ApplicationId = 6,
                         Status = FeedbackStatus.Cancelled,
                         Members = [.. context.Members.Where(m => m.SquadId == 6)]
                     },
                     new Feedback
                     {
                         Title = "Ótimo suporte ao cliente no Sales CRM",
                         Description = "Tive uma dúvida e o atendimento via chat no Sales CRM foi muito rápido e eficiente. Fiquei impressionado!",
                         CreatedAt = DateTime.UtcNow.AddHours(-12),
                         ClosedAt = DateTime.UtcNow.AddHours(-10),
                         ApplicationId = 7, 
                         Status = FeedbackStatus.Closed,
                         Members = [.. context.Members.Where(m => m.SquadId == 1)]
                     },
                     new Feedback
                     {
                         Title = "Problema de login intermitente no Quality Control System",
                         Description = "Às vezes, o sistema de controle de qualidade apresenta falha ao tentar fazer login, exigindo várias tentativas.",
                         CreatedAt = DateTime.UtcNow.AddDays(-4),
                         ApplicationId = 8, 
                         Status = FeedbackStatus.Open,
                         Members = [.. context.Members.Where(m => m.SquadId == 3)]
                     },
                     new Feedback
                     {
                         Title = "Sugestão de melhoria na interface do Supplier Chain Manager",
                         Description = "A interface poderia ser mais limpa e moderna para facilitar a visualização de grandes volumes de dados.",
                         CreatedAt = DateTime.UtcNow.AddDays(-6),
                         ApplicationId = 9, 
                         Status = FeedbackStatus.InProgress,
                         Members = [.. context.Members.Where(m => m.SquadId == 5)]
                     },
                     new Feedback
                     {
                         Title = "Ferramentas de Engenharia estão ótimas!",
                         Description = "As Engineering Tools estão muito completas e eficientes para o nosso trabalho diário. Excelente!",
                         CreatedAt = DateTime.UtcNow.AddDays(-8),
                         ClosedAt = DateTime.UtcNow.AddDays(-7),
                         ApplicationId = 10,
                         Status = FeedbackStatus.Closed,
                         Members = [.. context.Members.Where(m => m.SquadId == 4)]
                     }
                 );

                // Incidents
                context.Incidents.AddRange(
                    new Incident
                    {
                        Title = "Erro no Portal AMS",
                        Description = "Erro ao acessar o portal AMS.",
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        Status = IncidentStatus.Open,
                        ApplicationId = 1,
                        Members = context.Members.Where(m => m.Id == 1 || m.Id == 2).ToList()
                    },
                    new Incident
                    {
                        Title = "Falha no eLog",
                        Description = "O sistema eLog está fora do ar.",
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        Status = IncidentStatus.InProgress,
                        ApplicationId = 2,
                        Members = context.Members.Where(m => m.Id == 3).ToList()
                    },
                    new Incident
                    {
                        Title = "Problema no Finance Tracker",
                        Description = "Erro ao gerar relatórios financeiros.",
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        ClosedAt = DateTime.UtcNow.AddDays(-1),
                        Status = IncidentStatus.Closed,
                        ApplicationId = 4,
                        Members = context.Members.Where(m => m.Id == 4 || m.Id == 5).ToList()
                    },
                    new Incident
                    {
                        Title = "Chamado reaberto no HR Portal",
                        Description = "Problema voltou a ocorrer no HR Portal.",
                        CreatedAt = DateTime.UtcNow.AddDays(-8),
                        Status = IncidentStatus.Reopened,
                        ApplicationId = 5,
                        Members = context.Members.Where(m => m.Id == 4 || m.Id == 5).ToList()
                    }
                );

                context.Notifications.AddRange(
                
                    new Notification
                    {
                        UserEmail = "matheus@stellantis.com",
                        Message = "Bem-vindo ao sistema de notificações!",
                        SentAt = DateTime.UtcNow.AddMinutes(-30)
                    },
                    new Notification
                    {
                        UserEmail = "patricia@stellantis.com",
                        Message = "Todas as notificações de Incidentes e Feedbacks aparecerão aqui.",
                        SentAt = DateTime.UtcNow.AddMinutes(-10)
                    }
                );

                await context.SaveChangesAsync().ConfigureAwait(false);
            }
        }
    }
}
#endif