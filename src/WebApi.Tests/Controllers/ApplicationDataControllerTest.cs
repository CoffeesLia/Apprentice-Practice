using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.Mapper;
using Stellantis.ProjectName.WebApi.ViewModels;
using WebApi.Tests.Helpers;

namespace WebApi.Tests.Controllers
{
    public class ApplicationDataControllerTest
    {
        private readonly Mock<IApplicationDataService> _serviceMock;
        private readonly ApplicationDataController _controller;
        private readonly Fixture _fixture = new();

        public ApplicationDataControllerTest()
        {
            _serviceMock = new Mock<IApplicationDataService>();
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfiguration.CreateMapper();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactor = LocalizerFactorHelper.Create();
            _controller = new ApplicationDataController(_serviceMock.Object, mapper, localizerFactor);

            _fixture.Behaviors
            .OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task GetListAsyncShouldReturnPagedResultVm()
        {
            // Arrange
            ApplicationDataFilterDto filterDto = _fixture.Create<ApplicationDataFilterDto>();
            PagedResult<ApplicationData> pagedResult = _fixture.Create<PagedResult<ApplicationData>>();
            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<ApplicationFilter>())).ReturnsAsync(pagedResult);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PagedResultVm<ApplicationVm>>(okResult.Value);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenApplicationDataIsValid()
        {
            // Arrange
            ApplicationDataDto applicationDataDto = new()
            {
                Name = "Valid Name",
                AreaId = 1,
                Description = "Description",
                External = true,
                ResponsibleId = 1
            };

            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<ApplicationData>())).ReturnsAsync(OperationResult.Complete(ServiceResources.RegisteredSuccessfully));

            // Act
            IActionResult result = await _controller.CreateAsync(applicationDataDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task GetAsyncReturnsApplicationVmWhenIdIsValid()
        {
            // Arrange
            ApplicationData applicationData = new("Name")
            {
                Id = 1,
                Name = "Test Application",
            };
            ApplicationVm applicationVm = new()
            {
                Id = 1,
                Name = "Test Application",
                Area = new AreaVm()
            };

            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync(applicationData);

            // Act
            ActionResult<ApplicationVm> result = await _controller.GetAsync(1);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
            ApplicationVm returnValue = Assert.IsType<ApplicationVm>(okResult.Value);
            Assert.Equal(applicationVm.Id, returnValue.Id);
            Assert.Equal(applicationVm.Name, returnValue.Name);
        }

        [Fact]
        public async Task GetAsyncReturnsNotFoundWhenIdIsInvalid()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync((ApplicationData?)null);

            // Act
            ActionResult<ApplicationVm> result = await _controller.GetAsync(1);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }



        [Fact]
        public async Task UpdateAsyncShouldReturnOkResultWhenApplicationDataIsValid()
        {
            // Arrange
            ApplicationDataDto applicationDataDto = new()
            {
                Name = "Valid Name",
                AreaId = 1,
                Description = "Description",
                External = true,
                ResponsibleId = 1
            };

            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<ApplicationData>())).ReturnsAsync(OperationResult.Complete(ServiceResources.UpdatedSuccessfully));

            // Act
            IActionResult result = await _controller.UpdateAsync(1, applicationDataDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsyncReturnsNoContentWhenDeletionIsSuccessful()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(service => service.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete());

            // Act
            IActionResult result = await _controller.DeleteAsync(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncReturnsNotFoundWhenItemDoesNotExist()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(service => service.DeleteAsync(id)).ReturnsAsync(OperationResult.NotFound(ServiceResources.NotFound));

            // Act
            IActionResult result = await _controller.DeleteAsync(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExportCsvShouldReturnFileContentResultWithCsvContentType()
        {
            // Arrange
            var filterDto = _fixture.Create<ApplicationDataFilterDto>();
            var expectedBytes = new byte[] { 1, 2, 3 };
            _serviceMock.Setup(s => s.ExportToCsvAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(expectedBytes);

            // Act
            var result = await _controller.ExportCsv(filterDto);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("text/csv", fileResult.ContentType);
            Assert.Equal(expectedBytes, fileResult.FileContents);
            Assert.Contains("applications_", fileResult.FileDownloadName, StringComparison.Ordinal);
            Assert.EndsWith(".csv", fileResult.FileDownloadName, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExportPdfShouldReturnFileContentResultWithPdfContentType()
        {
            // Arrange
            var filterDto = _fixture.Create<ApplicationDataFilterDto>();
            var expectedBytes = new byte[] { 4, 5, 6 };
            _serviceMock.Setup(s => s.ExportToPdfAsync(It.IsAny<ApplicationFilter>()))
                .ReturnsAsync(expectedBytes);

            // Act
            var result = await _controller.ExportPdf(filterDto);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal(expectedBytes, fileResult.FileContents);
            Assert.Contains("applications_", fileResult.FileDownloadName, StringComparison.Ordinal);
            Assert.EndsWith(".pdf", fileResult.FileDownloadName, StringComparison.Ordinal);
        }

        [Fact]
        public async Task ExportSinglePdfShouldReturnFileContentResultWithPdfContentTypeAndIdInFileName()
        {
            // Arrange
            int id = 42;
            var expectedBytes = new byte[] { 7, 8, 9 };
            _serviceMock.Setup(s => s.ExportApplicationAsync(id))
                .ReturnsAsync(expectedBytes);

            // Act
            var result = await _controller.ExportSinglePdf(id);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.Equal(expectedBytes, fileResult.FileContents);
            Assert.Contains($"application_{id}_", fileResult.FileDownloadName, StringComparison.Ordinal);
            Assert.EndsWith(".pdf", fileResult.FileDownloadName, StringComparison.Ordinal);
        }

    }
}

