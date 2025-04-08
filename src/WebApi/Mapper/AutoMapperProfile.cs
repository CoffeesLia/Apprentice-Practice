using AutoMapper;
using Stellantis.ProjectName.Application.Models.Filters;
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
            CreateMap<ApplicationDataDto, ApplicationData>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.Integration, opt => opt.Ignore())
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.ProductOwner, opt => opt.MapFrom(src => src.ProductOwner))
                .ForMember(x => x.ConfigurationItem, opt => opt.MapFrom(src => src.ConfigurationItem))
                .ForMember(x => x.External, opt => opt.MapFrom(src => src.External))
                .ForMember(x => x.ResponsibleId, opt => opt.MapFrom(src => src.ResponsibleId))
                .ForMember(x => x.Responsibles, opt => opt.MapFrom(src => src.Responsibles));

            CreateMap<ApplicationData, ApplicationVm>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ProductOwner, opt => opt.MapFrom(src => src.ProductOwner))
                .ForMember(dest => dest.ConfigurationItem, opt => opt.MapFrom(src => src.ConfigurationItem))
                .ForMember(dest => dest.External, opt => opt.MapFrom(src => src.External));

            CreateMap<ApplicationDataFilterDto, ApplicationFilter>()
                .ForMember(x => x.AreaId, opt => opt.MapFrom(src => src.AreaId))
                .ForMember(x => x.External, opt => opt.MapFrom(src => src.External))
                .ForMember(x => x.ProductOwner, opt => opt.MapFrom(src => src.ProductOwner));

            CreateMap<DataServiceDto, DataService>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.ServiceId, x => x.Ignore());
            CreateMap<DataService, DataServiceVm>();

            CreateMap<SquadDto, EntitySquad>()
                .ForMember(x => x.Id, x => x.Ignore());
            CreateMap<EntitySquad, SquadDto>();

            CreateMap<GitRepoDto, GitRepo>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.Application, opt => opt.MapFrom(src => src.Application))
                .ForMember(x => x.ApplicationId, opt => opt.MapFrom(src => src.ApplicationId));
            CreateMap<GitRepo, GitRepoVm>();
        }
    }
}