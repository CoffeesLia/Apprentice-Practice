using AutoMapper;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.Domain.Entity;
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

            CreateMap<DataServiceDto, DataService>();

            // Correção do mapeamento de Squad
            CreateMap<SquadDto, EntitySquad>()
                .ForMember(x => x.Id, x => x.Ignore());
            CreateMap<EntitySquad, SquadDto>();

            // Adicionando mapeamento para Responsible
            CreateMap<ResponsibleDto, Responsible>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.AreaId, opt => opt.MapFrom(src => src.AreaId));
            CreateMap<Responsible, ResponsibleVm>()
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area));
            CreateMap<ResponsibleFilterDto, ResponsibleFilter>();

            CreateMap<ApplicationDataDto, ApplicationData>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.Integration, opt => opt.Ignore())
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.ProductOwner, opt => opt.MapFrom(src => src.ProductOwner))
                .ForMember(x => x.ConfigurationItem, opt => opt.MapFrom(src => src.ConfigurationItem))
                .ForMember(x => x.External, opt => opt.MapFrom(src => src.External));
            CreateMap<ApplicationData, ApplicationVm>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ProductOwner, opt => opt.MapFrom(src => src.ProductOwner))
                .ForMember(dest => dest.ConfigurationItem, opt => opt.MapFrom(src => src.ConfigurationItem))
                .ForMember(dest => dest.External, opt => opt.MapFrom(src => src.External));
            CreateMap<ApplicationDataFilterDto, ApplicationFilter>()
                .ForMember(x => x.AreaId, opt => opt.MapFrom(src => src.AreaId));

            CreateMap<Integration, IntegrationVM>()
                .ForMember(dest => dest.ApplicationData,
                opt => opt.MapFrom(src => src.ApplicationData)); 
        }
    }
}

