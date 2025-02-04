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
            MapperVMToDto();
            MapperDtoToEntity();
        }

        private void MapperVMToDto()
        {
            CreateMap<VehicleFilterDto, VehicleFilter>().ReverseMap();
        }

        private void MapperDtoToEntity()
        {
            CreateMap(typeof(PagedResultDto<>), typeof(PagedResult<>));
            CreateMap<Vehicle, Vehicle>().ReverseMap();
            CreateMap<Supplier, Supplier>()
                .ConstructUsing(src => new Supplier(src.Code, src.CompanyName, src.Phone, src.Address));
            CreateMap<PartNumber, PartNumber>().ReverseMap();
        }
    }
}
