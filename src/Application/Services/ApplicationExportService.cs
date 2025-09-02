using System.Globalization;
using System.Text;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Stellantis.ProjectName.Application.Services
{
    public class ApplicationExportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStringLocalizer _localizer;

        public ApplicationExportService(IUnitOfWork unitOfWork, IStringLocalizerFactory localizerFactory)
        {
            _unitOfWork = unitOfWork;
            _localizer = localizerFactory.Create(typeof(ApplicationDataResources));
        }

        private static string CsvSafe(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }

        public async Task<byte[]> ExportToCsvAsync(ApplicationFilter filter)
        {
            var applications = await _unitOfWork.ApplicationDataRepository.GetListAsync(filter).ConfigureAwait(false);

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",",
               _localizer["Csv_Id"].Value,
               _localizer["Csv_Name"].Value,
               _localizer["Csv_Area"].Value,
               _localizer["Csv_Responsible"].Value,
               _localizer["Csv_Squad"].Value,
               _localizer["Csv_External"].Value
            ));
            foreach (var app in applications.Result)
            {
                sb.AppendLine(string.Join(",",
                    app.Id,
                    CsvSafe(app.Name),
                    CsvSafe(app.Area?.Name),
                    CsvSafe(app.Responsible?.Name),
                    CsvSafe(app.Squad?.Name),
                    app.External ? _localizer["Csv_External_Yes"].Value : _localizer["Csv_External_No"].Value));
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        public async Task<byte[]> ExportToPdfAsync(ApplicationFilter filter)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var applications = await _unitOfWork.ApplicationDataRepository
                .GetListAsync(filter)
                .ConfigureAwait(false);

            var primaryColor = Colors.Blue.Darken2;
            var headerBgColor = Colors.Grey.Lighten3;
            var evenRowColor = Colors.Grey.Lighten4;
            var oddRowColor = Colors.White;
            var textColor = Color.FromHex("#444444");
            var logoPath = @"C:\Users\SE68087\gitlab\api\src\Application\Services\logo.png"; // ajuste conforme necessário

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(TextStyle.Default.FontFamily("Segoe UI").FontColor(textColor).FontSize(11));

                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.RelativeColumn().AlignMiddle().Text(_localizer["ApplicationsList"].Value ?? "Lista de Aplicações")
                                .FontSize(18).SemiBold().FontColor(textColor); // Título na cor do texto
                            row.ConstantColumn(120).AlignMiddle().Image(logoPath, ImageScaling.FitWidth);
                        });
                    });

                    page.Content().Column(content =>
                    {
                        content.Item().Row(row =>
                        {
                            row.RelativeColumn().Text($"Total: {applications.Result.Count()} aplicações")
                                .FontSize(11).FontColor(textColor);
                        });

                        content.Item().PaddingVertical(8);

                        content.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);  // Id
                                columns.RelativeColumn(2);   // Nome
                                columns.RelativeColumn(2);   // Área
                                columns.RelativeColumn(2);   // Responsável
                                columns.RelativeColumn(2);   // Squad
                                columns.ConstantColumn(70);  // Externo
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(headerBgColor).Padding(6)
                                    .Text(_localizer["Csv_Id"].Value).SemiBold().FontColor(textColor).FontSize(11);
                                header.Cell().Background(headerBgColor).Padding(6)
                                    .Text(_localizer["Csv_Name"].Value).SemiBold().FontColor(textColor).FontSize(11);
                                header.Cell().Background(headerBgColor).Padding(6)
                                    .Text(_localizer["Csv_Area"].Value).SemiBold().FontColor(textColor).FontSize(11);
                                header.Cell().Background(headerBgColor).Padding(6)
                                    .Text(_localizer["Csv_Responsible"].Value).SemiBold().FontColor(textColor).FontSize(11);
                                header.Cell().Background(headerBgColor).Padding(6)
                                    .Text(_localizer["Csv_Squad"].Value).SemiBold().FontColor(textColor).FontSize(11);
                                header.Cell().Background(headerBgColor).Padding(6)
                                    .Text(_localizer["Csv_External"].Value).SemiBold().FontColor(textColor).FontSize(11);
                            });

                            int rowIndex = 0;
                            foreach (var app in applications.Result)
                            {
                                var bg = rowIndex % 2 == 0 ? evenRowColor : oddRowColor;

                                table.Cell().Background(bg).Padding(5).Text(app.Id.ToString()).FontSize(11).FontColor(textColor);
                                table.Cell().Background(bg).Padding(5).Text(app.Name ?? "").FontSize(11).FontColor(textColor);
                                table.Cell().Background(bg).Padding(5).Text(app.Area?.Name ?? "").FontSize(11).FontColor(textColor);
                                table.Cell().Background(bg).Padding(5).Text(app.Responsible?.Name ?? "").FontSize(11).FontColor(textColor);
                                table.Cell().Background(bg).Padding(5).Text(app.Squad?.Name ?? "").FontSize(11).FontColor(textColor);
                                table.Cell().Background(bg).Padding(5)
                                    .Text(app.External
                                        ? _localizer["Csv_External_Yes"].Value
                                        : _localizer["Csv_External_No"].Value)
                                    .FontSize(11).FontColor(textColor);

                                rowIndex++;
                            }
                        });

                        content.Item().PaddingVertical(8);
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeColumn().AlignLeft().Text(t =>
                        {
                            t.Span("Gerado em: ").SemiBold();
                            t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture)); // Dia/Mês/Ano
                        });

                        row.RelativeColumn().AlignRight().Text(t =>
                        {
                            t.Span("Página ").SemiBold();
                            t.CurrentPageNumber();
                            t.Span(" / ");
                            t.TotalPages();
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}