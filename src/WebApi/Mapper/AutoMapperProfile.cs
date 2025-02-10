using AutoMapper;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Mapper
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap(typeof(PagedResult<>), typeof(PagedResultVm<>));
        }
    }
}
