using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;

namespace WebApi.Tests.Controllers
{
    public class SupplierControllerTests : EntityControllerTestsBase<SupplierController, ISupplierService, SupplierDto, Supplier>
    {
        protected override SupplierController CreateController()
        {
            return new SupplierController(MapperMock.Object, ServiceMock.Object, LocalizerFactor);
        }

        /// Given a supplier filter,
        /// when GetListAsync is called,
        /// then it should return Ok with the list of supplier.
        [Fact]
        public async Task GetListAsync_Success()
        {
            // Arrange
            var pagedResultDto = Fixture.Create<PagedResultDto<SupplierDto>>();
            var pagedResult = Fixture.Create<PagedResult<Supplier>>();

            var filterDto = Fixture.Create<SupplierFilterDto>();
            ServiceMock
                .Setup(s => s.GetListAsync(It.IsAny<SupplierFilter>()))
                .ReturnsAsync(pagedResult);
            MapperMock
                .Setup(m => m.Map<PagedResultDto<SupplierDto>>(pagedResult))
                .Returns(pagedResultDto);

            // Act
            var result = await Controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pagedResultDto, okResult.Value);
        }
    }
}
