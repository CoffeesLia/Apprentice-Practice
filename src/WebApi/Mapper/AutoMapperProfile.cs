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
            CreateMap<PartNumberDto, PartNumber>()
                .ForMember(x => x.Id, x => x.Ignore())
                .ConstructUsing(src => new PartNumber(src.Code!, src.Description!, src.Type!.Value))
                .ForMember(dest => dest.Suppliers, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore());
            CreateMap<PartNumber, PartNumberVm>();
            CreateMap<PartNumberFilterDto, PartNumberFilter>();

            CreateMap<SupplierDto, Supplier>()
                .ForMember(x => x.Id, x => x.Ignore());
            CreateMap<SupplierFilterDto, SupplierFilter>();
            CreateMap<Supplier, SupplierVm>()
                .ReverseMap()
                .ConstructUsing(src => new Supplier(src.Code, src.CompanyName, src.Phone, src.Address));

            CreateMap<VehicleDto, Vehicle>()
                .ForMember(x => x.Id, x => x.Ignore());
            CreateMap<Vehicle, VehicleVm>();
            CreateMap<VehicleFilterDto, VehicleFilter>();

            CreateMap(typeof(PagedResult<>), typeof(PagedResultVm<>));
        }
    }
}
