using System.Globalization;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Test.Mock;
using Xunit;

namespace X39.Solutions.PdfTemplate.Test.Controls.Charts;

public class LineChartTests
{
    private const float Dpi = 90f;

    private static (DeferredCanvasMock Mock, Size PageSize) RenderChart(
        LineChart chart,
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
    public void LineChart_AllDrawOperationsWithinBounds()
    {
        var chart = new LineChart();
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });
        chart.Add(new ChartDataControl { X = "2", Y = "15" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
    }

    [Fact]
    public void LineChart_WithTitle_AllDrawOperationsWithinBounds()
    {
        var chart = new LineChart { Title = "Sales Trend" };
        chart.Add(new ChartDataControl { X = "0", Y = "100" });
        chart.Add(new ChartDataControl { X = "1", Y = "150" });
        chart.Add(new ChartDataControl { X = "2", Y = "120" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
        mock.AssertAnyDrawTextContains("Sales Trend");
    }

    [Fact]
    public void LineChart_WithNegativeValues_AllDrawOperationsWithinBounds()
    {
        var chart = new LineChart();
        chart.Add(new ChartDataControl { X = "0", Y = "-10" });
        chart.Add(new ChartDataControl { X = "1", Y = "10" });
        chart.Add(new ChartDataControl { X = "2", Y = "-5" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
    }

    [Fact]
    public void LineChart_SmallDimensions_AllDrawOperationsWithinBounds()
    {
        var chart = new LineChart
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
        mock.AssertAllDrawTextWithin(bounds);
    }

    #endregion

    #region Rendering elements

    [Fact]
    public void LineChart_WithTwoPoints_DrawsConnectingLine()
    {
        var chart = new LineChart
        {
            ShowGrid = false,
            ShowXAxis = false,
            ShowYAxis = false,
            ShowPoints = false,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // With grid/axis/points off and 2 data points: exactly 1 connecting line
        Assert.Equal(1, mock.DrawLineCount);
    }

    [Fact]
    public void LineChart_WithMultiplePoints_DrawsNMinusOneLines()
    {
        var chart = new LineChart
        {
            ShowGrid = false,
            ShowXAxis = false,
            ShowYAxis = false,
            ShowPoints = false,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "0" });
        chart.Add(new ChartDataControl { X = "1", Y = "10" });
        chart.Add(new ChartDataControl { X = "2", Y = "5" });
        chart.Add(new ChartDataControl { X = "3", Y = "15" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // 4 points, no grid/axis/points → 3 connecting lines
        Assert.Equal(3, mock.DrawLineCount);
    }

    [Fact]
    public void LineChart_WithShowPointsTrue_DrawsPointMarkers()
    {
        var chart = new LineChart
        {
            ShowGrid = false,
            ShowXAxis = false,
            ShowYAxis = false,
            ShowPoints = true,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // 1 connecting line + 2 points × 2 cross-lines each = 5 lines
        Assert.Equal(5, mock.DrawLineCount);
    }

    [Fact]
    public void LineChart_WithShowPointsFalse_DoesNotDrawPointMarkers()
    {
        var chart = new LineChart
        {
            ShowPoints = false,
            ShowGrid = false,
            ShowXAxis = false,
            ShowYAxis = false,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        Assert.Equal(1, mock.DrawLineCount);
    }

    [Fact]
    public void LineChart_WithSinglePoint_RendersPointOnly()
    {
        var chart = new LineChart
        {
            ShowGrid = false,
            ShowXAxis = false,
            ShowYAxis = false,
            ShowPoints = true,
        };
        chart.Add(new ChartDataControl { X = "5", Y = "10" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // Single point → 0 connecting lines + 1 point × 2 cross-lines = 2 lines
        Assert.Equal(2, mock.DrawLineCount);
    }

    [Fact]
    public void LineChart_WithShowGridTrue_DrawsGridLines()
    {
        var chart = new LineChart
        {
            ShowGrid = true,
            ShowXAxis = false,
            ShowYAxis = false,
            ShowPoints = false,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // Grid: 6 vertical + 6 horizontal = 12 grid lines, plus 1 data line = 13
        mock.AssertDrawLineCountAtLeast(13);
    }

    [Fact]
    public void LineChart_WithAxes_DrawsAxisLines()
    {
        var chart = new LineChart
        {
            ShowGrid = false,
            ShowXAxis = true,
            ShowYAxis = true,
            ShowPoints = false,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // 2 axis lines + 1 data line = 3
        Assert.Equal(3, mock.DrawLineCount);
    }

    [Fact]
    public void LineChart_WithCustomColor_UsesSpecifiedColor()
    {
        var customColor = new Color(0xFF0000FF);
        var chart = new LineChart
        {
            LineColor = customColor,
            ShowGrid = false,
            ShowXAxis = false,
            ShowYAxis = false,
            ShowPoints = false,
        };
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        mock.AssertAnyDrawLineWithColor(customColor);
    }

    #endregion

    #region Empty data / edge cases

    [Fact]
    public void LineChart_WithEmptyData_RendersNoDataMessage()
    {
        var chart = new LineChart();

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        mock.AssertAnyDrawTextContains("No Data");
    }

    [Fact]
    public void LineChart_SortsDataByX()
    {
        var chart = new LineChart
        {
            ShowGrid = false,
            ShowXAxis = false,
            ShowYAxis = false,
        };
        // Add in unsorted order
        chart.Add(new ChartDataControl { X = "2", Y = "30" });
        chart.Add(new ChartDataControl { X = "0", Y = "10" });
        chart.Add(new ChartDataControl { X = "1", Y = "20" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
    }

    #endregion

    #region Measurement

    [Theory]
    [InlineData(100f, 200f)]
    [InlineData(300f, 400f)]
    [InlineData(50f, 50f)]
    public void LineChart_MeasureReturnsCorrectSize(float width, float height)
    {
        var chart = new LineChart
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
