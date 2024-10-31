using System.Globalization;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class TableRowControlTests
{
    [Fact]
    public async Task RowControlWith1CellAndStretchScalesToFullAvailableWidthAndCellContentHeight()
    {
        var control = await $$"""
                              <tr horizontalAlignment="Stretch" verticalAlignment="Stretch">
                                  <td horizontalAlignment="Left" verticalAlignment="Top">
                                        <mock width="100px" height="100px"/>
                                  </td>
                              </tr>
                              """.ToControl<TableRowControl>();
        var table = new TableControl();
        control.Table = table;

        const float pageHeight                   = 200;
        const float pageWidth                    = 200;
        const float dpi                          = 90;
        const float expectedControlMeasureWidth  = 100;
        const float expectedControlMeasureHeight = 100;
        const float expectedControlArrangeWidth  = 200;
        const float expectedControlArrangeHeight = 100;
        const float clipLeft                     = 0;
        const float clipTop                      = 0;
        const float clipWidth                    = 200;
        const float clipHeight                   = 100;
        const float cellSpacing                  = 200;
        
        const float cell0Width = 100;

        var pageSize   = new Size(pageHeight, pageWidth);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(dpi, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(expectedControlMeasureWidth, expectedControlMeasureHeight), measure);
        Assert.Equal(cell0Width,                                                          table.CellWidths[0].Item1);
        table.CellWidths.ApplyForEach((_, tuple) => (cellSpacing, tuple.Item2));
        var arrange = control.Arrange(dpi, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(expectedControlArrangeWidth, expectedControlArrangeHeight), arrange);
        control.Render(mockCanvas, dpi, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();                                                                                                        
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(clipLeft, clipTop, clipWidth, clipHeight));
    }

    [Fact]
    public async Task RowControlWith2CellsAndStretchScalesToFullAvailableWidthAndCellContentHeight()
    {
        var control = await $$"""
                              <tr horizontalAlignment="Stretch" verticalAlignment="Stretch">
                                  <td horizontalAlignment="Left" verticalAlignment="Top">
                                        <mock width="100px" height="100px"/>
                                  </td>
                                  <td horizontalAlignment="Left" verticalAlignment="Top">
                                        <mock width="100px" height="100px"/>
                                  </td>
                              </tr>
                              """.ToControl<TableRowControl>();
        var table = new TableControl();
        control.Table = table;

        const float pageHeight                   = 200;
        const float pageWidth                    = 200;
        const float dpi                          = 90;
        const float expectedControlMeasureWidth  = 200;
        const float expectedControlMeasureHeight = 100;
        const float expectedControlArrangeWidth  = 200;
        const float expectedControlArrangeHeight = 100;
        const float clipLeft                     = 0;
        const float clipTop                      = 0;
        const float clipWidth                    = 200;
        const float clipHeight                   = 100;
        const float cellSpacing                  = 100;
        
        const float cell0Width = 100;
        const float cell1Width = 100;

        var pageSize   = new Size(pageHeight, pageWidth);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(dpi, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(expectedControlMeasureWidth, expectedControlMeasureHeight), measure);
        Assert.Equal(cell0Width,                                         table.CellWidths[0].Item1);
        Assert.Equal(cell1Width,                                         table.CellWidths[1].Item1);
        table.CellWidths.ApplyForEach((_, tuple) => (cellSpacing, tuple.Item2));
        var arrange = control.Arrange(dpi, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(expectedControlArrangeWidth, expectedControlArrangeHeight), arrange);
        control.Render(mockCanvas, dpi, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(clipLeft, clipTop, clipWidth, clipHeight));
    }

    [Fact]
    public async Task RowControlWith4CellsAndStretchScalesToFullAvailableWidthAndCellContentHeight()
    {
        var control = await $$"""
                              <tr horizontalAlignment="Stretch" verticalAlignment="Stretch">
                                  <td horizontalAlignment="Left" verticalAlignment="Top">
                                        <mock width0="100px" height0="100px" width1="50px" height1="150px"/>
                                  </td>
                                  <td horizontalAlignment="Left" verticalAlignment="Top">
                                        <mock width0="100px" height0="100px" width1="50px" height1="200px"/>
                                  </td>
                                  <td horizontalAlignment="Left" verticalAlignment="Top">
                                        <mock width0="100px" height0="100px" width1="50px" height1="200px"/>
                                  </td>
                                  <td horizontalAlignment="Left" verticalAlignment="Top">
                                        <mock width0="100px" height0="100px" width1="50px" height1="100px"/>
                                  </td>
                              </tr>
                              """.ToControl<TableRowControl>();
        var table = new TableControl();
        control.Table = table;

        const float pageHeight                   = 200;
        const float pageWidth                    = 200;
        const float dpi                          = 90;
        const float expectedControlMeasureWidth  = 400;
        const float expectedControlMeasureHeight = 100;
        const float expectedControlArrangeWidth  = 200;
        const float expectedControlArrangeHeight = 200;
        const float clipLeft                     = 0;
        const float clipTop                      = 0;
        const float clipWidth                    = 200;
        const float clipHeight                   = 200;
        const float cellSpacing                  = 50;

        const float cell0Width = 100;
        const float cell1Width = 100;
        const float cell2Width = 100;
        const float cell3Width = 100;

        var pageSize   = new Size(pageHeight, pageWidth);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        var measure    = control.Measure(dpi, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(expectedControlMeasureWidth, expectedControlMeasureHeight), measure);
        Assert.Equal(cell0Width,                                                          table.CellWidths[0].Item1);
        Assert.Equal(cell1Width,                                                          table.CellWidths[1].Item1);
        Assert.Equal(cell2Width,                                                          table.CellWidths[2].Item1);
        Assert.Equal(cell3Width,                                                          table.CellWidths[3].Item1);
        table.CellWidths.ApplyForEach((_, tuple) => (cellSpacing, tuple.Item2));
        var arrange = control.Arrange(dpi, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        Assert.Equal(new Size(expectedControlArrangeWidth, expectedControlArrangeHeight), arrange);
        control.Render(mockCanvas, dpi, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        mockCanvas.AssertClip(0, new Rectangle(clipLeft, clipTop, clipWidth, clipHeight));
    }
}
