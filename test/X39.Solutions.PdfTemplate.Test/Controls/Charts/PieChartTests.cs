using System.Globalization;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Test.Mock;
using Xunit;

namespace X39.Solutions.PdfTemplate.Test.Controls.Charts;

public class PieChartTests
{
    private const float Dpi = 90f;

    private static (DeferredCanvasMock Mock, Size PageSize) RenderChart(
        PieChart chart,
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

    #region Bounds validation - labels within clip region

    [Fact]
    public void PieChart_WithLabels_AllDrawOperationsWithinBounds()
    {
        var chart = new PieChart();
        chart.Add(new ChartDataControl { Y = "40", Label = "Category 1" });
        chart.Add(new ChartDataControl { Y = "30", Label = "Category 2" });
        chart.Add(new ChartDataControl { Y = "30", Label = "Category 3" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
    }

    [Fact]
    public void PieChart_WithTitle_AllDrawOperationsWithinBounds()
    {
        var chart = new PieChart { Title = "Donut Chart" };
        chart.Add(new ChartDataControl { Y = "40", Label = "Category 1" });
        chart.Add(new ChartDataControl { Y = "30", Label = "Category 2" });
        chart.Add(new ChartDataControl { Y = "30", Label = "Category 3" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
        mock.AssertAnyDrawTextContains("Donut Chart");
    }

    [Fact]
    public void PieChart_DonutWithLabels_AllDrawOperationsWithinBounds()
    {
        var chart = new PieChart
        {
            Title = "Donut Chart",
            InnerRadius = new Length(0.5f, ELengthUnit.Percent),
        };
        chart.Add(new ChartDataControl { Y = "40", Label = "Category 1" });
        chart.Add(new ChartDataControl { Y = "30", Label = "Category 2" });
        chart.Add(new ChartDataControl { Y = "30", Label = "Category 3" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
    }

    [Fact]
    public void PieChart_SmallDimensions_AllDrawOperationsWithinBounds()
    {
        var chart = new PieChart
        {
            Width = new Length(200, ELengthUnit.Pixel),
            Height = new Length(200, ELengthUnit.Pixel),
        };
        chart.Add(new ChartDataControl { Y = "50", Label = "A" });
        chart.Add(new ChartDataControl { Y = "50", Label = "B" });

        var (mock, pageSize) = RenderChart(chart, 200, 200);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
    }

    [Fact]
    public void PieChart_WithPercentages_LabelsWithinBounds()
    {
        var chart = new PieChart { ShowPercentages = true, ShowLabels = true };
        chart.Add(new ChartDataControl { Y = "40", Label = "Category 1" });
        chart.Add(new ChartDataControl { Y = "30", Label = "Category 2" });
        chart.Add(new ChartDataControl { Y = "20", Label = "Category 3" });
        chart.Add(new ChartDataControl { Y = "10", Label = "Category 4" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
    }

    [Fact]
    public void PieChart_ManySlices_AllWithinBounds()
    {
        var chart = new PieChart { ShowLabels = true, ShowPercentages = true };
        for (var i = 0; i < 8; i++)
        {
            chart.Add(new ChartDataControl { Y = (10 + i * 5).ToString(), Label = $"Slice {i + 1}" });
        }

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
        mock.AssertAllDrawTextWithin(bounds);
    }

    #endregion

    #region Data parsing (Y-only for pie charts)

    [Fact]
    public void PieChart_WithoutXValues_RendersSuccessfully()
    {
        var chart = new PieChart();
        chart.Add(new ChartDataControl { Y = "50" });
        chart.Add(new ChartDataControl { Y = "50" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // Should render slices, not "No Data Available"
        mock.AssertDrawLineCountAtLeast(1);
    }

    [Fact]
    public void PieChart_WithXValues_AlsoRenders()
    {
        var chart = new PieChart();
        chart.Add(new ChartDataControl { X = "0", Y = "50" });
        chart.Add(new ChartDataControl { X = "1", Y = "50" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        mock.AssertDrawLineCountAtLeast(1);
    }

    #endregion

    #region Rendering elements

    [Fact]
    public void PieChart_WithTwoEqualSlices_RendersSlices()
    {
        var chart = new PieChart { ShowPercentages = false, ShowLabels = false };
        chart.Add(new ChartDataControl { Y = "50" });
        chart.Add(new ChartDataControl { Y = "50" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // Each slice renders multiple line segments for the arc + fill + radial lines
        mock.AssertDrawLineCountAtLeast(4);
    }

    [Fact]
    public void PieChart_WithPercentagesEnabled_DrawsPercentageText()
    {
        var chart = new PieChart { ShowPercentages = true, ShowLabels = false };
        chart.Add(new ChartDataControl { Y = "50" });
        chart.Add(new ChartDataControl { Y = "50" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        mock.AssertAnyDrawTextContains("50.0%");
    }

    [Fact]
    public void PieChart_WithPercentagesFalse_HidesPercentages()
    {
        var chart = new PieChart { ShowPercentages = false, ShowLabels = false };
        chart.Add(new ChartDataControl { Y = "50" });
        chart.Add(new ChartDataControl { Y = "50" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        mock.AssertNoDrawTextContains("%");
    }

    [Fact]
    public void PieChart_WithLabels_DrawsLabelText()
    {
        var chart = new PieChart { ShowLabels = true, ShowPercentages = false };
        chart.Add(new ChartDataControl { Y = "50", Label = "First" });
        chart.Add(new ChartDataControl { Y = "50", Label = "Second" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        mock.AssertAnyDrawTextContains("First");
        mock.AssertAnyDrawTextContains("Second");
    }

    [Fact]
    public void PieChart_WithLabelsAndPercentages_DrawsBoth()
    {
        var chart = new PieChart { ShowLabels = true, ShowPercentages = true };
        chart.Add(new ChartDataControl { Y = "50", Label = "First" });
        chart.Add(new ChartDataControl { Y = "50", Label = "Second" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // Labels should contain both name and percentage, e.g. "First (50.0%)"
        mock.AssertAnyDrawTextContains("First");
        mock.AssertAnyDrawTextContains("50.0%");
    }

    [Fact]
    public void PieChart_WithInnerRadius_CreatesDonut()
    {
        var chart = new PieChart
        {
            InnerRadius = new Length(0.5f, ELengthUnit.Percent),
            ShowPercentages = false,
            ShowLabels = false,
        };
        chart.Add(new ChartDataControl { Y = "50" });
        chart.Add(new ChartDataControl { Y = "50" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // Donut chart renders more line segments (inner + outer arcs)
        mock.AssertDrawLineCountAtLeast(4);
    }

    [Fact]
    public void PieChart_WithStartAngle_RendersSuccessfully()
    {
        var chart = new PieChart { StartAngle = 90 };
        chart.Add(new ChartDataControl { Y = "100" });

        var (mock, pageSize) = RenderChart(chart);
        var bounds = ChartBounds(pageSize);

        mock.AssertState();
        mock.AssertAllDrawLinesWithin(bounds);
    }

    #endregion

    #region Empty data / edge cases

    [Fact]
    public void PieChart_WithEmptyData_RendersNoDataMessage()
    {
        var chart = new PieChart();

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        mock.AssertAnyDrawTextContains("No Data");
    }

    [Fact]
    public void PieChart_WithNegativeValues_RendersErrorMessage()
    {
        var chart = new PieChart();
        chart.Add(new ChartDataControl { Y = "-10" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // Negative total shows error
        mock.AssertAnyDrawTextContains("No Data");
    }

    [Fact]
    public void PieChart_WithDifferentColors_UsesColorPalette()
    {
        var chart = new PieChart { ShowPercentages = false, ShowLabels = false };
        chart.Add(new ChartDataControl { Y = "25" });
        chart.Add(new ChartDataControl { Y = "25" });
        chart.Add(new ChartDataControl { Y = "25" });
        chart.Add(new ChartDataControl { Y = "25" });

        var (mock, _) = RenderChart(chart);

        mock.AssertState();
        // Should render slices (multiple line segments from fill and outline)
        mock.AssertDrawLineCountAtLeast(8);
    }

    #endregion

    #region Measurement

    [Theory]
    [InlineData(100f, 200f)]
    [InlineData(300f, 300f)]
    [InlineData(50f, 50f)]
    public void PieChart_MeasureReturnsCorrectSize(float width, float height)
    {
        var chart = new PieChart
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
