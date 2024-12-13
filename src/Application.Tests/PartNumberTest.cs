using Application.Services;
using AutoFixture;
using AutoMapper;
using Domain.DTO;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Resources;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Application.Tests
{
    public class PartNumberTest
    {
        [Fact]
        public async Task ValidPartNumber_CreateIsCalled_ReturnValidPartNumberDTO()
        {
            var partNumberDTO = new Fixture().Create<PartNumberDTO>();
            var partNumber = new Fixture().Create<PartNumber>();
            partNumber.Id = 0;

            var partNumberRepositoryMock = new Mock<IPartNumberRepository>();
            partNumberRepositoryMock.Setup(x => x.VerifyCodeExists(partNumberDTO.Code!)).Returns(false);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.PartNumberRepository).Returns(partNumberRepositoryMock.Object);

            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<PartNumber>(It.IsAny<PartNumberDTO>())).Returns(partNumber);

            var localizer = new Mock<IStringLocalizer<Messages>>();

            var partNumberServiceMock = new PartNumberService(mapper.Object, unitOfWork.Object, localizer.Object);
            await partNumberServiceMock.Create(partNumberDTO!);

            partNumberRepositoryMock.Verify(x => x.Create(It.IsAny<PartNumber>(), true), Times.Once);

        }

        [Fact]
        public async Task InvalidPartNumber_CreateIsCalled_ReturnInValidPartNumberDTO()
        {
            var partNumberDTO = new Fixture().Create<PartNumberDTO>();
            var partNumber = new Fixture().Create<PartNumber>();
            partNumber.Id = 0;

            var partNumberRepositoryMock = new Mock<IPartNumberRepository>();
            partNumberRepositoryMock.Setup(x => x.VerifyCodeExists(partNumberDTO.Code!)).Returns(true);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.PartNumberRepository).Returns(partNumberRepositoryMock.Object);

            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<PartNumber>(It.IsAny<PartNumberDTO>())).Returns(partNumber);

            var localizer = new Mock<IStringLocalizer<Messages>>();
            localizer.Setup(x => x["AlreadyExistCode"]).Returns(new LocalizedString("AlreadyExistCode", "Code already exists"));

            var partNumberService = new PartNumberService(mapper.Object, unitOfWork.Object, localizer.Object);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await partNumberService.Create(partNumberDTO));

            Assert.Equal("Code already exists", exception.Message);
        }

        [Fact]
        public async Task InvalidPartNumber_UpdateIsCalled_ReturnInValidPartNumberDTO()
        {
            var partNumberDTO = new Fixture().Create<PartNumberDTO>();
            var partNumber = new Fixture().Create<PartNumber>();

            var partNumberRepositoryMock = new Mock<IPartNumberRepository>();
            partNumberRepositoryMock.Setup(x => x.VerifyCodeExists(partNumberDTO.Code!)).Returns(true);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.PartNumberRepository).Returns(partNumberRepositoryMock.Object);

            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<PartNumber>(It.IsAny<PartNumberDTO>())).Returns(partNumber);

            var localizer = new Mock<IStringLocalizer<Messages>>();
            localizer.Setup(x => x["AlreadyExistCode"]).Returns(new LocalizedString("AlreadyExistCode", "Code already exists"));

            var partNumberService = new PartNumberService(mapper.Object, unitOfWork.Object, localizer.Object);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await partNumberService.Update(partNumberDTO));

            Assert.Equal("Code already exists", exception.Message);
        }

        [Fact]
        public async Task ValidPartNumber_UpdateIsCalled_ReturnValidPartNumberDTO()
        {
            var partNumberDTO = new Fixture().Create<PartNumberDTO>();
            var partNumber = new Fixture().Create<PartNumber>();

            var partNumberRepositoryMock = new Mock<IPartNumberRepository>();
            partNumberRepositoryMock.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync(partNumber);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.PartNumberRepository).Returns(partNumberRepositoryMock.Object);

            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<PartNumber>(It.IsAny<PartNumberDTO>())).Returns(partNumber);

            var localizer = new Mock<IStringLocalizer<Messages>>();

            var partNumberServiceMock = new PartNumberService(mapper.Object, unitOfWork.Object, localizer.Object);
            await partNumberServiceMock.Update(partNumberDTO);

            partNumberRepositoryMock.Verify(x => x.Update(It.IsAny<PartNumber>(), true), Times.Once);
        }

        [Fact]
        public async Task NotFoundPartNumber_UpdateIsCalled_ReturnNotFoundPartNumberDTO()
        {

            var partNumberDTO = new Fixture().Create<PartNumberDTO>();
            var partNumber = new Fixture().Create<PartNumber>();

            var partNumberRepositoryMock = new Mock<IPartNumberRepository>();
            partNumberRepositoryMock.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync((PartNumber)null!);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(x => x.PartNumberRepository).Returns(partNumberRepositoryMock.Object);

            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<PartNumber>(It.IsAny<PartNumberDTO>())).Returns(partNumber);

            var localizer = new Mock<IStringLocalizer<Messages>>();
            localizer.Setup(x => x["NotFound"]).Returns(new LocalizedString("NotFound", "Not found"));

            var partNumberServiceMock = new PartNumberService(mapper.Object, unitOfWork.Object, localizer.Object);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await partNumberServiceMock.Update(partNumberDTO));

            Assert.Equal("Not found", exception.Message);
        }

        [Fact]
        public async Task ReturnPartNumber_GetIsCalled_ReturnPartNumberDTO()
        {
            var partNumberDTO = new Fixture().Create<PartNumberDTO>();
            partNumberDTO.Type = PartNumberType.Interno;
            var partNumber = new PartNumber(partNumberDTO.Code!, partNumberDTO.Description!, 1);
            var id = new Fixture().Create<int>();

            var partNumberRepositoryMock = new Mock<IPartNumberRepository>();
            partNumberRepositoryMock.Setup(x => x.GetById(It.IsAny<int>())).ReturnsAsync(partNumber);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.PartNumberRepository).Returns(partNumberRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<PartNumberDTO>(It.IsAny<PartNumber>())).Returns(partNumberDTO);

            var localizerMock = new Mock<IStringLocalizer<Messages>>();

            var partNumberService = new PartNumberService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);
            var result = await partNumberService.Get(id);

            Assert.Equal(partNumberDTO.Code, result.Code);
            Assert.Equal(partNumberDTO.Description, result.Description);
            Assert.Equal(partNumberDTO.Type, result.Type);
        }

        [Fact]
        public async Task ReturnPartNumber_GetAllIsCalled_ReturnListPartNumberDTO()
        {
            var partNumberFilterDTO = new Fixture().Create<PartNumberFilterDTO>();
            var paginationPartNumber = new Fixture().Create<PaginationDTO<PartNumber>>();
            var paginationPartNumberDTO = new PaginationDTO<Domain.DTO.PartNumberDTO>()
            {
                Result = new Fixture().Create<List<PartNumberDTO>>(),
                Total = paginationPartNumber.Total
            };

            var partNumberRepositoryMock = new Mock<IPartNumberRepository>();
            partNumberRepositoryMock.Setup(x => x.GetListFilter(It.IsAny<PartNumberFilterDTO>())).ReturnsAsync(paginationPartNumber);

            var unitOfWorkMock = new Mock<IUnitOfWork>();
            unitOfWorkMock.SetupGet(x => x.PartNumberRepository).Returns(partNumberRepositoryMock.Object);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(x => x.Map<PaginationDTO<PartNumberDTO>>(It.IsAny<PaginationDTO<PartNumber>>())).Returns(paginationPartNumberDTO);

            var localizerMock = new Mock<IStringLocalizer<Messages>>();

            var partNumberService = new PartNumberService(mapperMock.Object, unitOfWorkMock.Object, localizerMock.Object);
            var result = await partNumberService.GetList(partNumberFilterDTO);

            Assert.True(result.Result!.Count > 0);
            Assert.True(result.Total > 0);

        }
    }
}
