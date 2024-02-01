using System.Globalization;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Test.Mock;

[Control("X39.Solutions.PdfTemplate.Controls", "mock")]
public class MockControl : IControl
{
    private List<Length> _widths  = new();
    private List<Length> _heights = new();
    private int _widhtIndex;
    private int _heightIndex;

    [Parameter]
    public Length Width
    {
        get => _widths.Count is 1 ? _widths.First() : default;
        set
        {
            _widths.Clear();
            _widths.Add(value);
        }
    }

    [Parameter]
    public Length Height
    {
        get => _heights.Count is 1 ? _heights.First() : default;
        set
        {
            _heights.Clear();
            _heights.Add(value);
        }
    }

    [Parameter] public Length Width0 { get => _widths.Count > 0 ? _widths[0] : default; set { while (_widths.Count < 1) _widths.Add(default); _widths[0] = value; } }
    [Parameter] public Length Width1 { get => _widths.Count > 1 ? _widths[1] : default; set { while (_widths.Count < 2) _widths.Add(default); _widths[1] = value; } }
    [Parameter] public Length Width2 { get => _widths.Count > 2 ? _widths[2] : default; set { while (_widths.Count < 3) _widths.Add(default); _widths[2] = value; } }
    [Parameter] public Length Width3 { get => _widths.Count > 3 ? _widths[3] : default; set { while (_widths.Count < 4) _widths.Add(default); _widths[3] = value; } }
    [Parameter] public Length Width4 { get => _widths.Count > 4 ? _widths[4] : default; set { while (_widths.Count < 5) _widths.Add(default); _widths[4] = value; } }
    [Parameter] public Length Width5 { get => _widths.Count > 5 ? _widths[5] : default; set { while (_widths.Count < 6) _widths.Add(default); _widths[5] = value; } }
    [Parameter] public Length Width6 { get => _widths.Count > 6 ? _widths[6] : default; set { while (_widths.Count < 7) _widths.Add(default); _widths[6] = value; } }
    [Parameter] public Length Width7 { get => _widths.Count > 7 ? _widths[7] : default; set { while (_widths.Count < 8) _widths.Add(default); _widths[7] = value; } }
    [Parameter] public Length Width8 { get => _widths.Count > 8 ? _widths[8] : default; set { while (_widths.Count < 9) _widths.Add(default); _widths[8] = value; } }
    [Parameter] public Length Width9 { get => _widths.Count > 9 ? _widths[9] : default; set { while (_widths.Count < 10) _widths.Add(default); _widths[9] = value; } }
    
    [Parameter] public Length Height0 { get => _heights.Count > 0 ? _heights[0] : default; set { while (_heights.Count < 1) _heights.Add(default); _heights[0] = value; } }
    [Parameter] public Length Height1 { get => _heights.Count > 1 ? _heights[1] : default; set { while (_heights.Count < 2) _heights.Add(default); _heights[1] = value; } }
    [Parameter] public Length Height2 { get => _heights.Count > 2 ? _heights[2] : default; set { while (_heights.Count < 3) _heights.Add(default); _heights[2] = value; } }
    [Parameter] public Length Height3 { get => _heights.Count > 3 ? _heights[3] : default; set { while (_heights.Count < 4) _heights.Add(default); _heights[3] = value; } }
    [Parameter] public Length Height4 { get => _heights.Count > 4 ? _heights[4] : default; set { while (_heights.Count < 5) _heights.Add(default); _heights[4] = value; } }
    [Parameter] public Length Height5 { get => _heights.Count > 5 ? _heights[5] : default; set { while (_heights.Count < 6) _heights.Add(default); _heights[5] = value; } }
    [Parameter] public Length Height6 { get => _heights.Count > 6 ? _heights[6] : default; set { while (_heights.Count < 7) _heights.Add(default); _heights[6] = value; } }
    [Parameter] public Length Height7 { get => _heights.Count > 7 ? _heights[7] : default; set { while (_heights.Count < 8) _heights.Add(default); _heights[7] = value; } }
    [Parameter] public Length Height8 { get => _heights.Count > 8 ? _heights[8] : default; set { while (_heights.Count < 9) _heights.Add(default); _heights[8] = value; } }
    [Parameter] public Length Height9 { get => _heights.Count > 9 ? _heights[9] : default; set { while (_heights.Count < 10) _heights.Add(default); _heights[9] = value; } }
    

    public Size Measure(
        float       dpi,
        in Size     fullPageSize,
        in Size     framedPageSize,
        in Size     remainingSize,
        CultureInfo cultureInfo)
    {
        var widthLength  = _widths.Count > _widhtIndex ? _widths[_widhtIndex++] : _widths.LastOrDefault();
        var heightLength = _heights.Count > _heightIndex ? _heights[_heightIndex++] : _heights.LastOrDefault();
        var width        = widthLength.ToPixels(fullPageSize.Width, dpi);
        var height       = heightLength.ToPixels(fullPageSize.Height, dpi);
        return new Size(width, height);
    }

    public Size Arrange(
        float       dpi,
        in Size     fullPageSize,
        in Size     framedPageSize,
        in Size     remainingSize,
        CultureInfo cultureInfo)
    {
        var widthLength  = _widths.Count > _widhtIndex ? _widths[_widhtIndex++] : _widths.LastOrDefault();
        var heightLength = _heights.Count > _heightIndex ? _heights[_heightIndex++] : _heights.LastOrDefault();
        var width        = widthLength.ToPixels(fullPageSize.Width, dpi);
        var height       = heightLength.ToPixels(fullPageSize.Height, dpi);
        return new Size(width, height);
    }

    public Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo) { 
        return Size.Zero;}
}
