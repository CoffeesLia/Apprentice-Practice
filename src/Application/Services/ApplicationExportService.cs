using System.Globalization;
using System.Text;
using Microsoft.Extensions.Localization;
using Stellantis.ProjectName.Application.Models.Filters;
using Stellantis.ProjectName.Application.Resources;
using Stellantis.ProjectName.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Stellantis.ProjectName.Domain.Entities;

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
        public async Task<byte[]> ExportApplicationAsync(int id)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var app = await _unitOfWork.ApplicationDataRepository
                .GetFullByIdAsync(id)
                .ConfigureAwait(false);

            if (app == null)
                throw new KeyNotFoundException($"Application with Id {id} not found");

            var primaryColor = Colors.Blue.Darken2;
            var headerBgColor = Colors.Grey.Lighten3;
            var evenRowColor = Colors.Grey.Lighten4;
            var oddRowColor = Colors.White;
            var textColor = Color.FromHex("#444444");
            var logoPath = @"C:\Users\SE68087\gitlab\api\src\Application\Services\logo.png";

            // Buscar membros do squad (se houver)
            List<Member> squadMembers = new();
            if (app.Squad != null)
            {
                var squadMembersResult = await _unitOfWork.MemberRepository.GetListAsync(
                    new MemberFilter { SquadId = app.Squad.Id }
                );
                squadMembers = squadMembersResult.Result.ToList();
            }

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(TextStyle.Default.FontFamily("Segoe UI").FontColor(textColor).FontSize(11));

                    // Cabeçalho
                    page.Header().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.RelativeColumn().AlignMiddle()
                                .Text(app.Name ?? "Aplicação")
                                .FontSize(18).SemiBold().FontColor(textColor);
                            row.ConstantColumn(120).AlignMiddle().Image(logoPath, ImageScaling.FitWidth);
                        });
                    });

                    // Conteúdo
                    page.Content().Column(content =>
                    {
                        // Bloco de dados principais
                        content.Item().Element(e =>
                        {
                            e.Background(headerBgColor).Padding(10).Column(col =>
                            {
                                col.Item().Text("Dados da Aplicação").SemiBold().FontSize(13).FontColor(textColor);
                                col.Item().PaddingTop(4).Row(row =>
                                {
                                    row.RelativeColumn().Text($"Área:").SemiBold();
                                    row.RelativeColumn().Text(app.Area?.Name ?? "-");
                                });
                                col.Item().Row(row =>
                                {
                                    row.RelativeColumn().Text($"Responsável:").SemiBold();
                                    row.RelativeColumn().Text(app.Responsible?.Name ?? "-");
                                });
                                col.Item().Row(row =>
                                {
                                    row.RelativeColumn().Text($"Squad:").SemiBold();
                                    row.RelativeColumn().Text(app.Squad?.Name ?? "-");
                                });
                                col.Item().Row(row =>
                                {
                                    row.RelativeColumn().Text($"Externo:").SemiBold();
                                    row.RelativeColumn().Text(app.External ? _localizer["Csv_External_Yes"].Value : _localizer["Csv_External_No"].Value);
                                });
                            });
                        });

                        // Descrição
                        if (!string.IsNullOrWhiteSpace(app.Description))
                        {
                            content.Item().PaddingTop(10).Element(e =>
                            {
                                e.Background(evenRowColor).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Descrição").SemiBold().FontSize(13).FontColor(textColor);
                                    col.Item().PaddingTop(4).Text(app.Description).FontColor(textColor);
                                });
                            });
                        }

                        // Integrações
                        if (app.Integration != null && app.Integration.Any())
                        {
                            content.Item().PaddingTop(10).Element(e =>
                            {
                                e.Background(oddRowColor).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Integrações").SemiBold().FontSize(13).FontColor(textColor);
                                    col.Item().PaddingTop(4).Table(table =>
                                    {
                                        table.ColumnsDefinition(c => c.RelativeColumn());
                                        int idx = 0;
                                        foreach (var integ in app.Integration)
                                        {
                                            var bg = idx % 2 == 0 ? evenRowColor : oddRowColor;
                                            table.Cell().Background(bg).Padding(5).Text(integ.Name).FontColor(textColor);
                                            idx++;
                                        }
                                    });
                                });
                            });
                        }

                        // Repositórios
                        if (app.Repos != null && app.Repos.Any())
                        {
                            content.Item().PaddingTop(10).Element(e =>
                            {
                                e.Background(evenRowColor).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Repositórios").SemiBold().FontSize(13).FontColor(textColor);
                                    col.Item().PaddingTop(4).Table(table =>
                                    {
                                        table.ColumnsDefinition(c => c.RelativeColumn());
                                        int idx = 0;
                                        foreach (var repo in app.Repos)
                                        {
                                            var bg = idx % 2 == 0 ? oddRowColor : evenRowColor;
                                            table.Cell().Background(bg).Padding(5).Text(repo.Name).FontColor(textColor);
                                            idx++;
                                        }
                                    });
                                });
                            });
                        }

                        // Membros do Squad
                        if (squadMembers.Any())
                        {
                            content.Item().PaddingTop(10).Element(e =>
                            {
                                e.Background(oddRowColor).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Membros do Squad").SemiBold().FontSize(13).FontColor(textColor);
                                    col.Item().PaddingTop(4).Table(table =>
                                    {
                                        table.ColumnsDefinition(c =>
                                        {
                                            c.RelativeColumn(2); // Nome
                                            c.RelativeColumn(3); // Email
                                        });
                                        int idx = 0;
                                        foreach (var member in squadMembers)
                                        {
                                            var bg = idx % 2 == 0 ? evenRowColor : oddRowColor;
                                            table.Cell().Background(bg).Padding(5).Text(member.Name).FontColor(textColor);
                                            table.Cell().Background(bg).Padding(5).Text(member.Email).FontColor(textColor);
                                            idx++;
                                        }
                                    });
                                });
                            });
                        }
                    });

                    // Rodapé
                    page.Footer().Row(row =>
                    {
                        row.RelativeColumn().AlignLeft().Text(t =>
                        {
                            t.Span("Gerado em: ").SemiBold();
                            t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture));
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