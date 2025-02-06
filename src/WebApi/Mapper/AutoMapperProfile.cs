using AutoMapper;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;

namespace Stellantis.ProjectName.WebApi.Mapper
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<PartNumberDto, PartNumber>()
                .ConstructUsing(src => new PartNumber(src.Code!, src.Description!, src.Type!.Value))
                .ForMember(dest => dest.Suppliers, opt => opt.Ignore())
                .ForMember(dest => dest.Vehicles, opt => opt.Ignore()); 
            CreateMap<Vehicle, VehicleDto>()
                .ReverseMap();
            CreateMap<Supplier, SupplierDto>()
                .ReverseMap()
                .ConstructUsing(src => new Supplier(src.Code, src.CompanyName, src.Phone, src.Address));
            CreateMap<VehicleFilterDto, VehicleFilter>();
            CreateMap(typeof(PagedResult<>), typeof(PagedResultDto<>));
        }
    }
}
