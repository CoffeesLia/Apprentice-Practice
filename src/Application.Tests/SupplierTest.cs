using Application.Services;
using AutoFixture;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Resources;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;
using Domain.DTO;

namespace Application.Tests
{
    public class SupplierTest
    {

        [Fact]
        public async Task ValidSupplier_CreateIsCalled_ReturnValidSupplier()
        {
            var supplierDTO = new Fixture().Create<SupplierDTO>();
            var supplier = new Fixture().Create<Supplier>();
            supplier.Id = 0;

            var supplierRepositoryMock = new Mock<ISupplierRepository>();
            supplierRepositoryMock.Setup(x => x.VerifyCodeExists(supplierDTO.Code!)).Returns(false);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.SupplierRepository).Returns(supplierRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<Supplier>(It.IsAny<SupplierDTO>())).Returns(supplier);
          

            var localizerMock = new Mock<IStringLocalizer<Messages>>();

            var supplierService = new SupplierService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);
            await supplierService.Create(supplierDTO);

            supplierRepositoryMock.Verify(x => x.Create(It.IsAny<Supplier>(), true), Times.Once);
        }

        [Fact]
        public async Task InvalidSupplier_CreateIsCalled_ReturnInvalidVehicleDTO()
        {
            var supplierDTO = new Fixture().Create<SupplierDTO>();
            var supplier = new Fixture().Create<Supplier>();
            supplier.Id = 0;

            var supplierRepositoryMock = new Mock<ISupplierRepository>();
            supplierRepositoryMock.Setup(x => x.VerifyCodeExists(supplierDTO.Code!)).Returns(true);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.SupplierRepository).Returns(supplierRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<Supplier>(It.IsAny<SupplierDTO>())).Returns(supplier);

            var localizerMock = new Mock<IStringLocalizer<Messages>>();
            localizerMock.Setup(x => x["AlreadyExistCode"]).Returns(new LocalizedString("AlreadyExistCode", "Code already exists"));

            var supplierService = new SupplierService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await supplierService.Create(supplierDTO));

            Assert.Equal("Code already exists", exception.Message);

        }

        [Fact]
        public async Task ValidSupplier_UpdateIsCalled_ReturnValidSupplierDTO()
        {
            var supplierDTO = new Fixture().Create<SupplierDTO>();
            var supplier = new Fixture().Create<Supplier>();
            var oldsupplier = new Fixture().Create<Supplier>();

            var supplierRepositoryMock = new Mock<ISupplierRepository>();
            var partNumberSupplierRepositoryMock = new Mock<IPartNumberSupplierRepository>();

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.SupplierRepository).Returns(supplierRepositoryMock.Object);
            unitOfWorkMock.SetupGet(x => x.PartNumberSupplierRepository).Returns(partNumberSupplierRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<Supplier>(supplierDTO)).Returns(supplier);

            var localizerMock = new Mock<IStringLocalizer<Messages>>();

            var supplierService = new SupplierService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);

            unitOfWorkMock.Setup(x => x.SupplierRepository.GetByIdWithPartNumber(It.IsAny<int>()))
                .ReturnsAsync(oldsupplier);

            partNumberSupplierRepositoryMock.Setup(x => x.Delete(It.IsAny<PartNumberSupplier>(), false))
                .Returns(Task.CompletedTask);

            await supplierService.Update(supplierDTO);

            unitOfWorkMock.Verify(x => x.BeginTransaction(), Times.Once);
            unitOfWorkMock.Verify(x => x.SupplierRepository.Update(supplier, true), Times.Once);
            unitOfWorkMock.Verify(x => x.Commit(), Times.Once);
        }

        [Fact]
        public async Task NotFoundSupplier_UpdateIsCalled_ReturnNotFoundSupplierDTO()
        {
            var supplierDTO = new Fixture().Create<SupplierDTO>();
            var supplier = new Fixture().Create<Supplier>();

            var supplierRepositoryMock = new Mock<ISupplierRepository>();
            supplierRepositoryMock.Setup(x => x.GetByIdWithPartNumber(It.IsAny<int>())).ReturnsAsync((Supplier)null!);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.SupplierRepository).Returns(supplierRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<Supplier>(It.IsAny<SupplierDTO>())).Returns(supplier);

            var localizerMock = new Mock<IStringLocalizer<Messages>>();
            localizerMock.Setup(x => x["NotFound"]).Returns(new LocalizedString("NotFound", "Not found"));

            var supplierService = new SupplierService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await supplierService.Update(supplierDTO));

            Assert.Equal("Not found", exception.Message);
        }

        [Fact]
        public async Task ReturnSupplier_GetIsCalled_ReturnSupplierMV()
        {
            var supplier = new Fixture().Create<Supplier>();

            var supplierRepositoryMock = new Mock<ISupplierRepository>();
            supplierRepositoryMock.Setup(x => x.GetByIdWithPartNumber(It.IsAny<int>())).ReturnsAsync(supplier);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.SupplierRepository).Returns(supplierRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<SupplierDTO>(It.IsAny<Supplier>())).Returns(new SupplierDTO());

            var localizerMock = new Mock<IStringLocalizer<Messages>>();

            var supplierService = new SupplierService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);
            var result = await supplierService.Get(1);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task ReturnSupplier_GetListIsCalled_ReturnListSupplier()
        {
            var supplierFilterDTO = new Fixture().Create<SupplierFilterDTO>();
            var paginationSupplier = new Fixture().Create<PaginationDTO<Supplier>>();
            var paginationSupplierDTO = new PaginationDTO<SupplierDTO>()
            {
                Result = new Fixture().Create<List<SupplierDTO>>(),
                Total = paginationSupplier.Total
            };

            var id = new Fixture().Create<int>();

            var supplierRepositoryMock = new Mock<ISupplierRepository>();
            supplierRepositoryMock.Setup(x => x.GetListFilter(It.IsAny<SupplierFilterDTO>())).ReturnsAsync(paginationSupplier);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.SupplierRepository).Returns(supplierRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<PaginationDTO<SupplierDTO>>(It.IsAny<PaginationDTO<Supplier>>())).Returns(paginationSupplierDTO);

            var localizerMock = new Mock<IStringLocalizer<Messages>>();

            var supplierService = new SupplierService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);
            var result = await supplierService.GetList(supplierFilterDTO);

            Assert.True(result.Result!.Count > 0);
            Assert.True(result.Total > 0);
        }
    }
}



