using AutoFixture;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;

namespace WebApi.Tests.Controllers
{
    public class PartNumberControllerTests
        : EntityControllerTestsBase<PartNumberController, IPartNumberService, PartNumberDto, PartNumber>
    {
        protected override PartNumberController CreateController()
        {
            return new PartNumberController(Mapper, ServiceMock.Object, LocalizerFactor);
        }

        /// Given a part number filter,
        /// when GetListAsync is called,
        /// then it should return Ok with the list of part numbers.
        [Fact]
        public async Task GetListAsync_Success()
        {
            // Arrange
            var pagedResult = Fixture.Create<PagedResult<PartNumber>>();
            var expect = Mapper.Map<PagedResultDto<PartNumberDto>>(pagedResult);
            var filterDto = Fixture.Create<PartNumberFilterDto>();
            ServiceMock
                .Setup(s => s.GetListAysnc(It.IsAny<PartNumberFilter>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await Controller.GetListAsync(filterDto);

            // Assert
            AssertResultIsOkAndValueIsEqual(result, expect);
        }
    }
}
