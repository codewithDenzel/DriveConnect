using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.IO;
using System.Drawing.Text;
using System.Windows.Forms;

namespace DriveConnect.winforms.Report;

public static class ReportChartRenderer
{
    private static readonly Color Purple = Color.FromArgb(124, 58, 237);
    private static readonly Color Purple2 = Color.FromArgb(139, 92, 246);
    private static readonly Color Cyan = Color.FromArgb(34, 211, 238);
    private static readonly Color Green = Color.FromArgb(16, 185, 129);
    private static readonly Color Red = Color.FromArgb(239, 68, 68);
    private static readonly Color Amber = Color.FromArgb(245, 158, 11);
    private static readonly Color Text = Color.FromArgb(55, 65, 81);
    private static readonly Color Muted = Color.FromArgb(107, 114, 128);
    private static readonly Color Grid = Color.FromArgb(229, 231, 235);

    public static Bitmap Render(ManagementReportChart chart, int width = 980, int height = 360)
    {
        var bitmap = new Bitmap(Math.Max(420, width), Math.Max(240, height));
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.Clear(Color.White);

        switch (chart.Type)
        {
            case ManagementReportChartType.Pie:
                DrawPie(g, chart.Data, bitmap.Size);
                break;
            case ManagementReportChartType.Line:
                DrawLine(g, chart.Data, chart.Currency, bitmap.Size);
                break;
            case ManagementReportChartType.Funnel:
                DrawFunnel(g, chart.Data, bitmap.Size);
                break;
            case ManagementReportChartType.HorizontalBar:
                DrawBars(g, chart.Data, chart.Currency, bitmap.Size, true);
                break;
            default:
                DrawBars(g, chart.Data, chart.Currency, bitmap.Size, false);
                break;
        }

        return bitmap;
    }

    public static byte[] ToPng(ManagementReportChart chart, int width = 980, int height = 360)
    {
        using var bitmap = Render(chart, width, height);
        using var stream = new MemoryStream();
        bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        return stream.ToArray();
    }

    private static void DrawBars(Graphics g, List<KeyValuePair<string, decimal>> data, bool currency, Size size, bool horizontal)
    {
        data = data ?? new List<KeyValuePair<string, decimal>>();
        if (data.Count == 0) { DrawNoData(g, size); return; }

        if (horizontal)
        {
            decimal max = Math.Max(1, data.Max(x => x.Value));
            int rowH = Math.Max(34, (size.Height - 35) / Math.Max(1, data.Count));
            int labelW = 190;
            int valueW = 100;
            int barArea = Math.Max(120, size.Width - labelW - valueW - 28);

            for (int i = 0; i < data.Count; i++)
            {
                int y = 15 + i * rowH;
                int w = (int)(barArea * (double)(data[i].Value / max));
                TextRenderer.DrawText(g, data[i].Key, new Font("Segoe UI", 9F), new Rectangle(8, y, labelW - 12, 26), Text, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                using var brush = new SolidBrush(Purple);
                if (w > 0) g.FillRectangle(brush, labelW, y + 4, Math.Max(4, w), 18);
                string value = currency ? $"₱{data[i].Value:N2}" : data[i].Value.ToString("N0");
                TextRenderer.DrawText(g, value, new Font("Segoe UI", 9F, FontStyle.Bold), new Rectangle(labelW + barArea + 8, y, valueW, 26), Text, TextFormatFlags.VerticalCenter);
            }
            return;
        }

        decimal maxV = Math.Max(1, data.Max(x => x.Value));
        var plot = new Rectangle(52, 20, Math.Max(180, size.Width - 75), Math.Max(150, size.Height - 70));
        using (var axis = new Pen(Grid))
        {
            g.DrawLine(axis, plot.Left, plot.Bottom, plot.Right, plot.Bottom);
            g.DrawLine(axis, plot.Left, plot.Top, plot.Left, plot.Bottom);
        }

        int gap = 18;
        int barW = Math.Max(24, (plot.Width - gap * Math.Max(0, data.Count - 1) - 12) / Math.Max(1, data.Count));
        for (int i = 0; i < data.Count; i++)
        {
            int h = (int)((plot.Height - 20) * (double)(data[i].Value / maxV));
            int x = plot.Left + 6 + i * (barW + gap);
            int y = plot.Bottom - h;
            using var brush = new SolidBrush(i % 3 == 0 ? Purple : i % 3 == 1 ? Cyan : Green);
            if (h > 0) g.FillRectangle(brush, x, y, barW, h);
            string val = currency ? $"₱{data[i].Value:N0}" : data[i].Value.ToString("N0");
            TextRenderer.DrawText(g, val, new Font("Segoe UI", 8F, FontStyle.Bold), new Rectangle(x - 5, y - 20, barW + 10, 18), Text, TextFormatFlags.HorizontalCenter);
            TextRenderer.DrawText(g, data[i].Key, new Font("Segoe UI", 8F), new Rectangle(x - 12, plot.Bottom + 5, barW + 24, 34), Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.WordBreak);
        }
    }

    private static void DrawLine(Graphics g, List<KeyValuePair<string, decimal>> data, bool currency, Size size)
    {
        if (data.Count == 0) { DrawNoData(g, size); return; }
        var plot = new Rectangle(55, 22, Math.Max(200, size.Width - 80), Math.Max(150, size.Height - 75));
        decimal max = Math.Max(1, data.Max(x => x.Value));
        using var gridPen = new Pen(Grid) { DashStyle = DashStyle.Dash };
        for (int i = 0; i < 4; i++)
        {
            int y = plot.Top + i * plot.Height / 3;
            g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
        }
        PointF? previous = null;
        using var linePen = new Pen(Cyan, 3F);
        for (int i = 0; i < data.Count; i++)
        {
            float x = plot.Left + (data.Count == 1 ? 0 : (plot.Width - 10F) * i / (data.Count - 1));
            float y = plot.Bottom - (float)(data[i].Value / max) * (plot.Height - 12);
            if (previous.HasValue) g.DrawLine(linePen, previous.Value, new PointF(x, y));
            using var dotBrush = new SolidBrush(Cyan);
            g.FillEllipse(dotBrush, x - 5, y - 5, 10, 10);
            TextRenderer.DrawText(g, data[i].Key, new Font("Segoe UI", 8F), new Rectangle((int)x - 40, plot.Bottom + 8, 80, 22), Muted, TextFormatFlags.HorizontalCenter);
            string value = currency ? $"₱{data[i].Value:N0}" : data[i].Value.ToString("N0");
            TextRenderer.DrawText(g, value, new Font("Segoe UI", 7.5F, FontStyle.Bold), new Rectangle((int)x - 45, (int)y - 26, 90, 18), Text, TextFormatFlags.HorizontalCenter);
            previous = new PointF(x, y);
        }
    }

    private static void DrawPie(Graphics g, List<KeyValuePair<string, decimal>> data, Size size)
    {
        data = data.Where(x => x.Value > 0).ToList();
        if (data.Count == 0) { DrawNoData(g, size); return; }
        decimal total = Math.Max(1, data.Sum(x => x.Value));
        int pieSize = Math.Min(size.Height - 50, 235);
        var rect = new Rectangle(20, (size.Height - pieSize) / 2, pieSize, pieSize);
        var colors = new[] { Purple, Cyan, Green, Red, Amber, Color.FromArgb(99, 102, 241) };
        float start = 0;
        for (int i = 0; i < data.Count; i++)
        {
            float sweep = 360F * (float)(data[i].Value / total);
            using var brush = new SolidBrush(colors[i % colors.Length]);
            g.FillPie(brush, rect, start, sweep);
            start += sweep;
        }

        int y = 32;
        for (int i = 0; i < data.Count; i++)
        {
            using var brush = new SolidBrush(colors[i % colors.Length]);
            g.FillRectangle(brush, rect.Right + 28, y, 13, 13);
            string label = $"{data[i].Key}  {data[i].Value:N0}";
            TextRenderer.DrawText(g, label, new Font("Segoe UI", 9F), new Rectangle(rect.Right + 50, y - 4, size.Width - rect.Right - 60, 24), Text, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            y += 32;
        }
    }

    private static void DrawFunnel(Graphics g, List<KeyValuePair<string, decimal>> data, Size size)
    {
        if (data.Count == 0) { DrawNoData(g, size); return; }
        decimal max = Math.Max(1, data.Max(x => x.Value));
        int rowH = Math.Max(42, (size.Height - 30) / data.Count);
        int labelW = 165;
        int valueW = 60;
        int barArea = Math.Max(160, size.Width - labelW - valueW - 28);
        var fills = new[] { Purple, Purple2, Color.FromArgb(168, 85, 247), Color.FromArgb(192, 132, 252) };
        for (int i = 0; i < data.Count; i++)
        {
            int y = 12 + i * rowH;
            int width = data[i].Value <= 0 ? 0 : Math.Max(8, (int)(barArea * (double)(data[i].Value / max)));
            TextRenderer.DrawText(g, data[i].Key, new Font("Segoe UI", 9F), new Rectangle(8, y, labelW - 8, 28), Text, TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            if (width > 0)
            {
                using var brush = new SolidBrush(fills[i % fills.Length]);
                g.FillRectangle(brush, labelW, y + 4, width, 20);
            }
            TextRenderer.DrawText(g, data[i].Value.ToString("N0"), new Font("Segoe UI", 9F, FontStyle.Bold), new Rectangle(labelW + barArea + 8, y, valueW, 28), Text, TextFormatFlags.VerticalCenter);
        }
    }

    private static void DrawNoData(Graphics g, Size size)
    {
        TextRenderer.DrawText(g, "No data available for this period", new Font("Segoe UI", 10F, FontStyle.Italic), new Rectangle(Point.Empty, size), Muted, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
