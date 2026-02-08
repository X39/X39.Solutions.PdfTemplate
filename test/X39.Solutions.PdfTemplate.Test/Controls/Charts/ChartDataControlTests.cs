using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using Xunit;

namespace X39.Solutions.PdfTemplate.Test.Controls.Charts;

public class ChartDataControlTests
{
    [Fact]
    public void ChartDataControl_ParsesNumericX()
    {
        var control = new ChartDataControl { X = "42.5" };
        var result = control.GetParsedX();

        Assert.NotNull(result);
        Assert.Equal(42.5, result.Value, precision: 5);
    }

    [Fact]
    public void ChartDataControl_ParsesNumericY()
    {
        var control = new ChartDataControl { Y = "100.25" };
        var result = control.GetParsedY();

        Assert.NotNull(result);
        Assert.Equal(100.25, result.Value, precision: 5);
    }

    [Fact]
    public void ChartDataControl_InvalidX_ReturnsNull()
    {
        var control = new ChartDataControl { X = "not-a-number" };
        var result = control.GetParsedX();

        Assert.Null(result);
    }

    [Fact]
    public void ChartDataControl_InvalidY_ReturnsNull()
    {
        var control = new ChartDataControl { Y = "invalid" };
        var result = control.GetParsedY();

        Assert.Null(result);
    }

    [Fact]
    public void ChartDataControl_CachesParseResult()
    {
        var control = new ChartDataControl { X = "10" };

        var result1 = control.GetParsedX();
        var result2 = control.GetParsedX();

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void ChartDataControl_WithColor_StoresColor()
    {
        var color = new Color(0xFF0000FF);
        var control = new ChartDataControl { Color = color };

        Assert.Equal(color, control.Color);
    }

    [Fact]
    public void ChartDataControl_WithLabel_StoresLabel()
    {
        var label = "Test Label";
        var control = new ChartDataControl { Label = label };

        Assert.Equal(label, control.Label);
    }

    [Theory]
    [InlineData("0", 0.0)]
    [InlineData("-5", -5.0)]
    [InlineData("3.14159", 3.14159)]
    [InlineData("1e5", 100000.0)]
    public void ChartDataControl_ParsesVariousFormats(string input, double expected)
    {
        var control = new ChartDataControl { X = input };
        var result = control.GetParsedX();

        Assert.NotNull(result);
        Assert.Equal(expected, result.Value, precision: 5);
    }
}
