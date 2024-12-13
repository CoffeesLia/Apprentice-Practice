using Application.Interfaces;
using Application.Services;
using Data;
using Domain.Interfaces;
using Domain.ViewModel;
using FluentValidation;
using Infrastructure.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;
using static Domain.ViewModel.PartNumberVM;
using static Domain.ViewModel.SupplierVM;
using static Domain.ViewModel.VehicleVM;

namespace IoC
{
    public static class DependencyInjectionConfig
    {
        public static void ConfigureDependencyInjection(this IServiceCollection services)
        {
            Validations(services);
            Services(services);
            Repositories(services);
        }

        private static void Validations(IServiceCollection services)
        {
            services.AddTransient<IValidator<PartNumberVM>, PartNumberVMValidation>();
            services.AddTransient<IValidator<VehicleVM>, VehicleVMValidation>();
            services.AddTransient<IValidator<SupplierVM>, SupplierVMValidation>();
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
            services.AddScoped<IPartNumberVehicleRepository, PartNumberVehicleRepository>();
            services.AddScoped<ISupplierRepository, SupplierRepository>();
            services.AddScoped<IPartNumberSupplierRepository, PartNumberSupplierRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
