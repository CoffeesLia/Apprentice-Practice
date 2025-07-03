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
                .ForMember(x => x.Repos, opt => opt.Ignore())
                .ForMember(x => x.Id, x => x.Ignore())
                .ForMember(x => x.Area, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(x => x.Integration, opt => opt.Ignore())
                .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(x => x.ProductOwner, opt => opt.MapFrom(src => src.ProductOwner))
                .ForMember(x => x.ConfigurationItem, opt => opt.MapFrom(src => src.ConfigurationItem))
                .ForMember(x => x.External, opt => opt.MapFrom(src => src.External))
                .ForMember(x => x.ResponsibleId, opt => opt.MapFrom(src => src.ResponsibleId))
                .ForMember(dest => dest.Squads, opt => opt.Ignore());


            CreateMap<ApplicationData, ApplicationVm>()
                .ForMember(dest => dest.Squads, opt => opt.MapFrom(src => src.Squads))
                .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ProductOwner, opt => opt.MapFrom(src => src.ProductOwner))
                .ForMember(dest => dest.ConfigurationItem, opt => opt.MapFrom(src => src.ConfigurationItem))
                .ForMember(dest => dest.External, opt => opt.MapFrom(src => src.External));

            CreateMap<ApplicationDataFilterDto, ApplicationFilter>()
                .ForMember(x => x.SquadId, opt => opt.MapFrom(src => src.SquadId))
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
                .ForMember(dest => dest.Id, opt => opt.Ignore())
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

            CreateMap<FeedbackDto, Feedback>()
              .ForMember(dest => dest.Id, opt => opt.Ignore())
              .ForMember(dest => dest.Members, opt => opt.Ignore())
              .ForMember(dest => dest.Application, opt => opt.Ignore())
              .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
              .ForMember(dest => dest.ClosedAt, opt => opt.Ignore())
              .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
            CreateMap<FeedbackFilterDto, FeedbackFilter>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
            CreateMap<Feedback, FeedbackVm>()
             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
             .ForMember(dest => dest.Members, opt => opt.Ignore())
             .ForMember(dest => dest.Application, opt => opt.MapFrom(src => src.Application));

            CreateMap<IncidentDto, Incident>()
             .ForMember(dest => dest.Id, opt => opt.Ignore())
             .ForMember(dest => dest.Members, opt => opt.Ignore())
             .ForMember(dest => dest.Application, opt => opt.Ignore())
             .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
             .ForMember(dest => dest.ClosedAt, opt => opt.Ignore())
             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            CreateMap<IncidentFilterDto, IncidentFilter>()
             .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.MemberIds, opt => opt.MapFrom(src => src.MemberIds));

            CreateMap<Incident, IncidentVm>()
             .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
             .ForMember(dest => dest.Application, opt => opt.MapFrom(src => src.Application))
             .ForMember(dest => dest.Members, opt => opt.MapFrom(src => src.Members));

            CreateMap<IntegrationFilterDto, IntegrationFilter>()
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ApplicationDataId, opt => opt.MapFrom(src => src.ApplicationDataId));

            CreateMap<IntegrationDto, Integration>()
            .ForMember(x => x.Id, x => x.Ignore())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.ApplicationDataId, opt => opt.MapFrom(src => src.ApplicationDataId))
            .ForMember(dest => dest.ApplicationData, opt => opt.Ignore());

            CreateMap<Integration, IntegrationVm>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.ApplicationDataId, opt => opt.MapFrom(src => src.ApplicationDataId));

            CreateMap<PagedResult<Integration>, PagedResultVm<IntegrationVm>>()
                .ForMember(dest => dest.Result, opt => opt.MapFrom(src => src.Result))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.Page, opt => opt.MapFrom(src => src.Page))
                .ForMember(dest => dest.PageSize, opt => opt.MapFrom(src => src.PageSize));

            CreateMap<SquadDto, Squad>()
            .ForMember(x => x.Id, x => x.Ignore())
            .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(x => x.Cost, x => x.Ignore())
            .ForMember(x => x.Members, x => x.Ignore());

            CreateMap<PagedResult<Squad>, PagedResultVm<SquadVm>>();
            CreateMap<Squad, SquadVm>()
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Members != null ? src.Members.Sum(m => m.Cost) : 0))
               .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Cost ?? 0));
            CreateMap<SquadFilterDto, SquadFilter>()
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<RepoDto, Repo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationData, opt => opt.Ignore())
                .ForMember(x => x.Url, opt => opt.MapFrom(src => src.Url));

            CreateMap<Repo, RepoVm>()
                .ForMember(dest => dest.ApplicationData, opt => opt.MapFrom(src => src.ApplicationData))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.Url, opt => opt.MapFrom(src => src.Url));

            CreateMap<RepoFilterDto, RepoFilter>()
                .ForMember(x => x.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.ApplicationId, opt => opt.MapFrom(src => src.ApplicationId));


            CreateMap<MemberDto, Member>()
               .ForMember(dest => dest.Id, opt => opt.Ignore())
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Cost))
               .ForMember(dest => dest.SquadId, opt => opt.MapFrom(src => src.SquadId))
                .ForMember(dest => dest.Squads, opt => opt.Ignore());

            CreateMap<Member, MemberVm>().ReverseMap()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Cost))
               .ForMember(dest => dest.SquadId, opt => opt.MapFrom(src => src.SquadId))
               .ForMember(dest => dest.Squads, opt => opt.MapFrom(src => src.Squads));


            CreateMap<MemberFilterDto, MemberFilter>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Cost))
               .ForMember(dest => dest.SquadId, opt => opt.MapFrom(src => src.SquadId));

            CreateMap<PagedResult<Member>, PagedResultVm<MemberVm>>();

            CreateMap<DocumentDto, DocumentData>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationData, opt => opt.Ignore())
                .ForMember(x => x.Url, opt => opt.MapFrom(src => src.Url));

            CreateMap<DocumentData, DocumentVm>()
                .ForMember(dest => dest.ApplicationData, opt => opt.MapFrom(src => src.ApplicationData))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.Url, opt => opt.MapFrom(src => src.Url));

            CreateMap<DocumentDataFilterDto, DocumentDataFilter>()
                .ForMember(x => x.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(x => x.ApplicationId, opt => opt.MapFrom(src => src.ApplicationId));

            CreateMap<ManagerDto, Manager>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
            CreateMap<Manager, ManagerVm>().ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
            CreateMap<ManagerFilterDto, ManagerFilter>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email));
            CreateMap<PagedResult<Manager>, PagedResultVm<ManagerVm>>();
        }
    }
}