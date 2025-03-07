using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;
using WebApi.Tests.Helpers;


namespace WebApi.Tests.Controllers
{
    public class ResponsibleControllerBaseTests
    {
        private readonly Mock<IAreaService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly Mock<IStringLocalizer> _localizerMock;
        private readonly AreaControllerBase _controller;

        public ResponsibleControllerBaseTests()
        {
            _serviceMock = new Mock<IAreaService>();
            _mapperMock = new Mock<IMapper>();
            var localizer = LocalizerFactorHelper.Create();

            _controller = new AreaControllerBase(_serviceMock.Object, _mapperMock.Object, localizer);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnBadRequest_WhenItemDtoIsNull()
        {
            // Act
            var result = await _controller.CreateAsync(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var message = ((dynamic)badRequestResult.Value)?.Message;
            Assert.Equal(_localizerMock.Object["NameIsRequired"].Value, message);
        }




    }
}