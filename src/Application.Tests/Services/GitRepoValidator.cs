using Microsoft.Extensions.Localization;

namespace Application.Tests.Services
{
    internal class GitRepoValidator
    {
        private IStringLocalizerFactory localizer;

        public GitRepoValidator(IStringLocalizerFactory localizer)
        {
            this.localizer = localizer;
        }
    }
}