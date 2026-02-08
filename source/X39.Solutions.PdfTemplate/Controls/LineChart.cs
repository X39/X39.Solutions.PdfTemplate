using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// Line chart.
/// </summary>
[Control(Constants.ControlsNamespace)]
public class LineChart : ChartBaseControl
{
    /// <summary>
    /// Thickness of the line.
    /// </summary>
    [Parameter(Name = "line-thickness")]
    public Length LineThickness { get; set; } = new(2, ELengthUnit.Pixel);

    /// <summary>
    /// Color of the line.
    /// </summary>
    [Parameter(Name = "line-color")]
    public Color? LineColor { get; set; }

    /// <summary>
    /// Whether to show point markers.
    /// </summary>
    [Parameter(Name = "show-points")]
    public bool ShowPoints { get; set; } = true;

    /// <summary>
    /// Size of point markers.
    /// </summary>
    [Parameter(Name = "point-size")]
    public float PointSize { get; set; } = 4f;

    private const float TitleHeight = 30f;
    private const float AxisPadding = 40f;

    /// <inheritdoc />
    protected override Size DoMeasure(float dpi, in Size fullPageSize, in Size framedPageSize, in Size remainingSize, CultureInfo cultureInfo)
    {
        var width = Width.ToPixels(remainingSize.Width, dpi);
        var height = Height.ToPixels(remainingSize.Height, dpi);
        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override Size DoArrange(float dpi, in Size fullPageSize, in Size framedPageSize, in Size remainingSize, CultureInfo cultureInfo)
    {
        var width = Width.ToPixels(remainingSize.Width, dpi);
        var height = Height.ToPixels(remainingSize.Height, dpi);
        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override Size DoRender(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
    {
        // Use the control's own arranged dimensions, not the parent's size
        var chartWidth = ArrangementInner.Width;
        var chartHeight = ArrangementInner.Height;
        var dataPoints = ParseDataPoints();

        // Handle empty data
        if (dataPoints.Count == 0)
        {
            var textStyle = new TextStyle
            {
                Foreground = AxisColor,
                FontSize = 12f,
            };
            canvas.DrawText(textStyle, dpi, "No Data Available", 10, 20);
            return Size.Zero;
        }

        // Sort by X value
        dataPoints.Sort((a, b) => a.X.CompareTo(b.X));

        // Calculate bounds and scaling
        var (minX, maxX, minY, maxY) = CalculateAxisBounds(dataPoints);

        // Calculate plot area
        var titleOffset = string.IsNullOrEmpty(Title) ? 0f : TitleHeight;
        var plotLeft = ShowYAxis ? AxisPadding : 10f;
        var plotTop = titleOffset + 10f;
        var plotWidth = chartWidth - plotLeft - 20f;
        var plotHeight = chartHeight - plotTop - (ShowXAxis ? AxisPadding : 10f);

        // Render title
        if (!string.IsNullOrEmpty(Title))
        {
            RenderTitle(canvas, dpi, chartWidth / 2, 5);
        }

        // Render grid
        RenderGrid(canvas, plotLeft, plotTop, plotWidth, plotHeight);

        // Render axes
        RenderAxes(canvas, plotLeft, plotTop, plotWidth, plotHeight);

        // Calculate scaling
        var (scaleX, scaleY) = CalculateScaling(plotWidth, plotHeight, minX, maxX, minY, maxY);

        // Determine line color
        var lineColor = LineColor ?? GetPaletteColor(0);
        var thickness = LineThickness.ToPixels(chartHeight, dpi);

        // Convert data points to screen coordinates
        var screenPoints = new List<(float X, float Y)>();
        foreach (var (x, y, _) in dataPoints)
        {
            var screenX = plotLeft + (float)((x - minX) * scaleX);
            var screenY = plotTop + plotHeight - (float)((y - minY) * scaleY);
            screenPoints.Add((screenX, screenY));
        }

        // Draw lines connecting points
        for (var i = 0; i < screenPoints.Count - 1; i++)
        {
            var (x1, y1) = screenPoints[i];
            var (x2, y2) = screenPoints[i + 1];
            canvas.DrawLine(lineColor, thickness, x1, y1, x2, y2);
        }

        // Draw point markers
        if (ShowPoints)
        {
            foreach (var (x, y) in screenPoints)
            {
                // Draw point as a small filled circle (approximated with cross)
                canvas.DrawLine(lineColor, PointSize, x - PointSize / 2, y, x + PointSize / 2, y);
                canvas.DrawLine(lineColor, PointSize, x, y - PointSize / 2, x, y + PointSize / 2);
            }
        }

        return Size.Zero;
    }
}