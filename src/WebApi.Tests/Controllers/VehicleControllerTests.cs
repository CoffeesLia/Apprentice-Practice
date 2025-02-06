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
    public class VehicleControllerTests : EntityControllerTestsBase<VehicleController, IVehicleService, VehicleDto, Vehicle>
    {
        protected override VehicleController CreateController()
        {
            return new VehicleController(MapperMock.Object, ServiceMock.Object, LocalizerFactor);
        }

        /// Given a vehicle filter,
        /// when GetListAsync is called,
        /// then it should return Ok with the list of vehicle.
        [Fact]
        public async Task GetListAsync_Success()
        {
            // Arrange
            var pagedResultDto = Fixture.Create<PagedResultDto<VehicleDto>>();
            var pagedResult = Fixture.Create<PagedResult<Vehicle>>();

            var filterDto = Fixture.Create<VehicleFilterDto>();
            ServiceMock
                .Setup(s => s.GetListAsync(It.IsAny<VehicleFilter>()))
                .ReturnsAsync(pagedResult);
            MapperMock
                .Setup(m => m.Map<PagedResultDto<VehicleDto>>(pagedResult))
                .Returns(pagedResultDto);

            // Act
            var result = await Controller.GetListAsync(filterDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(pagedResultDto, okResult.Value);
        }

    }
}
