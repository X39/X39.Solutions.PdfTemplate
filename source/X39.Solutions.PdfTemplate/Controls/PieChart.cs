using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// Pie chart (and donut chart with InnerRadius > 0).
/// </summary>
[Control(Constants.ControlsNamespace)]
public class PieChart : ChartBaseControl
{
    /// <summary>
    /// Starting angle in degrees (0 = top, 90 = right).
    /// </summary>
    [Parameter(Name = "start-angle")]
    public float StartAngle { get; set; } = 0f;

    /// <summary>
    /// Inner radius as percentage of outer radius. Set > 0 to create a donut chart.
    /// </summary>
    [Parameter(Name = "inner-radius")]
    public Length InnerRadius { get; set; } = new(0, ELengthUnit.Percent);

    /// <summary>
    /// Whether to show percentage labels.
    /// </summary>
    [Parameter(Name = "show-percentages")]
    public bool ShowPercentages { get; set; } = true;

    /// <summary>
    /// Whether to show slice labels.
    /// </summary>
    [Parameter(Name = "show-labels")]
    public bool ShowLabels { get; set; } = true;

    private const float TitleHeight = 30f;
    private const int CircleSegments = 60; // Number of segments to approximate circle

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

        var dataPoints = ParseDataPoints(requireX: false);

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

        // Calculate total (only use Y values for pie chart)
        var total = dataPoints.Sum(p => p.Y);

        if (total <= 0)
        {
            var textStyle = new TextStyle
            {
                Foreground = AxisColor,
                FontSize = 12f,
            };
            canvas.DrawText(textStyle, dpi, "No Data Available (all values <= 0)", 10, 20);
            return Size.Zero;
        }

        // Calculate plot area
        var titleOffset = string.IsNullOrEmpty(Title) ? 0f : TitleHeight;

        // Render title
        if (!string.IsNullOrEmpty(Title))
        {
            RenderTitle(canvas, dpi, chartWidth / 2, 5);
        }

        // Calculate center and radius
        var centerX = chartWidth / 2;
        var centerY = titleOffset + (chartHeight - titleOffset) / 2;
        // Reserve more space for labels when they are shown
        var labelPadding = (ShowLabels || ShowPercentages) ? 80f : 20f;
        var radius = Math.Min(chartWidth, chartHeight - titleOffset) / 2 - labelPadding;
        radius = Math.Max(radius, 10f); // Ensure minimum viable radius

        var innerRadiusPixels = InnerRadius.ToPixels(radius, dpi);

        // Render slices
        var currentAngle = StartAngle - 90; // Adjust so 0 degrees is at top
        var colorIndex = 0;

        foreach (var (_, y, control) in dataPoints)
        {
            var sliceAngle = (float)((y / total) * 360);
            var color = control.Color ?? GetPaletteColor(colorIndex++);

            RenderSlice(canvas, centerX, centerY, radius, innerRadiusPixels, currentAngle, sliceAngle, color);

            // Render label if enabled
            if (ShowLabels || ShowPercentages)
            {
                var labelAngle = currentAngle + sliceAngle / 2;
                var labelAngleRad = labelAngle * Math.PI / 180;
                var labelRadius = radius + 15;
                var labelX = centerX + (float)(labelRadius * Math.Cos(labelAngleRad));
                var labelY = centerY + (float)(labelRadius * Math.Sin(labelAngleRad));

                var label = string.Empty;
                if (ShowLabels && !string.IsNullOrEmpty(control.Label))
                    label = control.Label;
                if (ShowPercentages)
                {
                    var percentage = (y / total) * 100;
                    var percentText = string.Format(cultureInfo, "{0:F1}%", percentage);
                    label = string.IsNullOrEmpty(label) ? percentText : $"{label} ({percentText})";
                }

                if (!string.IsNullOrEmpty(label))
                {
                    // Estimate text width (~6px per character at 10px font)
                    var estimatedTextWidth = label.Length * 6f;
                    var estimatedTextHeight = 10f;

                    // Clamp label position to stay within chart bounds
                    labelX = Math.Clamp(labelX, 2f, chartWidth - estimatedTextWidth - 2f);
                    labelY = Math.Clamp(labelY, estimatedTextHeight + 2f, chartHeight - 2f);

                    var textStyle = new TextStyle
                    {
                        Foreground = AxisColor,
                        FontSize = 10f,
                    };
                    canvas.DrawText(textStyle, dpi, label, labelX, labelY);
                }
            }

            currentAngle += sliceAngle;
        }

        return Size.Zero;
    }

    private void RenderSlice(
        IDeferredCanvas canvas,
        float centerX,
        float centerY,
        float radius,
        float innerRadius,
        float startAngle,
        float sweepAngle,
        Color color)
    {
        // Calculate number of segments for this slice
        var segments = Math.Max(2, (int)(CircleSegments * sweepAngle / 360));

        var startRad = startAngle * (float)Math.PI / 180;
        var sweepRad = sweepAngle * (float)Math.PI / 180;

        // Draw filled slice using triangular segments
        for (var i = 0; i < segments; i++)
        {
            var angle1 = startRad + (sweepRad * i / segments);
            var angle2 = startRad + (sweepRad * (i + 1) / segments);

            var outerX1 = centerX + radius * (float)Math.Cos(angle1);
            var outerY1 = centerY + radius * (float)Math.Sin(angle1);
            var outerX2 = centerX + radius * (float)Math.Cos(angle2);
            var outerY2 = centerY + radius * (float)Math.Sin(angle2);

            if (innerRadius > 0)
            {
                // Donut chart - draw trapezoid segment
                var innerX1 = centerX + innerRadius * (float)Math.Cos(angle1);
                var innerY1 = centerY + innerRadius * (float)Math.Sin(angle1);
                var innerX2 = centerX + innerRadius * (float)Math.Cos(angle2);
                var innerY2 = centerY + innerRadius * (float)Math.Sin(angle2);

                // Draw as quad (filled with lines)
                DrawFilledQuad(canvas, color, outerX1, outerY1, outerX2, outerY2, innerX2, innerY2, innerX1, innerY1);
            }
            else
            {
                // Pie chart - draw triangle segment
                DrawFilledTriangle(canvas, color, centerX, centerY, outerX1, outerY1, outerX2, outerY2);
            }
        }

        // Draw outline
        var outlineSegments = segments;
        for (var i = 0; i <= outlineSegments; i++)
        {
            var angle = startRad + (sweepRad * i / outlineSegments);
            var x = centerX + radius * (float)Math.Cos(angle);
            var y = centerY + radius * (float)Math.Sin(angle);

            if (i > 0)
            {
                var prevAngle = startRad + (sweepRad * (i - 1) / outlineSegments);
                var prevX = centerX + radius * (float)Math.Cos(prevAngle);
                var prevY = centerY + radius * (float)Math.Sin(prevAngle);
                canvas.DrawLine(color, 1f, prevX, prevY, x, y);
            }
        }

        // Draw radial lines
        var startX = centerX + (innerRadius > 0 ? innerRadius : 0) * (float)Math.Cos(startRad);
        var startY = centerY + (innerRadius > 0 ? innerRadius : 0) * (float)Math.Sin(startRad);
        var endX = centerX + radius * (float)Math.Cos(startRad);
        var endY = centerY + radius * (float)Math.Sin(startRad);
        canvas.DrawLine(color, 1f, innerRadius > 0 ? startX : centerX, innerRadius > 0 ? startY : centerY, endX, endY);

        var endAngle = startRad + sweepRad;
        var endStartX = centerX + (innerRadius > 0 ? innerRadius : 0) * (float)Math.Cos(endAngle);
        var endStartY = centerY + (innerRadius > 0 ? innerRadius : 0) * (float)Math.Sin(endAngle);
        var endEndX = centerX + radius * (float)Math.Cos(endAngle);
        var endEndY = centerY + radius * (float)Math.Sin(endAngle);
        canvas.DrawLine(color, 1f, innerRadius > 0 ? endStartX : centerX, innerRadius > 0 ? endStartY : centerY, endEndX, endEndY);

        // Draw inner circle arc if donut
        if (innerRadius > 0)
        {
            for (var i = 0; i <= outlineSegments; i++)
            {
                var angle = startRad + (sweepRad * i / outlineSegments);
                var x = centerX + innerRadius * (float)Math.Cos(angle);
                var y = centerY + innerRadius * (float)Math.Sin(angle);

                if (i > 0)
                {
                    var prevAngle = startRad + (sweepRad * (i - 1) / outlineSegments);
                    var prevX = centerX + innerRadius * (float)Math.Cos(prevAngle);
                    var prevY = centerY + innerRadius * (float)Math.Sin(prevAngle);
                    canvas.DrawLine(color, 1f, prevX, prevY, x, y);
                }
            }
        }
    }

    private void DrawFilledTriangle(IDeferredCanvas canvas, Color color, float x1, float y1, float x2, float y2, float x3, float y3)
    {
        // Fill triangle with horizontal lines
        var minY = Math.Min(Math.Min(y1, y2), y3);
        var maxY = Math.Max(Math.Max(y1, y2), y3);

        for (var y = (int)minY; y <= (int)maxY; y++)
        {
            var intersections = new List<float>();

            // Check intersection with each edge
            AddIntersection(intersections, y, x1, y1, x2, y2);
            AddIntersection(intersections, y, x2, y2, x3, y3);
            AddIntersection(intersections, y, x3, y3, x1, y1);

            if (intersections.Count >= 2)
            {
                intersections.Sort();
                canvas.DrawLine(color, 1f, intersections[0], y, intersections[^1], y);
            }
        }
    }

    private void DrawFilledQuad(IDeferredCanvas canvas, Color color, float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
    {
        // Fill quad with horizontal lines
        var minY = Math.Min(Math.Min(Math.Min(y1, y2), y3), y4);
        var maxY = Math.Max(Math.Max(Math.Max(y1, y2), y3), y4);

        for (var y = (int)minY; y <= (int)maxY; y++)
        {
            var intersections = new List<float>();

            // Check intersection with each edge
            AddIntersection(intersections, y, x1, y1, x2, y2);
            AddIntersection(intersections, y, x2, y2, x3, y3);
            AddIntersection(intersections, y, x3, y3, x4, y4);
            AddIntersection(intersections, y, x4, y4, x1, y1);

            if (intersections.Count >= 2)
            {
                intersections.Sort();
                for (var i = 0; i < intersections.Count - 1; i += 2)
                {
                    canvas.DrawLine(color, 1f, intersections[i], y, intersections[i + 1], y);
                }
            }
        }
    }

    private void AddIntersection(List<float> intersections, float y, float x1, float y1, float x2, float y2)
    {
        if ((y1 <= y && y <= y2) || (y2 <= y && y <= y1))
        {
            if (Math.Abs(y2 - y1) < 0.001)
            {
                intersections.Add(x1);
                intersections.Add(x2);
            }
            else
            {
                var t = (y - y1) / (y2 - y1);
                var x = x1 + t * (x2 - x1);
                intersections.Add(x);
            }
        }
    }
}
