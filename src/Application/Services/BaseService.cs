using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Resources;

namespace Stellantis.ProjectName.Application.Services
{
    public abstract class BaseService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory)
    {
        protected IStringLocalizer Localizer => localizerFactory.Create(typeof(GeneralResources));
        protected IUnitOfWork UnitOfWork { get; } = unitOfWork;
    }
}
