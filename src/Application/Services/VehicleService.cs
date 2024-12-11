using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;

using Microsoft.Extensions.Localization;
using AutoMapper;
using Domain.Resources;
using Domain.DTO;

namespace Application.Services
{
    public class VehicleService(IMapper mapper, IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer) : IVehicleService
    {
        private readonly IMapper _mapper = mapper;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IStringLocalizer<Messages> _localizer = localizer;

        public async Task Create(VehicleDTO vehicleDTO)
        {

            if (_unitOfWork.VehicleRepository.VerifyChassiExists(vehicleDTO.Chassi!))
            {
                throw new InvalidOperationException(_localizer["AlreadyExistChassi"].Value);
            }

            var vehicle = this._mapper.Map<Vehicle>(vehicleDTO);

            VerifyDuplicatePartNumbers(vehicle);

            await this._unitOfWork.VehicleRepository.Create(vehicle);
        }

        public async Task Delete(int id)
        {
            var vehicle = await _unitOfWork.VehicleRepository.GetByIdWithInclude(id, x => x.PartNumberVehicle!);
            await _unitOfWork.VehicleRepository.Delete(vehicle);
        }

        public async Task<VehicleDTO> Get(int id)
        {
            var vehicle = await _unitOfWork.VehicleRepository.GetByIdWithPartNumber(id);
            return vehicle == null ? throw new InvalidOperationException(_localizer["NotFound"].Value) : this._mapper.Map<VehicleDTO>(vehicle);
        }

        public async Task<PaginationDTO<VehicleDTO>> GetList(VehicleFilterDTO filter)
        {
            return this._mapper.Map<PaginationDTO<VehicleDTO>>(await _unitOfWork.VehicleRepository.GetListFilter(filter));

        }

        public async Task Update(VehicleDTO vehicleDTO)
        {
            var vehicle = this._mapper.Map<Vehicle>(vehicleDTO);
            VerifyDuplicatePartNumbers(vehicle);

            _unitOfWork.BeginTransaction();

            await this.DeleteExists(vehicle);
            await this._unitOfWork.VehicleRepository.Update(vehicle);

            await _unitOfWork.Commit();
        }

        private async Task DeleteExists(Vehicle vehicle)
        {
            var oldVehicle = await _unitOfWork.VehicleRepository.GetByIdWithPartNumber(vehicle.Id) ?? throw new InvalidOperationException(_localizer["NotFound"].Value);
            foreach (var partNumberVehicle in oldVehicle.PartNumberVehicle!)
            {
                await this._unitOfWork.PartNumberVehicleRepository.Delete(partNumberVehicle);
            }
            this._unitOfWork.VehicleRepository.DetachEntity(oldVehicle);
        }

        private void VerifyDuplicatePartNumbers(Vehicle vehicle)
        {
            var group = vehicle.PartNumberVehicle!.GroupBy(p => p.PartNumberId);
            var duplicates = group.Where(g => g.Count() > 1).ToList();

            if (duplicates.Count > 0)
                throw new InvalidOperationException(_localizer["TheDrawing"].Value + " " + $"{string.Join(", ", duplicates.Select(p => p.Key))}" + " " + _localizer["AlreadyExistPartNumberSupplier"].Value);
        }
    }
}

