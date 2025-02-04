using Microsoft.Extensions.DependencyInjection;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Infrastructure.Data;
using Stellantis.ProjectName.Infrastructure.Data.Repositories;

namespace Stellantis.ProjectName.IoC
{
    public static class DependencyInjectionConfig
    {
        public static void ConfigureDependencyInjection(this IServiceCollection services)
        {
            Services(services);
            Repositories(services);
        }

        private static void Services(IServiceCollection services)
        {
            services.AddScoped<IPartNumberService, PartNumberService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<ISupplierService, SupplierService>();
        }

        private static void Repositories(IServiceCollection services)
        {
            services.AddScoped<IPartNumberRepository, PartNumberRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
