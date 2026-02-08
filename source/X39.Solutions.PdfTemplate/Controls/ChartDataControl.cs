using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Controls;

/// <summary>
/// The data information for chart data.
/// </summary>
[Control(Constants.ControlsNamespace, "data")]
public class ChartDataControl : IControl
{
    /// <summary>
    /// X-Value
    /// </summary>
    [Parameter]
    public string X { get; set; } = string.Empty;

    /// <summary>
    /// Y-Value
    /// </summary>
    [Parameter]
    public string Y { get; set; } = string.Empty;

    /// <summary>
    /// X-Label
    /// </summary>
    [Parameter(Name = "x-label")]
    public string XLabel { get; set; } = string.Empty;

    /// <summary>
    /// Y-Label
    /// </summary>
    [Parameter(Name = "y-label")]
    public string YLabel { get; set; } = string.Empty;

    /// <summary>
    /// Color for this data point
    /// </summary>
    [Parameter]
    public Color? Color { get; set; }

    /// <summary>
    /// Label for this data point
    /// </summary>
    [Parameter]
    public string Label { get; set; } = string.Empty;

    private double? _parsedX;
    private double? _parsedY;
    private bool _xParsed;
    private bool _yParsed;

    /// <summary>
    /// Gets the parsed X value as a double, or null if parsing fails.
    /// </summary>
    public double? GetParsedX()
    {
        if (_xParsed)
            return _parsedX;

        _xParsed = true;
        if (double.TryParse(X, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            _parsedX = result;
        }
        else
        {
            _parsedX = null;
        }

        return _parsedX;
    }

    /// <summary>
    /// Gets the parsed Y value as a double, or null if parsing fails.
    /// </summary>
    public double? GetParsedY()
    {
        if (_yParsed)
            return _parsedY;

        _yParsed = true;
        if (double.TryParse(Y, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            _parsedY = result;
        }
        else
        {
            _parsedY = null;
        }

        return _parsedY;
    }

    /// <inheritdoc />
    public Size Measure(float dpi, in Size fullPageSize, in Size framedPageSize, in Size remainingSize, CultureInfo cultureInfo)
        => Size.Zero;

    /// <inheritdoc />
    public Size Arrange(float dpi, in Size fullPageSize, in Size framedPageSize, in Size remainingSize, CultureInfo cultureInfo)
        => Size.Zero;

    /// <inheritdoc />
    public Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        => Size.Zero;
}