using System.Globalization;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Test.Mock;
using Xunit;

namespace X39.Solutions.PdfTemplate.Test.Controls.Charts;

public class BarChartTests
{
    private const float Dpi = 90f;

    private static (DeferredCanvasMock Mock, Size PageSize) RenderChart(
        BarChart chart,
        float width = 500f,
        float height = 400f)
    {
        var pageSize = new Size(width, height);
        var mock = new DeferredCanvasMock { ActualPageSize = pageSize, PageSize = pageSize };

        chart.Measure(Dpi, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        chart.Arrange(Dpi, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        chart.Render(mock, Dpi, pageSize, CultureInfo.InvariantCulture);

        return (mock, pageSize);
    }

    private static Rectangle ChartBounds(Size pageSize) =>
        new(0, 0, pageSize.Width, pageSize.Height);

    #region Bounds validation

    [Fact]
    public void BarChart_AllDrawOperationsWithinBounds()
    {
        var chart = new BarChart();
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });
        chart.Add(new ChartDataControl { X = "2", Y = "15" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawRectsWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
    }

    [Fact]
    public void BarChart_WithTitle_AllDrawOperationsWithinBounds()
    {
        var chart = new BarChart { Title = "Revenue by Quarter" };
        chart.Add(new ChartDataControl { X = "1", Y = "100" });
        chart.Add(new ChartDataControl { X = "2", Y = "150" });
        chart.Add(new ChartDataControl { X = "3", Y = "120" });
        chart.Add(new ChartDataControl { X = "4", Y = "180" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawRectsWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
        mock.AssertAnyDrawTextContains("Revenue by Quarter");
    }

    [Fact]
    public void BarChart_WithNegativeValues_AllDrawOperationsWithinBounds()
    {
        var chart = new BarChart();
        chart.Add(new ChartDataControl { X = "0", Y = "-10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });
        chart.Add(new ChartDataControl { X = "2", Y = "-5" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawRectsWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
    }

    [Fact]
    public void BarChart_HorizontalOrientation_AllDrawOperationsWithinBounds()
    {
        var chart = new BarChart { Orientation = EOrientation.Horizontal };
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });
        chart.Add(new ChartDataControl { X = "2", Y = "15" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawRectsWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
    }

    [Fact]
    public void BarChart_SmallDimensions_AllDrawOperationsWithinBounds()
    {
        var chart = new BarChart
        {
            Width = new Length(200, ELengthUnit.Pixel),
            Height = new Length(150, ELengthUnit.Pixel),
        };
        chart.Add(new ChartDataControl { X = "0", Y = "5" });
        chart.Add(new ChartDataControl { X = "1", Y = "15" });

        var (mock, pageSize) = RenderChart(chart, 200, 150);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawRectsWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
    }

    #endregion

    #region Rendering elements

    [Fact]
    public void BarChart_VerticalOrientation_DrawsBars()
    {
        var chart = new BarChart
        {
            Orientation = EOrientation.Vertical,
            ShowGrid = false,
            ShowXAxis = false,
            ShowYAxis = false,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });
        chart.Add(new ChartDataControl { X = "2", Y = "15" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // 3 bars drawn as rectangles
        mock.AssertDrawRectCountAtLeast(3);
    }

    [Fact]
    public void BarChart_HorizontalOrientation_DrawsBars()
    {
        var chart = new BarChart
        {
            Orientation = EOrientation.Horizontal,
            ShowGrid = false,
            ShowXAxis = false,
            ShowYAxis = false,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        mock.AssertDrawRectCountAtLeast(2);
    }

    [Fact]
    public void BarChart_WithCustomBarColor_UsesSpecifiedColor()
    {
        var customColor = new Color(0x00FF00FF);
        var chart = new BarChart
        {
            BarColor = customColor,
            ShowGrid = false,
            ShowXAxis = false,
            ShowYAxis = false,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        mock.AssertAnyDrawRectWithColor(customColor);
    }

    [Fact]
    public void BarChart_WithDataPointColors_UsesIndividualColors()
    {
        var red = new Color(0xFF0000FF);
        var green = new Color(0x00FF00FF);
        var chart = new BarChart
        {
            ShowGrid = false,
            ShowXAxis = false,
            ShowYAxis = false,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "10", Color = red });
        chart.Add(new ChartDataControl { X = "1", Y = "20", Color = green });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        mock.AssertAnyDrawRectWithColor(red);
        mock.AssertAnyDrawRectWithColor(green);
    }

    [Fact]
    public void BarChart_WithGridAndAxes_DrawsAllElements()
    {
        var chart = new BarChart
        {
            ShowGrid = true,
            ShowXAxis = true,
            ShowYAxis = true,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // Grid: 6+6=12 lines, axes: 2 lines = at least 14 lines
        mock.AssertDrawLineCountAtLeast(14);
        // 2 bars
        mock.AssertDrawRectCountAtLeast(2);
    }

    [Fact]
    public void BarChart_WithSingleBar_RendersCorrectly()
    {
        var chart = new BarChart
        {
            ShowGrid = false,
            ShowXAxis = false,
            ShowYAxis = false,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "10" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertDrawRectCountAtLeast(1);
        mock.AssertAllDrawRectsWithin(bounds);
    }

    [Fact]
    public void BarChart_WithZeroValues_RendersAtBaseline()
    {
        var chart = new BarChart();
        chart.Add(new ChartDataControl { X = "0", Y = "0" });
        chart.Add(new ChartDataControl { X = "1", Y = "0" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawRectsWithin(bounds);
    }

    #endregion

    #region Empty data / edge cases

    [Fact]
    public void BarChart_WithEmptyData_RendersNoDataMessage()
    {
        var chart = new BarChart();

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        mock.AssertAnyDrawTextContains("No Data");
    }

    #endregion

    #region Measurement

    [Theory]
    [InlineData(100f, 200f)]
    [InlineData(300f, 400f)]
    [InlineData(50f, 50f)]
    public void BarChart_MeasureReturnsCorrectSize(float width, float height)
    {
        var chart = new BarChart
        {
            Width = new Length(width, ELengthUnit.Pixel),
            Height = new Length(height, ELengthUnit.Pixel),
        };

        var pageSize = new Size(1000, 1000);
        var measured = chart.Measure(Dpi, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);

        Assert.Equal(width, measured.Width);
        Assert.Equal(height, measured.Height);
    }

    #endregion
}
