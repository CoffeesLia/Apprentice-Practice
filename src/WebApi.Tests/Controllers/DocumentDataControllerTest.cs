using AutoFixture;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stellantis.ProjectName.Application.Interfaces.Services;
using Stellantis.ProjectName.Application.Models;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Domain.Entities;
using Stellantis.ProjectName.WebApi.Controllers;
using Stellantis.ProjectName.WebApi.Dto;
using Stellantis.ProjectName.WebApi.Dto.Filters;
using Stellantis.ProjectName.WebApi.Mapper;
using Stellantis.ProjectName.WebApi.ViewModels;
using WebApi.Tests.Helpers;

namespace WebApi.Tests.Controllers
{
    public class DocumentDataControllerTest
    {
        private readonly Mock<IDocumentService> _serviceMock;
        private readonly DocumentDataController _controller;
        private readonly Fixture _fixture = new();

        public DocumentDataControllerTest()
        {
            _serviceMock = new Mock<IDocumentService>();
            MapperConfiguration mapperConfiguration = new(x => { x.AddProfile<AutoMapperProfile>(); });
            IMapper mapper = mapperConfiguration.CreateMapper();
            Microsoft.Extensions.Localization.IStringLocalizerFactory localizerFactor = LocalizerFactorHelper.Create();
            _controller = new DocumentDataController(_serviceMock.Object, mapper, localizerFactor);

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
            DocumentDataFilterDto filterDto = _fixture.Create<DocumentDataFilterDto>();
            PagedResult<DocumentData> pagedResult = _fixture.Create<PagedResult<DocumentData>>();
            _serviceMock.Setup(s => s.GetListAsync(It.IsAny<DocumentDataFilter>())).ReturnsAsync(pagedResult);

            // Act
            IActionResult result = await _controller.GetListAsync(filterDto);

            // Assert
            OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<PagedResultVm<DocumentVm>>(okResult.Value);
        }

        [Fact]
        public async Task GetAsyncReturnsDocumentVmWhenIdIsValid()
        {
            // Arrange  
            var document = _fixture.Build<DocumentData>()
                .With(d => d.Id, 1)
                .With(d => d.Name, "Doc1")
                .With(d => d.Url, new Uri("https://example.com/doc1"))
                .With(d => d.ApplicationId, 1)
                .Create();

            var expectedVm = new DocumentVm
            {
                Id = document.Id,
                Name = document.Name,
                Url = document.Url,
                ApplicationId = document.ApplicationId,
                ApplicationData = new ApplicationVm
                {
                    Id = 1,
                    Name = "App1",
                    Area = new AreaVm(), // Assuming AreaVm is properly initialized elsewhere  
                    External = false
                }
            };

            _serviceMock.Setup(s => s.GetItemAsync(1)).ReturnsAsync(document);

            // Act  
            var result = await _controller.GetAsync(1);

            // Assert  
            var okResult = Assert.IsType<OkObjectResult>(result);
            var vm = Assert.IsType<DocumentVm>(okResult.Value);

            Assert.Equal(expectedVm.Id, vm.Id);
            Assert.Equal(expectedVm.Name, vm.Name);
            Assert.Equal(expectedVm.Url, vm.Url);
            Assert.Equal(expectedVm.ApplicationId, vm.ApplicationId);
        }

        [Fact]
        public async Task GetAsyncReturnsNotFoundWhenIdIsInvalid()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetItemAsync(It.IsAny<int>())).ReturnsAsync((DocumentData?)null);

            // Act
            var result = await _controller.GetAsync(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateAsyncShouldReturnCreatedAtActionResultWhenDocumentIsValid()
        {
            // Arrange
            var dto = new DocumentDto
            {
                Name = "Doc2",
                Url = new Uri("https://example.com/doc2"),
                ApplicationId = 2,
            };



            _serviceMock.Setup(s => s.CreateAsync(It.IsAny<DocumentData>()))
                .ReturnsAsync(OperationResult.Complete("Registered successfully"));

            // Act
            var result = await _controller.CreateAsync(dto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task UpdateAsyncShouldReturnOkResultWhenDocumentIsValid()
        {
            // Arrange
            var dto = new DocumentDto
            {
                Name = "Doc3",
                Url = new Uri("https://example.com/doc3"),
                ApplicationId = 3
            };

            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<DocumentData>()))
                .ReturnsAsync(OperationResult.Complete("Updated successfully"));

            // Act
            var result = await _controller.UpdateAsync(3, dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task DeleteAsyncReturnsNoContentWhenDeletionIsSuccessful()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(service => service.DeleteAsync(id)).ReturnsAsync(OperationResult.Complete());

            // Act
            var result = await _controller.DeleteAsync(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteAsyncReturnsNotFoundWhenItemDoesNotExist()
        {
            // Arrange
            int id = 1;
            _serviceMock.Setup(service => service.DeleteAsync(id)).ReturnsAsync(OperationResult.NotFound("Not found"));

            // Act
            var result = await _controller.DeleteAsync(id);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
