using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.ViewModels;
using System.Threading.Tasks;
using Xunit;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace Stellantis.ProjectName.WebApi.Tests.Controllers
{
    public class ResponsibleControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IResponsibleService> _serviceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly ResponsibleController _controller;

        public ResponsibleControllerTests()
        {
            _fixture = new Fixture();
            _serviceMock = _fixture.Freeze<Mock<IResponsibleService>>();
            _mapperMock = _fixture.Freeze<Mock<IMapper>>();
            _localizerFactoryMock = _fixture.Freeze<Mock<IStringLocalizerFactory>>();
            _controller = new ResponsibleController(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
        }


    }
}