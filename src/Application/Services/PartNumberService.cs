using Application.Interfaces;
using AutoMapper;
using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Resources;

using Microsoft.Extensions.Localization;

namespace Application.Services
{
    public class PartNumberService : IPartNumberService
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer<Messages> _localizer;

        public PartNumberService(IMapper mapper, IUnitOfWork unitOfWork, IStringLocalizer<Messages> localizer)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _localizer = localizer;
        }


        public async Task Create(PartNumberDTO partNumberDTO)
        {
            var partNumberValidated = ValidateCreateOrUpdate(partNumberDTO);

            if (_unitOfWork.PartNumberRepository.VerifyCodeExists(partNumberDTO.Code!))
            {
                throw new InvalidOperationException(_localizer["AlreadyExistCode"].Value);
            }

            var partNumber = this._mapper.Map<PartNumber>(partNumberValidated);
            await this._unitOfWork.PartNumberRepository.Create(partNumber);
        }

        public async Task Update(PartNumberDTO partNumberDTO)
        {
            var partNumberValidated = ValidateCreateOrUpdate(partNumberDTO);

            if (_unitOfWork.PartNumberRepository.VerifyCodeExists(partNumberDTO.Code!))
            {
                throw new InvalidOperationException(_localizer["AlreadyExistCode"].Value);
            }

            _unitOfWork.BeginTransaction();

            var partNumberDB = await _unitOfWork.PartNumberRepository.GetById(partNumberDTO.Id) ?? throw new InvalidOperationException(_localizer["NotFound"].Value);
            this._unitOfWork.PartNumberRepository.DetachEntity(partNumberDB);

            var partNumber = this._mapper.Map<PartNumber>(partNumberValidated);
            await this._unitOfWork.PartNumberRepository.Update(partNumber);

            await _unitOfWork.Commit();
        }

        public async Task<PartNumberDTO> Get(int id)
        {
            var partNumber = await _unitOfWork.PartNumberRepository.GetById(id);
            return partNumber == null
                ? throw new InvalidOperationException(_localizer["NotFound"].Value)
                : this._mapper.Map<PartNumberDTO>(await _unitOfWork.PartNumberRepository.GetById(id));
        }

        public async Task<PaginationDTO<PartNumberDTO>> GetList(PartNumberFilterDTO filter)
        {
            return this._mapper.Map<PaginationDTO<PartNumberDTO>>(await _unitOfWork.PartNumberRepository.GetListFilter(filter));

        }

        public async Task Delete(int id)
        {
            var partNumber = await _unitOfWork.PartNumberRepository.GetByIdWithInclude(id, x => x.PartNumberVehicle!, x => x.PartNumberSupplier!);
            if(partNumber.PartNumberSupplier!.Count == 0 || partNumber.PartNumberVehicle!.Count == 0)
            {
                throw new InvalidOperationException(_localizer["UndeletedDrawing"].Value);
            }
            await _unitOfWork.PartNumberRepository.Delete(id);
        }


        private static PartNumberDTO ValidateCreateOrUpdate(PartNumberDTO partNumberDTO)
        {
            if (partNumberDTO.Code!.Length < 11)
                partNumberDTO.Code = partNumberDTO.Code.PadLeft(11, '0');

            return partNumberDTO;
        }
    }
}
