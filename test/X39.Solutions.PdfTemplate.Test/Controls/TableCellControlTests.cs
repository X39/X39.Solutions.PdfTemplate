using System.Globalization;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class TableCellControlTests
{
    [Fact]
    public async Task SingleCellContentMatchesSize()
    {
        var control = await $$"""
                              <td horizontalAlignment="Stretch" verticalAlignment="Stretch">
                                    <mock width="100px" height="100px"/>
                              </td>
                              """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new CanvasMock();
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(200, 200), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 200, 200));
    }

    [Fact]
    public async Task SingleCellContentMatchesSizeWithPadding()
    {
        var control = await $$"""
                              <td padding="10px" horizontalAlignment="Stretch" verticalAlignment="Stretch">
                                    <mock width="100px" height="100px"/>
                              </td>
                              """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new CanvasMock();
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(120, 120), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(200, 200), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 200, 200));
    }

    [Fact]
    public async Task SingleCellContentMatchesSizeWithPaddingAndMargin()
    {
        var control = await $$"""
                              <td padding="10px" margin="10px" horizontalAlignment="Stretch" verticalAlignment="Stretch">
                                    <mock width="100px" height="100px"/>
                              </td>
                              """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new CanvasMock();
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(140, 140), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(200, 200), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(10, 10, 180, 180));
    }

    [Fact]
    public async Task CellControlWithTwoChildrenStacksVertical()
    {
        var control = await $$"""
                              <td horizontalAlignment="Stretch" verticalAlignment="Stretch">
                                    <mock width="100px" height="100px"/>
                                    <mock width="100px" height="100px"/>
                              </td>
                              """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new CanvasMock();
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 200), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(200, 200), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 200, 200));
    }
    
    [Fact]
    public async Task SingleCellContentWithLeftAndTopCellControl()
    {
        var control = await $$"""
                              <td horizontalAlignment="Left" verticalAlignment="Top">
                                    <mock width="100px" height="100px"/>
                              </td>
                              """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new CanvasMock();
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 100, 100));
    }
    
    [Fact]
    public async Task SingleCellContentWithRightAndBottomCellControl()
    {
        var control = await $$"""
                              <td horizontalAlignment="Right" verticalAlignment="Bottom">
                                    <mock width="100px" height="100px"/>
                              </td>
                              """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new CanvasMock();
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(100, 100, 100, 100));
    }
    
    [Fact]
    public async Task SingleCellContentWithCenterCellControl()
    {
        var control = await $$"""
                              <td horizontalAlignment="Center" verticalAlignment="Center">
                                    <mock width="100px" height="100px"/>
                              </td>
                              """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new CanvasMock();
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(100, 100), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(50, 50, 100, 100));
    }
    
    [Fact]
    public async Task CellControlWithTwoChildrenStacksVerticallyLeftAndTop()
    {
        var control = await $$"""
                              <td horizontalAlignment="Left" verticalAlignment="Top">
                                    <mock width="50px" height="50px"/>
                                    <mock width="50px" height="50px"/>
                              </td>
                              """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new CanvasMock();
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(50, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(50, 100), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 50, 100));
    }
    
    [Fact]
    public async Task CellControlWithTwoChildrenStacksVerticallyRightAndBottom()
    {
        var control = await $$"""
                              <td horizontalAlignment="Right" verticalAlignment="Bottom">
                                    <mock width="50px" height="50px"/>
                                    <mock width="50px" height="50px"/>
                              </td>
                              """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new CanvasMock();
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(50, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(50, 100), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(150, 100, 50, 100));
    }
    
    [Fact]
    public async Task CellControlWithTwoChildrenStacksVerticallyCenter()
    {
        var control = await $$"""
                              <td horizontalAlignment="Center" verticalAlignment="Center">
                                    <mock width="50px" height="50px"/>
                                    <mock width="50px" height="50px"/>
                              </td>
                              """.ToControl<TableCellControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new CanvasMock();
        var measure    = control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(50, 100), measure);
        var arrange = control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(50, 100), arrange);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(75, 50, 50, 100));
    }
}
