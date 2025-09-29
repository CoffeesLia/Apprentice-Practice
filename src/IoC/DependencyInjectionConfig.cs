using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.Validators;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Stellantis.ProjectName.IoC
{
    public static class DependencyInjectionConfig
    {
        public static void ConfigureDependencyInjection(this IServiceCollection services)
        {
            Services(services);
            Validators(services);
            Repositories(services);
        }

        private static void Validators(IServiceCollection services)
        {
            services.AddScoped<IValidator<Area>, AreaValidator>();
            services.AddScoped<IValidator<ApplicationData>, ApplicationDataValidator>();
            services.AddScoped<IValidator<Responsible>, ResponsibleValidator>();
            services.AddScoped<IValidator<Feedback>, FeedbackValidator>();
            services.AddScoped<IValidator<Repo>, RepoValidator>();
            services.AddScoped<IValidator<Integration>, IntegrationValidator>();
            services.AddScoped<IValidator<ServiceData>, ServiceDataValidator>();
            services.AddScoped<IValidator<Manager>, ManagerValidator>();
            services.AddScoped<IValidator<Squad>, SquadValidator>();
            services.AddScoped<IValidator<Member>, MemberValidator>();
            services.AddScoped<IValidator<Incident>, IncidentValidator>();
            services.AddScoped<IValidator<DocumentData>, DocumentDataValidator>();
            services.AddScoped<IValidator<Knowledge>, KnowledgeValidator>();
        }

        private static void Services(IServiceCollection services)
        {
            services.AddScoped<IAreaService, AreaService>();
            services.AddScoped<IApplicationDataService, ApplicationDataService>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddScoped<IResponsibleService, ResponsibleService>();
            services.AddScoped<IRepoService, RepoService>();
            services.AddScoped<IIntegrationService, IntegrationService>();
            services.AddScoped<IServiceDataService, ServiceDataService>();
            services.AddScoped<IManagerService, ManagerService>();
            services.AddScoped<ISquadService, SquadService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IDocumentService, DocumentDataService>();
            services.AddScoped<IIncidentService, IncidentService>();
            services.AddScoped<IKnowledgeService, KnowledgeService>();
            services.AddScoped<IDashboardService, DashboardService>();
        }

        private static void Repositories(IServiceCollection services)
        {
            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<IApplicationDataRepository, ApplicationDataRepository>();
            services.AddScoped<IResponsibleRepository, ResponsibleRepository>();
            services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            services.AddScoped<IRepoRepository, RepoRepository>();
            services.AddScoped<IIntegrationRepository, IntegrationRepository>();
            services.AddScoped<IServiceDataRepository, ServiceDataRepository>();
            services.AddScoped<IManagerRepository, ManagerRepository>();
            services.AddScoped<ISquadRepository, SquadRepository>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IIncidentRepository, IncidentRepository>();
            services.AddScoped<IDocumentRepository, DocumentDataRepository>();
            services.AddScoped<IKnowledgeRepository, KnowledgeRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}