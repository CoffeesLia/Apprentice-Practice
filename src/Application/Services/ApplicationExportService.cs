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
            ArgumentNullException.ThrowIfNull(localizerFactory);
            _unitOfWork = unitOfWork;
            _localizer = localizerFactory.Create(typeof(ApplicationDataResources));
        }

        internal static string CsvSafe(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";
            if (value.Contains(',', StringComparison.Ordinal) || value.Contains('"', StringComparison.Ordinal) || value.Contains('\n', StringComparison.Ordinal))
                return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
            return value;
        }
        
        public async Task<byte[]> ExportToCsvAsync(ApplicationFilter filter)
        {
            var applications = await _unitOfWork.ApplicationDataRepository.GetListAsync(filter).ConfigureAwait(false);

            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",",
               _localizer["Id"].Value,
               _localizer["Name"].Value,
               _localizer["rea"].Value,
               _localizer["Responsible"].Value,
               _localizer["Squad"].Value,
               _localizer["External"].Value
            ));
            foreach (var app in applications.Result)
            {
                sb.AppendLine(string.Join(",",
                    app.Id,
                    CsvSafe(app.Name),
                    CsvSafe(app.Area?.Name),
                    CsvSafe(app.Responsible?.Name),
                    CsvSafe(app.Squad?.Name),
                    app.External ? _localizer["External_Yes"].Value : _localizer["External_No"].Value));
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
            var logoPath = @"C:\Users\SE68087\gitlab\api\src\Application\Services\logo.png";

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(TextStyle.Default.FontFamily("Segoe UI").FontColor(textColor).FontSize(11));

                    // Cabeçalho (apenas na primeira página)
                    page.Header().ShowOnce().Element(header =>
                    {
                        header.Row(row =>
                        {
                            row.RelativeColumn().AlignMiddle().Text(_localizer["ApplicationsList"].Value ?? "Lista de Aplicações")
                                .FontSize(18).SemiBold().FontColor(textColor);

                            row.ConstantColumn(120).AlignMiddle().Image(logoPath, ImageScaling.FitWidth);
                        });
                    });

                    // Conteúdo principal
                    page.Content().Column(content =>
                    {
                        // --- Filtro de Período ---
                        if (filter.CreatedAfter.HasValue || filter.CreatedBefore.HasValue)
                        {
                            string periodo;
                            if (filter.CreatedAfter.HasValue && filter.CreatedBefore.HasValue)
                                periodo = string.Format(_localizer["PeriodRange"].Value, filter.CreatedAfter.Value.ToString("dd/MM/yyyy"), filter.CreatedBefore.Value.ToString("dd/MM/yyyy"));
                            else if (filter.CreatedAfter.HasValue)
                                periodo = string.Format(_localizer["FromDate"].Value, filter.CreatedAfter.Value.ToString("dd/MM/yyyy"));
                            else
                                periodo = string.Format(_localizer["ToDate"].Value, filter.CreatedBefore.Value.ToString("dd/MM/yyyy"));

                            content.Item().PaddingBottom(5)
                                .Text($"{_localizer["Period"].Value}: {periodo}")
                                .FontSize(11).FontColor(textColor).SemiBold();
                        }

                        // Total de aplicações
                        content.Item().Row(row =>
                        {
                            row.RelativeColumn().Text(
                                string.Format(CultureInfo.InvariantCulture, _localizer["TotalApplications"].Value, applications.Result.Count())
                            )
                            .FontSize(11).FontColor(textColor);
                        });

                        content.Item().PaddingVertical(8);

                        // Tabela de aplicações
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
                                    .Text(_localizer["Id"].Value).SemiBold().FontColor(textColor).FontSize(11);
                                header.Cell().Background(headerBgColor).Padding(6)
                                    .Text(_localizer["Name"].Value).SemiBold().FontColor(textColor).FontSize(11);
                                header.Cell().Background(headerBgColor).Padding(6)
                                    .Text(_localizer["Area"].Value).SemiBold().FontColor(textColor).FontSize(11);
                                header.Cell().Background(headerBgColor).Padding(6)
                                    .Text(_localizer["Responsible"].Value).SemiBold().FontColor(textColor).FontSize(11);
                                header.Cell().Background(headerBgColor).Padding(6)
                                    .Text(_localizer["Squad"].Value).SemiBold().FontColor(textColor).FontSize(11);
                                header.Cell().Background(headerBgColor).Padding(6)
                                    .Text(_localizer["External"].Value).SemiBold().FontColor(textColor).FontSize(11);
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
                                        ? _localizer["External_Yes"].Value
                                        : _localizer["External_No"].Value)
                                    .FontSize(11).FontColor(textColor);

                                rowIndex++;
                            }

                            // Rodapé
                            page.Footer().Row(row =>
                            {
                                row.RelativeColumn().AlignRight().Text(t =>
                                {
                                    t.Span(_localizer["Page"].Value + " ").SemiBold();
                                    t.CurrentPageNumber();
                                    t.Span(" / ");
                                    t.TotalPages();
                                });
                            });
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
            var evenRowColor = Colors.Grey.Lighten4; // Cinza claro
            var oddRowColor = Colors.Grey.Lighten2;  // Cinza um pouco mais escuro
            var textColor = Color.FromHex("#444444");
            var logoPath = @"C:\Users\SE68087\gitlab\api\src\Application\Services\logo.png";

            // Buscar membros do squad (se houver)
            List<Member> squadMembers = new();
            if (app.Squad != null)
            {
                var squadMembersResult = await _unitOfWork.MemberRepository.GetListAsync(
                    new MemberFilter { SquadId = app.Squad.Id }
                ).ConfigureAwait(false);
                squadMembers = squadMembersResult.Result.ToList();
            }

            // Buscar serviços da aplicação
            List<ServiceData> services = new();
            var serviceFilter = new ServiceDataFilter { ApplicationId = app.Id };
            var serviceResult = await _unitOfWork.ServiceDataRepository.GetListAsync(serviceFilter).ConfigureAwait(false);
            services = serviceResult.Result.ToList();

            // Buscar repositórios da aplicação
            List<Repo> repos = new();
            var repoFilter = new RepoFilter { ApplicationId = app.Id };
            var repoResult = await _unitOfWork.RepoRepository.GetListAsync(repoFilter).ConfigureAwait(false);
            repos = repoResult.Result.ToList();

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
                        content.Item().Text($"{_localizer["Area"].Value}: {app.Area?.Name ?? "-"}").FontSize(11).FontColor(textColor);
                        content.Item().Text($"{_localizer["Responsible"].Value}: {app.Responsible?.Name ?? "-"}").FontSize(11).FontColor(textColor);
                        content.Item().Text($"{_localizer["Squad"].Value}: {app.Squad?.Name ?? "-"}").FontSize(11).FontColor(textColor);
                        content.Item().Text($"{_localizer["External"].Value}: {(app.External ? _localizer["External_Yes"].Value : _localizer["External_No"].Value)}").FontSize(11).FontColor(textColor);

                        // Descrição
                        if (!string.IsNullOrWhiteSpace(app.Description))
                        {
                            content.Item().PaddingTop(10).Element(e =>
                            {
                                e.Column(col =>
                                {
                                    col.Item().Text(_localizer["Description"].Value).SemiBold().FontSize(13);
                                    col.Item().PaddingTop(4).Text(app.Description).FontColor(textColor);
                                });
                            });
                        }

                        // Integrações
                        if (app.Integration != null && app.Integration.Count != 0)
                        {
                            content.Item().PaddingTop(10).Element(e =>
                            {
                                e.Column(col =>
                                {
                                    col.Item().Text(_localizer["Integrations"].Value).SemiBold().FontSize(13).FontColor(textColor);
                                    col.Item().PaddingTop(4).Table(table =>
                                    {
                                        table.ColumnsDefinition(c => c.RelativeColumn());
                                        int idx = 0;
                                        foreach (var integ in app.Integration)
                                        {
                                            var bg = idx % 2 == 0 ? oddRowColor : evenRowColor;
                                            table.Cell().Background(bg).Padding(5).Text(integ.Name).FontColor(textColor);
                                            idx++;
                                        }
                                    });
                                });
                            });
                        }

                        // Repositórios
                        if (repos.Count != 0)
                        {
                            content.Item().PaddingTop(10).Element(e =>
                            {
                                e.Column(col =>
                                {
                                    col.Item().Text(_localizer["Repositories"].Value).SemiBold().FontSize(13).FontColor(textColor);
                                    col.Item().PaddingTop(4).Table(table =>
                                    {
                                        table.ColumnsDefinition(c => c.RelativeColumn());
                                        int idx = 0;
                                        foreach (var repo in repos)
                                        {
                                            var bg = idx % 2 == 0 ? oddRowColor : evenRowColor;
                                            table.Cell().Background(bg).Padding(5).Text(repo.Name).FontColor(textColor);
                                            idx++;
                                        }
                                    });
                                });
                            });
                        }

                        // Serviços
                        if (services.Any())
                        {
                            content.Item().PaddingTop(10).Element(e =>
                            {
                                e.Column(col =>
                                {
                                    col.Item().Text(_localizer["Services"].Value).SemiBold().FontSize(13).FontColor(textColor);
                                    col.Item().PaddingTop(4).Table(table =>
                                    {
                                        table.ColumnsDefinition(c => c.RelativeColumn());
                                        int idx = 0;
                                        foreach (var service in services)
                                        {
                                            var bg = idx % 2 == 0 ? oddRowColor : evenRowColor;
                                            table.Cell().Background(bg).Padding(5).Text(service.Name).FontColor(textColor);
                                            idx++;
                                        }
                                    });
                                });
                            });
                        }

                        // Membros do Squad
                        if (squadMembers.Count != 0)
                        {
                            content.Item().PaddingTop(10).Element(e =>
                            {
                                e.Column(col =>
                                {
                                    col.Item().Text(_localizer["SquadMembers"].Value).SemiBold().FontSize(13).FontColor(textColor);
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
                                            var bg = idx % 2 == 0 ? oddRowColor : evenRowColor;
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
                        row.RelativeColumn().AlignRight().Text(t =>
                        {
                            t.Span(_localizer["Page"].Value + " ").SemiBold();
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