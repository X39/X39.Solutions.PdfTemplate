using System.Globalization;
using System.Text;
using System.Xml;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Controls.Base;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Test.Mock;
using X39.Solutions.PdfTemplate.Transformers;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class TableControlTest
{
    [Fact]
    public async Task TableWith2X100PXLinesWillScaleToFullPageSize()
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
        var mockCanvas = new CanvasMock();
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
    public async Task TableWith2X200PXLinesWillScaleToFullPageSize()
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
        var mockCanvas = new CanvasMock();
        control.Measure(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Arrange(90, pageSize, pageSize, pageSize, CultureInfo.InvariantCulture);
        control.Render(mockCanvas, 90, pageSize, CultureInfo.InvariantCulture);
        mockCanvas.AssertState();
        mockCanvas.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
        // Assert that the two <td> elements take 50% of the width each
        mockCanvas.AssertClip(0, new Rectangle(0,   0, 200,  100)); // table
        mockCanvas.AssertClip(1, new Rectangle(0,   0, 200,  100)); // tr
        mockCanvas.AssertClip(2, new Rectangle(0,   0, 100,  100)); // td
        mockCanvas.AssertClip(3, new Rectangle(0,   0, 2000, 100)); // line
        mockCanvas.AssertClip(4, new Rectangle(100, 0, 100,  100)); // td
        mockCanvas.AssertClip(5, new Rectangle(100, 0, 2000, 100)); // line
    }

    [Fact]
    public async Task TableWith2X50PXLinesWillScaleToFullPageSize()
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
        var mockCanvas = new CanvasMock();
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
        var mockCanvas = new CanvasMock();
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
        var mockCanvas = new CanvasMock();
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
        var mockCanvas = new CanvasMock();
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
}
