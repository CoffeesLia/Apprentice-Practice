using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.ViewModels;

namespace WebApi.Tests.Controllers
{
public class IntegrationControllerTests
    {
        private readonly Mock<IIntegrationService> _serviceMock;
        private readonly IntegrationControllerBase _controller;
        private readonly Mock<IStringLocalizerFactory> _localizerFactoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Fixture _fixture;

        public IntegrationControllerTests()
        {
            _serviceMock = new Mock<IIntegrationService>();
            _localizerFactoryMock = new Mock<IStringLocalizerFactory>();
            var localizer = new Mock<IStringLocalizer>();
            _localizerFactoryMock.Setup(f => f.Create(typeof(IntegrationResources))).Returns(localizer.Object);
            _mapperMock = new Mock<IMapper>();
            _controller = new IntegrationControllerBase(_serviceMock.Object, _mapperMock.Object, _localizerFactoryMock.Object);
            _fixture = new Fixture();
        }

      
    }
}

