using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Abstraction.Controls;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Controls.Base;

/// <summary>
/// Abstract base class for chart controls with shared functionality.
/// </summary>
public abstract class ChartBaseControl : AlignableContentControl, IChart
{
    /// <summary>
    /// Width of the chart.
    /// </summary>
    [Parameter]
    public Length Width { get; set; } = new(1, ELengthUnit.Percent);

    /// <summary>
    /// Height of the chart.
    /// </summary>
    [Parameter]
    public Length Height { get; set; } = new(300, ELengthUnit.Pixel);

    /// <summary>
    /// Title of the chart.
    /// </summary>
    [Parameter]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Whether to show grid lines.
    /// </summary>
    [Parameter(Name = "show-grid")]
    public bool ShowGrid { get; set; } = true;

    /// <summary>
    /// Color of grid lines.
    /// </summary>
    [Parameter(Name = "grid-color")]
    public Color GridColor { get; set; } = new(0xCCCCCCFF);

    /// <summary>
    /// Color of axis lines.
    /// </summary>
    [Parameter(Name = "axis-color")]
    public Color AxisColor { get; set; } = new(0x000000FF);

    /// <summary>
    /// Whether to show X-axis.
    /// </summary>
    [Parameter(Name = "show-x-axis")]
    public bool ShowXAxis { get; set; } = true;

    /// <summary>
    /// Whether to show Y-axis.
    /// </summary>
    [Parameter(Name = "show-y-axis")]
    public bool ShowYAxis { get; set; } = true;

    /// <summary>
    /// Label for X-axis.
    /// </summary>
    [Parameter(Name = "x-axis-label")]
    public string XAxisLabel { get; set; } = string.Empty;

    /// <summary>
    /// Label for Y-axis.
    /// </summary>
    [Parameter(Name = "y-axis-label")]
    public string YAxisLabel { get; set; } = string.Empty;

    /// <summary>
    /// Default color palette for charts.
    /// </summary>
    protected static readonly Color[] DefaultColorPalette =
    [
        new(0x4472C4FF), // Blue
        new(0xED7D31FF), // Orange
        new(0xA5A5A5FF), // Gray
        new(0xFFC000FF), // Yellow
        new(0x5B9BD5FF), // Light Blue
        new(0x70AD47FF), // Green
        new(0x264478FF), // Dark Blue
        new(0x9E480EFF), // Dark Orange
    ];

    /// <summary>
    /// Parses data points from ChartDataControl children.
    /// </summary>
    /// <param name="requireX">Whether X values are required. When false, missing X values default to sequential indices.</param>
    /// <returns>List of tuples containing parsed X, Y values and the source control.</returns>
    protected List<(double X, double Y, ChartDataControl Control)> ParseDataPoints(bool requireX = true)
    {
        var dataPoints = new List<(double X, double Y, ChartDataControl Control)>();
        var index = 0;

        foreach (var child in Children.OfType<ChartDataControl>())
        {
            var x = child.GetParsedX();
            var y = child.GetParsedY();

            if (!y.HasValue)
            {
                index++;
                continue;
            }

            if (requireX && !x.HasValue)
            {
                index++;
                continue;
            }

            dataPoints.Add((x ?? index, y.Value, child));
            index++;
        }

        return dataPoints;
    }

    /// <summary>
    /// Calculates axis bounds with padding.
    /// </summary>
    /// <param name="dataPoints">The data points to analyze.</param>
    /// <param name="paddingPercent">Padding percentage (default 10%).</param>
    /// <returns>Tuple of (minX, maxX, minY, maxY).</returns>
    protected (double MinX, double MaxX, double MinY, double MaxY) CalculateAxisBounds(
        List<(double X, double Y, ChartDataControl Control)> dataPoints,
        double paddingPercent = 10.0)
    {
        if (dataPoints.Count == 0)
            return (0, 1, 0, 1);

        var minX = dataPoints.Min(p => p.X);
        var maxX = dataPoints.Max(p => p.X);
        var minY = dataPoints.Min(p => p.Y);
        var maxY = dataPoints.Max(p => p.Y);

        // Add padding
        var xRange = maxX - minX;
        var yRange = maxY - minY;

        // Handle zero range
        if (Math.Abs(xRange) < 0.0001)
        {
            minX -= 0.5;
            maxX += 0.5;
        }
        else
        {
            var xPadding = xRange * paddingPercent / 100.0;
            minX -= xPadding;
            maxX += xPadding;
        }

        if (Math.Abs(yRange) < 0.0001)
        {
            minY -= 0.5;
            maxY += 0.5;
        }
        else
        {
            var yPadding = yRange * paddingPercent / 100.0;
            minY -= yPadding;
            maxY += yPadding;
        }

        return (minX, maxX, minY, maxY);
    }

    /// <summary>
    /// Calculates scaling factors for data to pixels.
    /// </summary>
    /// <param name="plotWidth">Width of the plot area in pixels.</param>
    /// <param name="plotHeight">Height of the plot area in pixels.</param>
    /// <param name="minX">Minimum X value.</param>
    /// <param name="maxX">Maximum X value.</param>
    /// <param name="minY">Minimum Y value.</param>
    /// <param name="maxY">Maximum Y value.</param>
    /// <returns>Tuple of (scaleX, scaleY).</returns>
    protected (double ScaleX, double ScaleY) CalculateScaling(
        float plotWidth,
        float plotHeight,
        double minX,
        double maxX,
        double minY,
        double maxY)
    {
        var xRange = maxX - minX;
        var yRange = maxY - minY;

        var scaleX = xRange > 0.0001 ? plotWidth / xRange : 1.0;
        var scaleY = yRange > 0.0001 ? plotHeight / yRange : 1.0;

        return (scaleX, scaleY);
    }

    /// <summary>
    /// Renders grid lines on the canvas.
    /// </summary>
    protected void RenderGrid(
        IDeferredCanvas canvas,
        float plotLeft,
        float plotTop,
        float plotWidth,
        float plotHeight,
        int gridLinesX = 5,
        int gridLinesY = 5)
    {
        if (!ShowGrid)
            return;

        // Vertical grid lines
        for (var i = 0; i <= gridLinesX; i++)
        {
            var x = plotLeft + (plotWidth * i / gridLinesX);
            canvas.DrawLine(GridColor, 1f, x, plotTop, x, plotTop + plotHeight);
        }

        // Horizontal grid lines
        for (var i = 0; i <= gridLinesY; i++)
        {
            var y = plotTop + (plotHeight * i / gridLinesY);
            canvas.DrawLine(GridColor, 1f, plotLeft, y, plotLeft + plotWidth, y);
        }
    }

    /// <summary>
    /// Renders axis lines on the canvas.
    /// </summary>
    protected void RenderAxes(
        IDeferredCanvas canvas,
        float plotLeft,
        float plotTop,
        float plotWidth,
        float plotHeight)
    {
        if (ShowXAxis)
        {
            // X-axis at bottom
            canvas.DrawLine(AxisColor, 2f, plotLeft, plotTop + plotHeight, plotLeft + plotWidth, plotTop + plotHeight);
        }

        if (ShowYAxis)
        {
            // Y-axis at left
            canvas.DrawLine(AxisColor, 2f, plotLeft, plotTop, plotLeft, plotTop + plotHeight);
        }
    }

    /// <summary>
    /// Renders the chart title.
    /// </summary>
    protected void RenderTitle(
        IDeferredCanvas canvas,
        float dpi,
        float centerX,
        float y)
    {
        if (string.IsNullOrEmpty(Title))
            return;

        var textStyle = new TextStyle
        {
            Foreground = AxisColor,
            FontSize = 14f,
        };

        canvas.DrawText(textStyle, dpi, Title, centerX, y);
    }

    /// <summary>
    /// Gets a color from the palette by index.
    /// </summary>
    protected Color GetPaletteColor(int index)
    {
        return DefaultColorPalette[index % DefaultColorPalette.Length];
    }

    /// <inheritdoc />
    public override bool CanAdd(Type type)
        => type.IsEquivalentTo(typeof(ChartDataControl));
}
