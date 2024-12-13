using Application.ViewModel;
using AutoMapper;
using Domain.DTO;
using Domain.Entities;
using Domain.ViewModel;


namespace Application.Mapper
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            this.MapperVMToDTO();
            this.MapperDTOToEntity();
        }

        private void MapperVMToDTO()
        {

            CreateMap(typeof(PaginationVM<>), typeof(PaginationDTO<>)).ReverseMap();
            CreateMap<SupplierVM, SupplierDTO>().ReverseMap();
            CreateMap<PartNumberVM, PartNumberDTO>().ReverseMap();
            CreateMap<VehicleVM, VehicleDTO>().ReverseMap();
            CreateMap<PartNumberSupplierVM, PartNumberSupplierDTO>().ReverseMap();
            CreateMap<PartNumberVehicleVM, PartNumberVehicleDTO>().ReverseMap();
            CreateMap<SupplierFilterVM, SupplierFilterDTO>().ReverseMap();
            CreateMap<PartNumberFilterVM, PartNumberFilterDTO>().ReverseMap();
            CreateMap<VehicleFilterVM, VehicleFilterDTO>().ReverseMap();


        }

        private void MapperDTOToEntity()
        {
            CreateMap(typeof(PaginationDTO<>), typeof(PaginationDTO<>));
            CreateMap<Vehicle, VehicleDTO>().ReverseMap();
            CreateMap<PartNumberVehicle, PartNumberVehicleDTO>().ReverseMap();
            CreateMap<Supplier, SupplierDTO>().ReverseMap();
            CreateMap<PartNumberSupplier, PartNumberSupplierDTO>().ReverseMap();
            CreateMap<PartNumber, PartNumberDTO>().ReverseMap();
        }
    }




}
