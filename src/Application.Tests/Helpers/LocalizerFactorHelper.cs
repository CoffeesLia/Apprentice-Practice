using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Application.Tests.Helpers
{
    //comentario
    internal static class LocalizerFactorHelper
    {
        internal static IStringLocalizerFactory Create()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddLocalization();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IStringLocalizerFactory>();
        }

    }
}
