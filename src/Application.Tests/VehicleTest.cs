using Application.Services;
using AutoFixture;
using AutoMapper;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Resources;

using Microsoft.Extensions.Localization;
using Microsoft.SqlServer.Server;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Domain.DTO;

namespace Application.Tests
{
    public class VehicleTest
    {
        [Fact]
        public async Task ValidVehicle_CreateIsCalled_ReturnValidVehicleDTO()
        {
            var vehicleDTO = new Fixture().Create<VehicleDTO>();
            var vehicle = new Fixture().Create<Vehicle>();
            vehicle.Id = 0;

            var vehicleRepositoryMock = new Mock<IVehicleRepository>();
            vehicleRepositoryMock.Setup(x => x.VerifyChassiExists(vehicleDTO.Chassi!)).Returns(false);


            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.VehicleRepository).Returns(vehicleRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<Vehicle>(It.IsAny<VehicleDTO>())).Returns(vehicle);
            var localizerMock = new Mock<IStringLocalizer<Messages>>();

            var vehicleService = new VehicleService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);
            await vehicleService.Create(vehicleDTO);

            vehicleRepositoryMock.Verify(x => x.Create(It.IsAny<Vehicle>(), true), Times.Once);
        }

        [Fact]
        public async Task InvalidVehicle_CreateIsCalled_ReturnInvalidVehicleDTO()
        {
            var vehicleDTO = new Fixture().Create<VehicleDTO>();
            var vehicle = new Fixture().Create<Vehicle>();
            vehicle.Id = 0;

            var vehicleRepositoryMock = new Mock<IVehicleRepository>();
            vehicleRepositoryMock.Setup(x => x.VerifyChassiExists(vehicleDTO.Chassi!)).Returns(true);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.VehicleRepository).Returns(vehicleRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<Vehicle>(It.IsAny<VehicleDTO>())).Returns(vehicle);

            var localizerMock = new Mock<IStringLocalizer<Messages>>();
            localizerMock.Setup(x => x["AlreadyExistChassi"]).Returns(new LocalizedString("AlreadyExistChassi", "Chassi already exists"));

            var vehicleService = new VehicleService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await vehicleService.Create(vehicleDTO));

            Assert.Equal("Chassi already exists", exception.Message);
        }

        [Fact]
        public async Task ValidVehicle_UpdateIsCalled_ReturnValidVehicleDTO()
        {
            var vehicleDTO = new Fixture().Create<VehicleDTO>();
            var vehicle = new Fixture().Create<Vehicle>();
            var oldVehicle = new Fixture().Create<Vehicle>();

            var vehicleRepositoryMock = new Mock<IVehicleRepository>();
            var partNumberVehicleRepositoryMock = new Mock<IPartNumberVehicleRepository>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.VehicleRepository).Returns(vehicleRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.PartNumberVehicleRepository).Returns(partNumberVehicleRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<Vehicle>(vehicleDTO)).Returns(vehicle);

            var localizerMock = new Mock<IStringLocalizer<Messages>>();

            var service = new VehicleService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);

            unitOfWorkMock.Setup(x => x.VehicleRepository.GetByIdWithPartNumber(It.IsAny<int>()))
                .ReturnsAsync(oldVehicle);

            partNumberVehicleRepositoryMock.Setup(x => x.Delete(It.IsAny<PartNumberVehicle>(), false)).Returns(Task.CompletedTask);

            await service.Update(vehicleDTO);

            unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            unitOfWorkMock.Verify(x => x.VehicleRepository.Update(vehicle, false), Times.Once);
            unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task NotFoundVehicle_UpdateIsCalled_ReturnNotFoundVehicleDTO()
        {
            var addVehicleInputModel = new Fixture().Create<VehicleDTO>();
            var vehicle = new Fixture().Create<Vehicle>();

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<Vehicle>(It.IsAny<VehicleDTO>())).Returns(vehicle);

            var vehicleRepositoryMock = new Mock<IVehicleRepository>();
            var partNumberVehicleRepositoryMock = new Mock<IPartNumberVehicleRepository>();

            vehicleRepositoryMock.Setup(x => x.GetByIdWithPartNumber(It.IsAny<int>())).ReturnsAsync((Vehicle)null!);
            partNumberVehicleRepositoryMock.Setup(x => x.Delete(It.IsAny<PartNumberVehicle>(), true)).Returns(Task.CompletedTask);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.VehicleRepository).Returns(vehicleRepositoryMock.Object);

            var localizerMock = new Mock<IStringLocalizer<Messages>>();
            localizerMock.Setup(x => x["NotFound"]).Returns(new LocalizedString("NotFound", "Not found"));

            var vehicleService = new VehicleService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await vehicleService.Update(addVehicleInputModel));

            Assert.Equal("Not found", exception.Message);
        }

        [Fact]
        public async Task ReturnVehicle_GetIsCalled_ReturnVehicleDTO()
        {
            var vehicle = new Fixture().Create<Vehicle>();
            var vehicleRepositoryMock = new Mock<IVehicleRepository>();
            vehicleRepositoryMock.Setup(x => x.GetByIdWithPartNumber(It.IsAny<int>())).ReturnsAsync(vehicle);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.VehicleRepository).Returns(vehicleRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<VehicleDTO>(It.IsAny<Vehicle>())).Returns(new Fixture().Create<VehicleDTO>());

            var localizerMock = new Mock<IStringLocalizer<Messages>>();
            localizerMock.Setup(x => x["SuccessRegister"]).Returns(new LocalizedString("SuccessRegister", "Successfully Registered"));

            var vehicleService = new VehicleService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);
            var result = await vehicleService.Get(1);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ReturnVehicle_GetListIsCalled_ReturnListVehicle()
        {
            var vehicleFilterDTO = new Fixture().Create<VehicleFilterDTO>();
            var paginationVehicle = new Fixture().Create<PaginationDTO<Vehicle>>();
            var paginationVehicleDTO = new PaginationDTO<VehicleDTO>()
            {
                Result = new Fixture().Create<List<VehicleDTO>>(),
                Total = paginationVehicle.Total
            };

            var id = new Fixture().Create<int>();

            var vehicleRepositoryMock = new Mock<IVehicleRepository>();
            vehicleRepositoryMock.Setup(x => x.GetListFilter(It.IsAny<VehicleFilterDTO>())).ReturnsAsync(paginationVehicle);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.VehicleRepository).Returns(vehicleRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<PaginationDTO<VehicleDTO>>(It.IsAny<PaginationDTO<Vehicle>>())).Returns(paginationVehicleDTO);

            var localizerMock = new Mock<IStringLocalizer<Messages>>();

            var vehicleService = new VehicleService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);
            var result = await vehicleService.GetList(vehicleFilterDTO);

            Assert.True(result.Result!.Count > 0);
            Assert.True(result.Total > 0);

        }

    }
}
