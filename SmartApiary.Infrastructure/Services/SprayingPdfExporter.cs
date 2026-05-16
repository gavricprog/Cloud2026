using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartApiary.Domain.Entities;

namespace SmartApiary.Infrastructure.Services;

public static class SprayingPdfExporter
{
    public static byte[] Generate(IReadOnlyList<SprayingLog> logs, string title)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Header().Text(title).FontSize(18).Bold();
                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(1.5f);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Parcela");
                        header.Cell().Element(CellStyle).Text("Kultura");
                        header.Cell().Element(CellStyle).Text("Preparat");
                        header.Cell().Element(CellStyle).Text("Početak");
                        header.Cell().Element(CellStyle).Text("Kraj");
                        header.Cell().Element(CellStyle).Text("Vetar m/s");
                        header.Cell().Element(CellStyle).Text("Kiša mm");
                    });

                    foreach (var log in logs)
                    {
                        table.Cell().Element(CellStyle).Text(log.ParcelName);
                        table.Cell().Element(CellStyle).Text(log.CropType ?? "—");
                        table.Cell().Element(CellStyle).Text(log.SubstanceUsed ?? "—");
                        table.Cell().Element(CellStyle).Text(log.ActualStartTime.ToString("dd.MM.yyyy HH:mm"));
                        table.Cell().Element(CellStyle).Text(log.ActualEndTime.ToString("dd.MM.yyyy HH:mm"));
                        table.Cell().Element(CellStyle).Text(log.WindSpeed.ToString("F1"));
                        table.Cell().Element(CellStyle).Text(log.Precipitation.ToString("F1"));
                    }
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Smart Apiary — ");
                    text.Span(DateTime.UtcNow.ToString("dd.MM.yyyy HH:mm")).FontSize(9);
                });
            });
        }).GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container) =>
        container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
}
