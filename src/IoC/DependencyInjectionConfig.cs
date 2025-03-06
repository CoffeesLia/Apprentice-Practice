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

        [SuppressMessage("Critical Code Smell", "S1186:Methods should not be empty", Justification = "It's code temporary.")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "It's code temporary.")]
        private static void Validators(IServiceCollection services)
        {
            services.AddScoped<IValidator<Area>, AreaValidator>();
        }

        private static void Services(IServiceCollection services)
        {
            services.AddScoped<IAreaService, AreaService>();
        }

        private static void Repositories(IServiceCollection services)
        {
            services.AddScoped<IAreaRepository, AreaRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
