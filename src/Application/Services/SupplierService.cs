using Application.Helpers;
using Application.Interfaces;
using AutoMapper;
using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Resources;

using Microsoft.Extensions.Localization;

namespace Application.Services
{
    public class SupplierService(IMapper mapper, IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer) : ISupplierService
    {
        private readonly IMapper _mapper = mapper;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IStringLocalizer<Messages> _localizer = localizer;

        public async Task Create(SupplierDTO supplierDTO)
        {
            VerifyDuplicatePartNumbers(supplierDTO);

            if (_unitOfWork.SupplierRepository.VerifyCodeExists(supplierDTO.Code!))
            {
                throw new InvalidOperationException(_localizer["AlreadyExistCode"].Value);
            }

            var supplier = this._mapper.Map<Supplier>(supplierDTO);


            await this._unitOfWork.SupplierRepository.Create(supplier);
        }

        public async Task Delete(int id)
        {
            var supplier = await _unitOfWork.SupplierRepository.GetByIdWithInclude(id, x => x.PartNumberSupplier!);
            await _unitOfWork.SupplierRepository.Delete(supplier);
        }

        public async Task<SupplierDTO> Get(int id)
        {
            var supplier = await _unitOfWork.SupplierRepository.GetByIdWithPartNumber(id);
            return supplier == null 
                ? throw new InvalidOperationException(_localizer["NotFound"].Value) 
                : this._mapper.Map<SupplierDTO>(supplier);
        }

        public async Task Update(SupplierDTO supplierDTO)
        {
            VerifyDuplicatePartNumbers(supplierDTO);

            var supplier = await _unitOfWork.SupplierRepository.GetByIdWithPartNumber(supplierDTO.Id) ?? throw new InvalidOperationException(_localizer["NotFound"].Value);
            
            var partnumbersIdsToDelete = EntityComparison.PropsDiffer(supplier.PartNumberSupplier, supplierDTO.PartNumberSupplier, "PartNumberId").ToList();
            await _unitOfWork.PartNumberSupplierRepository.Delete(supplier.PartNumberSupplier.Where(f => partnumbersIdsToDelete.Contains(f.PartNumberId.GetValueOrDefault())), false);
            
            _mapper.Map(supplierDTO, supplier);

            await this._unitOfWork.SupplierRepository.Update(supplier); 
        }

        public async Task<PaginationDTO<SupplierDTO>> GetList(SupplierFilterDTO filter)
        {
           return this._mapper.Map<PaginationDTO<SupplierDTO>>(await _unitOfWork.SupplierRepository.GetListFilter(filter));
        }

        private async Task DeleteExists(Supplier supplier)
        {
            var oldSupplier = await _unitOfWork.SupplierRepository.GetByIdWithPartNumber(supplier.Id) ?? throw new InvalidOperationException(_localizer["NotFound"].Value);
            foreach (var partNumberSupplier in oldSupplier.PartNumberSupplier!)
            {
                await this._unitOfWork.PartNumberSupplierRepository.Delete(partNumberSupplier);
            }
            this._unitOfWork.SupplierRepository.DetachEntity(oldSupplier);
        }

        private void VerifyDuplicatePartNumbers(SupplierDTO supplier)
        {

            var group = supplier.PartNumberSupplier!.GroupBy(p => p.PartNumberId);
            var duplicates = group.Where(g => g.Count() > 1).ToList();

            if (duplicates.Count > 0)
                throw new InvalidOperationException(_localizer["TheDrawing"].Value + " " + $"{string.Join(", ", duplicates.Select(p => p.Key))}" + " " + _localizer["AlreadyExistPartNumberSupplier"].Value);

        }
    }



}

