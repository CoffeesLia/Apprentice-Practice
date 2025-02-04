using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Application.ViewModel;
using Stellantis.ProjectName.Application.ViewModel.Filters;
using Stellantis.ProjectName.Domain.Dto;
using Stellantis.ProjectName.WebApi.Controllers;

namespace WebApi.Tests
{
    public class SupplierControllerTests
    {
        private readonly IFixture _fixture;
        private readonly Mock<IMapper> _mapperMock;
        private readonly SupplierService service;
        //private readonly Mock<IStringLocalizer<SupplierResources>> _localizerMock;
        private readonly SupplierController _controller;

        public SupplierControllerTests()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());
            _mapperMock = _fixture.Freeze<Mock<IMapper>>();
            service = _fixture.Freeze<Mock<ISupplierService>>();
            //_localizerMock = _fixture.Freeze<Mock<IStringLocalizer<SupplierResources>>>();

            _controller = new SupplierController(_mapperMock.Object, service.Object, null);
        }

        [Theory, AutoData]
        public async Task GetList_ReturnOk_WithFilteredSuppliers(SupplierFilterVM filterVm, SupplierFilterDto filterDto, PaginationDto<SupplierDto> paginationDto, PaginationDto<SupplierVm> paginationVm)
        {
            // Arrange
            _mapperMock.Setup(m => m.Map<SupplierFilterDto>(filterVm)).Returns(filterDto);
            service.Setup(s => s.GetListAsync(filterDto)).ReturnsAsync(paginationDto);
            _mapperMock.Setup(m => m.Map<PaginationDto<SupplierVm>>(paginationDto)).Returns(paginationVm);

            // Act
            filterVm.Code = "Test";
            var result = await _controller.GetList(filterVm);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paginationVm, okResult.Value);
        }
    }
}
