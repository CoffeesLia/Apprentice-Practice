using Microsoft.Extensions.DependencyInjection;
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
        }

        [SuppressMessage("Critical Code Smell", "S1186:Methods should not be empty", Justification = "It's code temporary.")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "It's code temporary.")]
        private static void Services(IServiceCollection services)
        {
        }

        [SuppressMessage("Critical Code Smell", "S1186:Methods should not be empty", Justification = "It's code temporary.")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "It's code temporary.")]
        private static void Repositories(IServiceCollection services)
        {
        }
    }
}
