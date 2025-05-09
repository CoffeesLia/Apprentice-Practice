using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Application.Tests.Helpers
{
    //comentario
    internal static class LocalizerFactorHelper
    {
        internal static IStringLocalizerFactory Create()
        {
            ServiceCollection serviceCollection = new();
            serviceCollection.AddLogging();
            serviceCollection.AddLocalization();
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IStringLocalizerFactory>();
        }

    }
}
