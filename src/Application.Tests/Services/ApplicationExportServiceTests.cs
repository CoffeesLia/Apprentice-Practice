using System.Globalization;
using System.Text;
using Microsoft.Extensions.Localization;
using Moq;
using Stellantis.ProjectName.Application.Interfaces;
using Stellantis.ProjectName.Application.Interfaces.Repositories;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Services;
using Stellantis.ProjectName.Domain.Entities;
using UglyToad.PdfPig;
using Xunit;

namespace Application.Tests.Services
{
    public class ApplicationExportServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IApplicationDataRepository> _applicationDataRepositoryMock;
        private readonly ApplicationExportService _exportService;
        private readonly IStringLocalizerFactory _localizerFactory;

        public ApplicationExportServiceTest()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _applicationDataRepositoryMock = new Mock<IApplicationDataRepository>();
            _unitOfWorkMock.Setup(u => u.ApplicationDataRepository).Returns(_applicationDataRepositoryMock.Object);

            _localizerFactory = Helpers.LocalizerFactorHelper.Create();
            _exportService = new ApplicationExportService(_unitOfWorkMock.Object, _localizerFactory);
        }

        [Theory]
        [InlineData("App1", "App1")]
        [InlineData("App1,App2", "\"App1,App2\"")]
        [InlineData("App\"1", "\"App\"\"1\"")]
        [InlineData("App1\nApp2", "\"App1\nApp2\"")]
        [InlineData(null, "")]
        [InlineData("   ", "")]
        public void CsvSafeShouldEscapeValuesCorrectly(string? input, string expected)
        {
            var result = ApplicationExportService.CsvSafe(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task ExportToCsvAsyncShouldReturnCsvBytes()
        {
            // Arrange
            var filter = new ApplicationFilter();
            var applications = new PagedResult<ApplicationData>
            {
                Result = [
                    new ApplicationData("App1") { Id = 1, Area = new Area("Area1"), Responsible = new Responsible { Name = "Resp1", Email = "resp1@email.com", AreaId = 1 }, Squad = new Squad { Name = "Squad1" }, External = true },
                    new ApplicationData("App2") { Id = 2, Area = new Area("Area2"), Responsible = new Responsible { Name = "Resp1", Email = "resp1@email.com", AreaId = 1 }, Squad = new Squad { Name = "Squad2" }, External = false }
                ]
            };
            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(applications);

            // Act
            var result = await _exportService.ExportToCsvAsync(filter);

            // Assert
            Assert.NotNull(result);
            var csv = Encoding.UTF8.GetString(result);
            Assert.Contains("App1", csv);
            Assert.Contains("App2", csv);
            Assert.Contains("Id", csv);
        }
      
        [Fact]
        public async Task ExportToPdfAsyncShouldReturnPdfBytes()
        {
            // Arrange
            var filter = new ApplicationFilter();
            var applications = new PagedResult<ApplicationData>
            {
                Result = [
                    new ApplicationData("App1") { Id = 1, Area = new Area("Area1"), Responsible = new Responsible { Name = "Resp1", Email = "resp1@email.com", AreaId = 1 }, Squad = new Squad { Name = "Squad1" }, External = true }
                ]
            };
            _applicationDataRepositoryMock.Setup(r => r.GetListAsync(filter)).ReturnsAsync(applications);

            // Act
            var result = await _exportService.ExportToPdfAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("%PDF", Encoding.UTF8.GetString(result)[..4]);
        }

        [Fact]
        public async Task ExportApplicationAsyncShouldReturnPdfBytes()
        {
            // Arrange
            var app = new ApplicationData("App1")
            {
                Id = 1,
                Area = new Area("Area1"),
                Responsible = new Responsible { Name = "Resp1", Email = "resp1@email.com", AreaId = 1 },Squad = new Squad { Id = 1, Name = "Squad1" },
                External = true,
                Description = "Descrição"
            };
            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(app.Id)).ReturnsAsync(app);
            _unitOfWorkMock.Setup(u => u.MemberRepository.GetListAsync(It.IsAny<MemberFilter>()))
                .ReturnsAsync(new PagedResult<Member> { Result = [] });
            _unitOfWorkMock.Setup(u => u.ServiceDataRepository.GetListAsync(It.IsAny<ServiceDataFilter>()))
                .ReturnsAsync(new PagedResult<ServiceData> { Result = [] });
            _unitOfWorkMock.Setup(u => u.RepoRepository.GetListAsync(It.IsAny<RepoFilter>()))
                .ReturnsAsync(new PagedResult<Repo> { Result = [] });

            // Act
            var result = await _exportService.ExportApplicationAsync(app.Id);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("%PDF", Encoding.UTF8.GetString(result)[..4]);
        }

        [Fact]
        public async Task ExportApplicationAsyncShouldThrowKeyNotFoundExceptionWhenAppNotFound()
        {
            // Arrange
            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(It.IsAny<int>())).ReturnsAsync((ApplicationData?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _exportService.ExportApplicationAsync(999));
        }

        [Fact]
        public async Task ExportApplicationAsyncShouldReturnPdfBytesWithAllSections()
        {
            // Arrange
            var app = new ApplicationData("App1")
            {
                Id = 1,
                Area = new Area("Area1"),
                Responsible = new Responsible { Name = "Resp1", Email = "resp1@email.com", AreaId = 1 },
                Squad = new Squad { Id = 1, Name = "Squad1" },
                External = true,
                Description = "Descrição"
            };

            app.Integration.Add(new Integration { Name = "Integração 1" });
            app.Integration.Add(new Integration { Name = "Integração 2" });

            _applicationDataRepositoryMock.Setup(r => r.GetFullByIdAsync(app.Id)).ReturnsAsync(app);

            _unitOfWorkMock.Setup(u => u.MemberRepository.GetListAsync(It.IsAny<MemberFilter>()))
                .ReturnsAsync(new PagedResult<Member>
                {
                    Result = [
                        new Member
                {
                    Name = "Member 1",
                    Email = "member1@email.com",
                    Role = "Desenvolvedor",
                    Cost = 1000
                }
                    ]
                });

            _unitOfWorkMock.Setup(u => u.ServiceDataRepository.GetListAsync(It.IsAny<ServiceDataFilter>()))
                .ReturnsAsync(new PagedResult<ServiceData>
                {
                    Result = [new ServiceData { Name = "Service 1" }]
                });

            _unitOfWorkMock.Setup(u => u.RepoRepository.GetListAsync(It.IsAny<RepoFilter>()))
                .ReturnsAsync(new PagedResult<Repo>
                {
                    Result = [
                        new Repo
                {
                    Name = "Repo 1",
                    Description = "Descrição do Repo 1",
                    Url = new Uri("https://repo1.com")
                },
                new Repo
                {
                    Name = "Repo 2",
                    Description = "Descrição do Repo 2",
                    Url = new Uri("https://repo2.com")
                }
                    ]
                });

            // Act
            var result = await _exportService.ExportApplicationAsync(app.Id);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("%PDF", Encoding.UTF8.GetString(result)[..4]);

            string pdfText;
            using (var ms = new MemoryStream(result))
            using (var pdf = PdfDocument.Open(ms))
            {
                pdfText = string.Join("\n", pdf.GetPages().Select(p => p.Text));
            }

            Assert.Contains("Integração 1", pdfText);
            Assert.Contains("Repo 1", pdfText);
            Assert.Contains("Service 1", pdfText);
            Assert.Contains("Member 1", pdfText);
        }
    }
}