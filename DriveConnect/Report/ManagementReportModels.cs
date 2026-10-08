using System;
using System.Collections.Generic;

namespace DriveConnect.winforms.Report;

public sealed class ManagementReportData
{
    public string Title { get; init; } = "DriveConnect Management Report";
    public string Period { get; init; } = "";
    public string Branch { get; init; } = "All Branches";
    public string ExecutiveSummary { get; init; } = "";
    public List<ManagementReportMetric> Metrics { get; init; } = new();
    public List<ManagementReportChart> Charts { get; init; } = new();
    public List<string> Findings { get; init; } = new();
    public List<string> DetailHeaders { get; init; } = new();
    public List<List<string>> DetailRows { get; init; } = new();
}

public sealed record ManagementReportMetric(string Label, string Value, string Note);

public enum ManagementReportChartType
{
    Bar,
    HorizontalBar,
    Line,
    Pie,
    Funnel
}

public sealed class ManagementReportChart
{
    public string Title { get; init; } = "Chart";
    public string Subtitle { get; init; } = "";
    public ManagementReportChartType Type { get; init; }
    public bool Currency { get; init; }
    public List<KeyValuePair<string, decimal>> Data { get; init; } = new();
}
