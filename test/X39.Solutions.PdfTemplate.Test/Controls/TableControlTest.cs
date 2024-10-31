using System.Globalization;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class TableControlTest
{
    [Fact]
    public async Task TableWith2X100PxLinesWillScaleToFullPageSize()
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                      <td><line thickness="100px" length="100px"/></td>
                                      <td><line thickness="100px" length="100px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0,   0, 200, 100)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,   0, 200, 100)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0,   0, 100, 100)); // td
        mockCanvas.AssertClip(3, new Rectangle(0,   0, 100, 100)); // line
        mockCanvas.AssertClip(4, new Rectangle(100, 0, 100, 100)); // td
        mockCanvas.AssertClip(5, new Rectangle(100, 0, 100, 100)); // line
    }

    [Fact]
    public async Task TableWith2X200PxLinesWillScaleToFullPageSize()
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                      <td><line thickness="100px" length="2000px"/></td>
                                      <td><line thickness="100px" length="2000px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0,   0, 200,  100)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,   0, 200,  100)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0,   0, 100, 100)); // td
        mockCanvas.AssertClip(3, new Rectangle(0,   0, 2000, 100)); // line
        mockCanvas.AssertClip(4, new Rectangle(100, 0, 100, 100)); // td
        mockCanvas.AssertClip(5, new Rectangle(100, 0, 2000, 100)); // line
    }

    [Fact]
    public async Task TableWith2X50PxLinesWillScaleToFullPageSize()
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                      <td><line thickness="100px" length="50px"/></td>
                                      <td><line thickness="100px" length="50px"/></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0,   0, 200, 100)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,   0, 200, 100)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0,   0, 100, 100)); // td
        mockCanvas.AssertClip(3, new Rectangle(0,   0, 50,  100)); // line
        mockCanvas.AssertClip(4, new Rectangle(100, 0, 100, 100)); // td
        mockCanvas.AssertClip(5, new Rectangle(100, 0, 50,  100)); // line
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public async Task TableWithEmptyColsWillScaleToFullPageSizeInSum(int amount)
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                    @for i from 0 to {{amount}} {
                                        <td></td>
                                      }
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 200, 0)); // table
        mockCanvas.AssertClip(1, new Rectangle(0, 0, 200, 0)); // tr
        for (var i = 0; i < amount; i++)
        {
            var width = 200F / amount;
            mockCanvas.AssertClip(2 + i, new Rectangle(width * i, 0, width, 0)); // td
        }
    }

    [Fact]
    public async Task TableWith2EmptyColsWillScaleToFullPageSizeEachColHalf()
    {
        var control = await $$"""
                              <table>
                                  <tr>
                                      <td></td>
                                      <td></td>
                                  </tr>
                              </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0,   0, 200, 0)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,   0, 200, 0)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0,   0, 100, 0)); // td
        mockCanvas.AssertClip(3, new Rectangle(100, 0, 100, 0)); // td
    }

    [Fact]
    public async Task RightAlignedContentIsNotClippedAway()
    {
        var control = await $$"""
                                <table>
                                    <tr>
                                     	<td><line horizontalAlignment="right" length="20px" thickness="20px"/></td>
                                    </tr>
                                </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(200, 200);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0,   0, 200, 20)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,   0, 200, 20)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0,   0, 200, 20)); // td
        mockCanvas.AssertClip(3, new Rectangle(180, 0, 20,  20)); // line
    }

    [Fact]
    public async Task RowHeightCorrectlyAdjustsForTableHeightExceedingPage()
    {
        var control = await $$"""
                                <table>
                                    <tr>
                                     	<td><mock width="25px" height="25px"/></td>
                                     	<td><mock width="25px" height="25px"/></td>
                                     	<td><mock width="25px" height="25px"/></td>
                                     	<td><mock width="25px" height="25px"/></td>
                                    </tr>
                                    <tr>
                                     	<td><mock width="25px" height="10px"/></td>
                                     	<td><mock width="25px" height="50px"/></td>
                                     	<td><mock width="25px" height="50px"/></td>
                                     	<td><mock width="25px" height="10px"/></td>
                                    </tr>
                                    <tr>
                                     	<td><mock width="25px" height="50px"/></td>
                                     	<td><mock width="25px" height="10px"/></td>
                                     	<td><mock width="25px" height="10px"/></td>
                                     	<td><mock width="25px" height="50px"/></td>
                                    </tr>
                                </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 125);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 100, 125)); // table
        // Assert that the first row is 25px high and 100px wide
        mockCanvas.AssertClip(1, new Rectangle(0, 0, 100, 25)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0, 0, 25,  25)); // td
        mockCanvas.AssertClip(3, new Rectangle(25, 0, 25,  25)); // td
        mockCanvas.AssertClip(4, new Rectangle(50, 0, 25,  25)); // td
        mockCanvas.AssertClip(5, new Rectangle(75, 0, 25,  25)); // td
        // Assert that the second row is 50px high and 100px wide
        mockCanvas.AssertClip(6, new Rectangle(0, 25, 100, 50)); // tr
        mockCanvas.AssertClip(7, new Rectangle(0, 25, 25,  50)); // td
        mockCanvas.AssertClip(8, new Rectangle(25, 25, 25,  50)); // td
        mockCanvas.AssertClip(9, new Rectangle(50, 25, 25,  50)); // td
        mockCanvas.AssertClip(10, new Rectangle(75, 25, 25,  50)); // td
        // Assert that the third row is 50px high and 100px wide
        mockCanvas.AssertClip(11, new Rectangle(0, 75, 100, 50)); // tr
        mockCanvas.AssertClip(12, new Rectangle(0, 75, 25,  50)); // td
        mockCanvas.AssertClip(13, new Rectangle(25, 75, 25,  50)); // td
        mockCanvas.AssertClip(14, new Rectangle(50, 75, 25,  50)); // td
        mockCanvas.AssertClip(15, new Rectangle(75, 75, 25,  50)); // td
    }

    [Fact]
    public async Task RowHeightCorrectlyAdjustsForPageFittingTable()
    {
        var control = await $$"""
                                <table>
                                    <tr>
                                     	<td><mock width="25px" height="25px"/></td>
                                     	<td><mock width="25px" height="25px"/></td>
                                     	<td><mock width="25px" height="25px"/></td>
                                     	<td><mock width="25px" height="25px"/></td>
                                    </tr>
                                    <tr>
                                     	<td><mock width="25px" height="10px"/></td>
                                     	<td><mock width="25px" height="50px"/></td>
                                     	<td><mock width="25px" height="50px"/></td>
                                     	<td><mock width="25px" height="10px"/></td>
                                    </tr>
                                    <tr>
                                     	<td><mock width="25px" height="50px"/></td>
                                     	<td><mock width="25px" height="10px"/></td>
                                     	<td><mock width="25px" height="10px"/></td>
                                     	<td><mock width="25px" height="50px"/></td>
                                    </tr>
                                </table>
                              """.ToControl<TableControl>();
        var pageSize   = new Size(100, 125);
        var mockCanvas = new DeferredCanvasMock{ActualPageSize = pageSize, PageSize = pageSize};
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertClip(0, new Rectangle(0, 0, 100, 125)); // table
        // Assert that the first row is 25px high and 100px wide
        mockCanvas.AssertClip(1, new Rectangle(0, 0, 100, 25)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0, 0, 25,  25)); // td
        mockCanvas.AssertClip(3, new Rectangle(25, 0, 25,  25)); // td
        mockCanvas.AssertClip(4, new Rectangle(50, 0, 25,  25)); // td
        mockCanvas.AssertClip(5, new Rectangle(75, 0, 25,  25)); // td
        // Assert that the second row is 50px high and 100px wide
        mockCanvas.AssertClip(6, new Rectangle(0, 25, 100, 50)); // tr
        mockCanvas.AssertClip(7, new Rectangle(0, 25, 25,  50)); // td
        mockCanvas.AssertClip(8, new Rectangle(25, 25, 25,  50)); // td
        mockCanvas.AssertClip(9, new Rectangle(50, 25, 25,  50)); // td
        mockCanvas.AssertClip(10, new Rectangle(75, 25, 25,  50)); // td
        // Assert that the third row is 50px high and 100px wide
        mockCanvas.AssertClip(11, new Rectangle(0, 75, 100, 50)); // tr
        mockCanvas.AssertClip(12, new Rectangle(0, 75, 25,  50)); // td
        mockCanvas.AssertClip(13, new Rectangle(25, 75, 25,  50)); // td
        mockCanvas.AssertClip(14, new Rectangle(50, 75, 25,  50)); // td
        mockCanvas.AssertClip(15, new Rectangle(75, 75, 25,  50)); // td
    }
}
