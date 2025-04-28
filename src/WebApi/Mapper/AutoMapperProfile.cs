using AutoMapper;
using Stellantis.ProjectName.Application.Models.Filters;
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
            CreateMap<ApplicationDataDto, ApplicationData>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.Area, opt => opt.Ignore())
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

            CreateMap<AreaDto, Area>()
                 .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Applications, opt => opt.Ignore())
                .ForMember(dest => dest.Responsibles, opt => opt.Ignore());
            CreateMap<Area, AreaVm>()
                .ForMember(x => x.Applications, x => x.Ignore());
            CreateMap<AreaFilterDto, AreaFilter>();

            CreateMap<ResponsibleDto, Responsible>()
            .ForMember(x => x.Id, x => x.Ignore())
            .ForMember(x => x.AreaId, opt => opt.MapFrom(src => src.AreaId))
            .ForMember(x => x.Area, opt => opt.Ignore());
            CreateMap<Responsible, ResponsibleVm>()
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area));


            CreateMap<ResponsibleFilterDto, ResponsibleFilter>();

            CreateMap<ServiceDataDto, ServiceData>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            CreateMap<ServiceData, ServiceDataVm>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            CreateMap<ServiceDataFilterDto, ServiceDataFilter>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            CreateMap<PagedResult<ServiceData>, PagedResultVm<ServiceDataVm>>();

            CreateMap<IntegrationDto, Integration>()
              .ForMember(x => x.Id, x => x.Ignore())
              .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
              .ForMember(x => x.ApplicationData, opt => opt.MapFrom(src => src.ApplicationData));

            CreateMap<IntegrationFilterDto, IntegrationFilter>()
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(x => x.ApplicationData, opt => opt.MapFrom(src => src.ApplicationDataDto));

            CreateMap<PagedResult<Integration>, PagedResult<IntegrationVm>>()
                .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
                .ForMember(dest => dest.Result, opt => opt.MapFrom(src => src.Result))
                .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total));


            CreateMap<Integration, IntegrationVm>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ApplicationData, opt => opt.MapFrom(src => src.ApplicationData));

            CreateMap<SquadDto, Squad>()
             .ForMember(x => x.Id, x => x.Ignore()) // Ignorar o ID porque ele é gerado pelo banco
             .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description)) // Mapear Description
             .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name)); // Mapear Name

            CreateMap<PagedResult<Squad>, PagedResultVm<SquadVm>>();

            CreateMap<Squad, SquadVm>()
                            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<SquadFilterDto, SquadFilter>()
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<GitRepoDto, GitRepo>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.Application, opt => opt.Ignore())
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.Url, opt => opt.MapFrom(src => src.Url));

            CreateMap<GitRepo, GitRepoVm>()
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.Url, opt => opt.MapFrom(src => src.Url));

            CreateMap<GitRepoFilterDto, GitRepoFilter>()
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.Url, opt => opt.MapFrom(src => src.Url));
            CreateMap<PagedResult<GitRepo>, PagedResultVm<GitRepoVm>>();

            CreateMap<MemberDto, Member>()
                 .ForMember(x => x.Id, x => x.Ignore());
            CreateMap<Member, MemberVm>();
            CreateMap<MemberFilterDto, MemberFilter>();
            CreateMap<PagedResult<Member>, PagedResultVm<MemberVm>>();
        }
    }
}