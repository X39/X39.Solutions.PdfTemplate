using System.Globalization;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Test.Mock;
using Xunit;

namespace X39.Solutions.PdfTemplate.Test.Controls.Charts;

public class ChartControlTests
{
    [Fact]
    public void ChartControl_WithMultipleCharts_RendersAll()
    {
        var chartControl = new ChartControl();

        var lineChart = new LineChart { Height = new Length(200, ELengthUnit.Pixel) };
        lineChart.Add(new ChartDataControl { X = "0", Y = "10" });
        lineChart.Add(new ChartDataControl { X = "1", Y = "20" });

        var barChart = new BarChart { Height = new Length(200, ELengthUnit.Pixel) };
        barChart.Add(new ChartDataControl { X = "0", Y = "15" });
        barChart.Add(new ChartDataControl { X = "1", Y = "25" });

        chartControl.Add(lineChart);
        chartControl.Add(barChart);

        var pageSize = new Size(500, 600);
        var mock = new DeferredCanvasMock { ActualPageSize = pageSize, PageSize = pageSize };

        chartControl.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        chartControl.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        chartControl.Render(mock, 90, pageSize, CultureInfo.InvariantCulture);

        mock.AssertState();
        // Both charts should be rendered
    }

    [Fact]
    public void ChartControl_OnlyAcceptsIChartChildren()
    {
        var chartControl = new ChartControl();

        // Should accept chart types
        Assert.True(chartControl.CanAdd(typeof(LineChart)));
        Assert.True(chartControl.CanAdd(typeof(BarChart)));
        Assert.True(chartControl.CanAdd(typeof(PieChart)));

        // Should not accept non-chart types
        Assert.False(chartControl.CanAdd(typeof(ChartDataControl)));
        Assert.False(chartControl.CanAdd(typeof(BorderControl)));
    }

    [Fact]
    public void ChartControl_MeasureAggregatesChildSizes()
    {
        var chartControl = new ChartControl();

        var chart1 = new LineChart
        {
            Width = new Length(400, ELengthUnit.Pixel),
            Height = new Length(200, ELengthUnit.Pixel)
        };
        chart1.Add(new ChartDataControl { X = "0", Y = "10" });

        var chart2 = new BarChart
        {
            Width = new Length(300, ELengthUnit.Pixel),
            Height = new Length(150, ELengthUnit.Pixel)
        };
        chart2.Add(new ChartDataControl { X = "0", Y = "10" });

        chartControl.Add(chart1);
        chartControl.Add(chart2);

        var pageSize = new Size(1000, 1000);
        var measured = chartControl.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);

        // Should use max width and sum of heights
        Assert.Equal(400, measured.Width); // max(400, 300)
        Assert.Equal(350, measured.Height); // 200 + 150
    }

    [Fact]
    public void ChartControl_WithSingleChart_RendersCorrectly()
    {
        var chartControl = new ChartControl();

        var lineChart = new LineChart();
        lineChart.Add(new ChartDataControl { X = "0", Y = "10" });

        chartControl.Add(lineChart);

        var pageSize = new Size(500, 400);
        var mock = new DeferredCanvasMock { ActualPageSize = pageSize, PageSize = pageSize };

        chartControl.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        chartControl.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        chartControl.Render(mock, 90, pageSize, CultureInfo.InvariantCulture);

        mock.AssertState();
        // Single chart should render without issues
    }

    [Fact]
    public void ChartControl_WithNoChildren_RendersEmpty()
    {
        var chartControl = new ChartControl();

        var pageSize = new Size(500, 400);
        var mock = new DeferredCanvasMock { ActualPageSize = pageSize, PageSize = pageSize };

        var measured = chartControl.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        chartControl.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        chartControl.Render(mock, 90, pageSize, CultureInfo.InvariantCulture);

        mock.AssertState();
        Assert.Equal(Size.Zero, measured);
    }
}
