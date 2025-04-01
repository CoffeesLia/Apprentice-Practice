using AutoMapper;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace Stellantis.ProjectName.WebApi.Mapper
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap(typeof(PagedResult<>), typeof(PagedResultVm<>));
            CreateMap<AreaDto, Area>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.Applications, x => x.Ignore());
            CreateMap<Area, AreaVm>()
                .ForMember(x => x.Applications, x => x.Ignore());
            CreateMap<AreaFilterDto, AreaFilter>();

            CreateMap<DataServiceDto, ApplicationService>();

            CreateMap<SquadDto, Squad>();
        }
    }
}
