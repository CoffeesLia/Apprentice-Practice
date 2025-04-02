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
using System.Diagnostics.CodeAnalysis;

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
            services.AddScoped<IValidator<Responsible>, ResponsibleValidator>();
            services.AddScoped<IValidator<DataService>, DataServiceValidator>();
            services.AddScoped<IValidator<Squad>, SquadValidator>();
            services.AddScoped<IValidator<Integration>, IntegrationValidator>();
        }


        private static void Services(IServiceCollection services)
        {
            services.AddScoped<IIntegrationService, IntegrationService>();
            services.AddScoped<IResponsibleService, ResponsibleService>();
            services.AddScoped<IDataService, ApplicationService>();
            services.AddScoped<ISquadService, SquadService>();
            services.AddScoped<IValidator<Integration>, IntegrationValidator>();
        }

        private static void Repositories(IServiceCollection services)
        {
            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<IResponsibleRepository, ResponsibleRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IDataServiceRepository, DataServiceRepository>();
            services.AddScoped<ISquadRepository, SquadRepository>();
            services.AddScoped<IIntegrationRepository, IntegrationRepository>();
        }
    }
}
