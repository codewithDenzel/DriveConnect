using System;
using System.IO;
using System.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DriveConnect.winforms.Report;

public static class ManagementReportPdfService
{
    public static void Export(ManagementReportData report, string filePath)
    {
        Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontFamily("Lato").FontSize(9).FontColor("263238"));

                page.Header().Column(header =>
                {
                    header.Item().Text("DriveConnect CRM").FontSize(11).SemiBold().FontColor("64748B");
                    header.Item().Text(report.Title).FontSize(22).SemiBold().FontColor("4F46E5");
                    header.Item().Text($"{report.Period}  •  {report.Branch}").FontSize(9).FontColor("64748B");
                });

                page.Content().Column(column =>
                {
                    column.Spacing(14);

                    column.Item().Background("F8FAFC").Border(1).BorderColor("E2E8F0").Padding(12).Column(summary =>
                    {
                        summary.Item().Text("Executive Summary").FontSize(13).SemiBold().FontColor("334155");
                        summary.Item().PaddingTop(5).Text(report.ExecutiveSummary);
                    });

                    column.Item().Row(row =>
                    {
                        foreach (var metric in report.Metrics.Take(4))
                        {
                            row.RelativeItem().PaddingRight(6).Background("FFFFFF").Border(1).BorderColor("E2E8F0").Padding(10).Column(card =>
                            {
                                card.Item().Text(metric.Label).FontSize(8).FontColor("64748B");
                                card.Item().Text(metric.Value).FontSize(16).SemiBold().FontColor("4F46E5");
                                card.Item().Text(metric.Note).FontSize(7).FontColor("64748B");
                            });
                        }
                    });

                    foreach (var chart in report.Charts)
                    {
                        var bytes = ReportChartRenderer.ToPng(chart, 980, 330);
                        column.Item().Border(1).BorderColor("E2E8F0").Padding(10).Column(card =>
                        {
                            card.Item().Text(chart.Title).FontSize(12).SemiBold().FontColor("334155");
                            if (!string.IsNullOrWhiteSpace(chart.Subtitle))
                                card.Item().Text(chart.Subtitle).FontSize(8).FontColor("64748B");
                            card.Item().PaddingTop(6).Image(bytes).FitWidth();
                        });
                    }

                    if (report.Findings.Count > 0)
                    {
                        column.Item().Background("F8FAFC").Border(1).BorderColor("E2E8F0").Padding(12).Column(findings =>
                        {
                            findings.Item().Text("Key Findings").FontSize(13).SemiBold().FontColor("334155");
                            foreach (var finding in report.Findings)
                                findings.Item().PaddingTop(4).Text($"• {finding}");
                        });
                    }

                    if (report.DetailHeaders.Count > 0 && report.DetailRows.Count > 0)
                    {
                        column.Item().Text("Detailed Records").FontSize(13).SemiBold().FontColor("334155");
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                foreach (var _ in report.DetailHeaders.Take(8))
                                    columns.RelativeColumn();
                            });

                            foreach (var header in report.DetailHeaders.Take(8))
                                table.Header(headerCell => headerCell.Cell().Background("EEF2FF").Padding(5).Text(header).FontSize(7).SemiBold().FontColor("4338CA"));

                            foreach (var row in report.DetailRows)
                            {
                                foreach (var cell in row.Take(8))
                                    table.Cell().BorderBottom(1).BorderColor("E2E8F0").Padding(4).Text(cell ?? "").FontSize(7);
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("DriveConnect CRM  •  Page ");
                    text.CurrentPageNumber();
                });
            });
        }).GeneratePdf(filePath);
    }
}
