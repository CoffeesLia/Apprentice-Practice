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
            services.AddScoped<IValidator<GitRepo>, GitRepoValidator>();
            services.AddScoped<IValidator<Integration>, IntegrationValidator>();
            services.AddScoped<IValidator<ServiceData>, ServiceDataValidator>();
            services.AddScoped<IValidator<Squad>, SquadValidator>();
            services.AddScoped<IValidator<Member>, MemberValidator>();
            services.AddScoped<IValidator<Incident>, IncidentValidator>();
            services.AddScoped<IValidator<DocumentData>, DocumentDataValidator>();

        }

        private static void Services(IServiceCollection services)
        {
            services.AddScoped<IAreaService, AreaService>();
            services.AddScoped<IApplicationDataService, ApplicationDataService>();
            services.AddScoped<IResponsibleService, ResponsibleService>();
            services.AddScoped<IGitRepoService, GitRepoService>();
            services.AddScoped<IIntegrationService, IntegrationService>();
            services.AddScoped<IServiceData, ServiceDataService>();
            services.AddScoped<ISquadService, SquadService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IIncidentService, IncidentService>();
            services.AddScoped<IDocumentService, IDocumentDataService>();

        }

        private static void Repositories(IServiceCollection services)
        {
            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<IApplicationDataRepository, ApplicationDataRepository>();
            services.AddScoped<IResponsibleRepository, ResponsibleRepository>();
            services.AddScoped<IGitRepoRepository, GitRepoRepository>();
            services.AddScoped<IIntegrationRepository, IntegrationRepository>();
            services.AddScoped<IServiceDataRepository, ServiceDataRepository>();
            services.AddScoped<ISquadRepository, SquadRepository>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IIncidentRepository, IncidentRepository>();
            services.AddScoped<IDocumentRepository, DocumentDataRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}