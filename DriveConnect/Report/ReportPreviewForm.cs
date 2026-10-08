using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DriveConnect.winforms.Report;

public sealed class ReportPreviewForm : Form
{
    private readonly ManagementReportData _report;

    public ReportPreviewForm(ManagementReportData report)
    {
        _report = report;
        Text = report.Title;
        ClientSize = new Size(1180, 820);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Color.FromArgb(243, 244, 246);
        MinimizeBox = true;
        MaximizeBox = true;
        BuildUi();
    }

    private void BuildUi()
    {
        var top = new Panel
        {
            Dock = DockStyle.Top,
            Height = 72,
            BackColor = Color.White,
            Padding = new Padding(18, 10, 18, 10)
        };
        top.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, top.ClientRectangle, Color.FromArgb(229, 231, 235), ButtonBorderStyle.Solid);

        var title = new Label
        {
            Text = _report.Title,
            Location = new Point(18, 10),
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(17, 24, 39)
        };
        var subtitle = new Label
        {
            Text = $"{_report.Period}  •  {_report.Branch}",
            Location = new Point(20, 42),
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Color.FromArgb(107, 114, 128)
        };

        var export = new Button
        {
            Text = "Export PDF",
            Width = 125,
            Height = 36,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(ClientSize.Width - 160, 18),
            BackColor = Color.FromArgb(79, 70, 229),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 9F),
            Cursor = Cursors.Hand
        };
        export.FlatAppearance.BorderSize = 0;
        export.Click += Export_Click;

        top.Controls.Add(title);
        top.Controls.Add(subtitle);
        top.Controls.Add(export);

        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(14),
            BackColor = Color.FromArgb(243, 244, 246)
        };

        var content = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        content.Controls.Add(CreateSummaryPanel());
        content.Controls.Add(CreateMetricPanel());

        foreach (var chart in _report.Charts)
            content.Controls.Add(CreateChartPanel(chart));

        if (_report.Findings.Count > 0)
            content.Controls.Add(CreateFindingsPanel());

        if (_report.DetailHeaders.Count > 0)
            content.Controls.Add(CreateDetailPanel());

        scroll.Controls.Add(content);
        Controls.Add(scroll);
        Controls.Add(top);
    }

    private Panel CreateSummaryPanel()
    {
        var panel = CreateCard(new Size(1100, 115));
        var title = new Label { Text = "Executive Summary", AutoSize = true, Location = new Point(18, 15), Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(67, 56, 202) };
        var text = new Label { Text = _report.ExecutiveSummary, Location = new Point(18, 47), Size = new Size(1050, 55), Font = new Font("Segoe UI", 9.5F), ForeColor = Color.FromArgb(55, 65, 81), AutoEllipsis = false };
        panel.Controls.Add(title); panel.Controls.Add(text);
        return panel;
    }

    private Panel CreateMetricPanel()
    {
        var panel = CreateCard(new Size(1100, 100));
        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0), Margin = new Padding(0) };
        foreach (var metric in _report.Metrics.Take(5))
        {
            var card = new Panel { Width = 205, Height = 76, Margin = new Padding(5), BackColor = Color.FromArgb(248, 250, 252), Padding = new Padding(10) };
            card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Color.FromArgb(226, 232, 240), ButtonBorderStyle.Solid);
            card.Controls.Add(new Label { Text = metric.Label, AutoSize = true, Location = new Point(10, 8), Font = new Font("Segoe UI", 8F), ForeColor = Color.FromArgb(100, 116, 139) });
            card.Controls.Add(new Label { Text = metric.Value, AutoSize = true, Location = new Point(10, 27), Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold), ForeColor = Color.FromArgb(79, 70, 229) });
            card.Controls.Add(new Label { Text = metric.Note, AutoSize = true, Location = new Point(10, 53), Font = new Font("Segoe UI", 7F), ForeColor = Color.FromArgb(100, 116, 139) });
            flow.Controls.Add(card);
        }
        panel.Controls.Add(flow);
        return panel;
    }

    private Panel CreateChartPanel(ManagementReportChart chart)
    {
        var panel = CreateCard(new Size(1100, 390));
        var title = new Label { Text = chart.Title, AutoSize = true, Location = new Point(18, 14), Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(67, 56, 202) };
        var subtitle = new Label { Text = chart.Subtitle, AutoSize = true, Location = new Point(18, 40), Font = new Font("Segoe UI", 8.5F), ForeColor = Color.FromArgb(107, 114, 128) };
        var picture = new PictureBox { Location = new Point(18, 68), Size = new Size(1050, 300), SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White };
        picture.Image = ReportChartRenderer.Render(chart, 1050, 300);
        panel.Controls.Add(title); panel.Controls.Add(subtitle); panel.Controls.Add(picture);
        return panel;
    }

    private Panel CreateFindingsPanel()
    {
        var height = Math.Max(120, 58 + _report.Findings.Count * 28);
        var panel = CreateCard(new Size(1100, height));
        panel.Controls.Add(new Label { Text = "Key Findings", AutoSize = true, Location = new Point(18, 14), Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(67, 56, 202) });
        int y = 48;
        foreach (var finding in _report.Findings)
        {
            panel.Controls.Add(new Label { Text = "• " + finding, AutoSize = false, Size = new Size(1040, 24), Location = new Point(20, y), Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(55, 65, 81) });
            y += 28;
        }
        return panel;
    }

    private Panel CreateDetailPanel()
    {
        var panel = CreateCard(new Size(1100, 390));
        panel.Controls.Add(new Label { Text = "Detailed Records", AutoSize = true, Location = new Point(18, 14), Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold), ForeColor = Color.FromArgb(67, 56, 202) });

        var grid = new DataGridView
        {
            Location = new Point(18, 48),
            Size = new Size(1050, 320),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            ReadOnly = true,
            AllowUserToAddRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };

        foreach (var header in _report.DetailHeaders)
            grid.Columns.Add(header, header);

        foreach (var row in _report.DetailRows)
            grid.Rows.Add(row.Take(_report.DetailHeaders.Count).Select(x => (object)(x ?? string.Empty)).ToArray());

        panel.Controls.Add(grid);
        return panel;
    }

    private static Panel CreateCard(Size size)
    {
        var panel = new Panel { Size = size, Margin = new Padding(0, 0, 0, 12), BackColor = Color.White, Padding = new Padding(0) };
        panel.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, panel.ClientRectangle, Color.FromArgb(229, 231, 235), ButtonBorderStyle.Solid);
        return panel;
    }

    private void Export_Click(object? sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "PDF files (*.pdf)|*.pdf",
            FileName = $"DriveConnect_{_report.Title.Replace(' ', '_')}_{DateTime.Now:yyyyMMdd_HHmm}.pdf",
            Title = "Export DriveConnect Report"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            ManagementReportPdfService.Export(_report, dialog.FileName);
            MessageBox.Show("The management report was exported successfully.", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"The report could not be exported.\n\n{ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
