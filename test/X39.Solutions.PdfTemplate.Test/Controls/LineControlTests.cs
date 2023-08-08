using System.Globalization;
using Moq;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class LineControlTests
{
    [Theory]
    // @formatter:max_line_length 2000
    [InlineData(EHorizontalAlignment.Center, EVerticalAlignment.Center, EOrientation.Horizontal, 0.5F, ELengthMode.Percent, 0.1F, ELengthMode.Percent, new[] {100F, 10F}, new[] {50.0F, 1.0F}, new[] {50.0F, 1.0F}, new[] {25F, 5F, 75F, 5F})]
    [InlineData(EHorizontalAlignment.Center, EVerticalAlignment.Center, EOrientation.Horizontal, 50.0F, ELengthMode.Pixel, 1.0F, ELengthMode.Pixel, new[] {100F, 10F}, new[] {50.0F, 1.0F}, new[] {50.0F, 1.0F}, new[] {25F, 5F, 75F, 5F})]
    [InlineData(EHorizontalAlignment.Center, EVerticalAlignment.Center, EOrientation.Vertical, 0.5F, ELengthMode.Percent, 0.1F, ELengthMode.Percent, new[] {10F, 100F}, new[] {1.0F, 50.0F}, new[] {1.0F, 50.0F}, new[] {5F, 25F, 5F, 75F})]
    [InlineData(EHorizontalAlignment.Center, EVerticalAlignment.Center, EOrientation.Vertical, 50.0F, ELengthMode.Pixel, 1.0F, ELengthMode.Pixel, new[] {10F, 100F}, new[] {1.0F, 50.0F}, new[] {1.0F, 50.0F}, new[] {5F, 25F, 5F, 75F})]
    [InlineData(EHorizontalAlignment.Left, EVerticalAlignment.Top, EOrientation.Horizontal, 0.5F, ELengthMode.Percent, 0.1F, ELengthMode.Percent, new[] {100F, 10F}, new[] {50.0F, 1.0F}, new[] {50.0F, 1.0F}, new[] {0F, 0.5F, 50F, 0.5F})]
    [InlineData(EHorizontalAlignment.Left, EVerticalAlignment.Top, EOrientation.Horizontal, 50.0F, ELengthMode.Pixel, 1.0F, ELengthMode.Pixel, new[] {100F, 10F}, new[] {50.0F, 1.0F}, new[] {50.0F, 1.0F}, new[] {0F, 0.5F, 50F, 0.5F})]
    [InlineData(EHorizontalAlignment.Left, EVerticalAlignment.Top, EOrientation.Vertical, 0.5F, ELengthMode.Percent, 0.1F, ELengthMode.Percent, new[] {10F, 100F}, new[] {1.0F, 50.0F}, new[] {1.0F, 50.0F}, new[] {0.5F, 0F, 0.5F, 50F})]
    [InlineData(EHorizontalAlignment.Left, EVerticalAlignment.Top, EOrientation.Vertical, 50.0F, ELengthMode.Pixel, 1.0F, ELengthMode.Pixel, new[] {10F, 100F}, new[] {1.0F, 50.0F}, new[] {1.0F, 50.0F}, new[] {0.5F, 0F, 0.5F, 50F})]
    [InlineData(EHorizontalAlignment.Right, EVerticalAlignment.Bottom, EOrientation.Horizontal, 0.5F, ELengthMode.Percent, 0.1F, ELengthMode.Percent, new[] {100F, 10F}, new[] {50.0F, 1.0F}, new[] {50.0F, 1.0F}, new[] {50F, 9.5F, 100F, 9.5F})]
    [InlineData(EHorizontalAlignment.Right, EVerticalAlignment.Bottom, EOrientation.Horizontal, 50.0F, ELengthMode.Pixel, 1.0F, ELengthMode.Pixel, new[] {100F, 10F}, new[] {50.0F, 1.0F}, new[] {50.0F, 1.0F}, new[] {50F, 9.5F, 100F, 9.5F})]
    [InlineData(EHorizontalAlignment.Right, EVerticalAlignment.Bottom, EOrientation.Vertical, 0.5F, ELengthMode.Percent, 0.1F, ELengthMode.Percent, new[] {10F, 100F}, new[] {1.0F, 50.0F}, new[] {1.0F, 50.0F}, new[] {9.5F, 50F, 9.5F, 100F})]
    [InlineData(EHorizontalAlignment.Right, EVerticalAlignment.Bottom, EOrientation.Vertical, 50.0F, ELengthMode.Pixel, 1.0F, ELengthMode.Pixel, new[] {10F, 100F}, new[] {1.0F, 50.0F}, new[] {1.0F, 50.0F}, new[] {9.5F, 50F, 9.5F, 100F})]
    // @formatter:max_line_length restore
    public void DrawRendersAsExpected(
        EHorizontalAlignment horizontalAlignment,
        EVerticalAlignment verticalAlignment,
        EOrientation orientation,
        float lengthValue,
        ELengthMode lengthMode,
        float thicknessValue,
        ELengthMode thicknessMode,
        float[] pageArr,
        float[] measureArr,
        float[] arrangeArr,
        float[] expectedArr)
    {
        var expectedStartX = expectedArr[0];
        var expectedStartY = expectedArr[1];
        var expectedEndX = expectedArr[2];
        var expectedEndY = expectedArr[3];
        var pageBounds = new Size(pageArr[0], pageArr[1]);
        var measure = new Size(measureArr[0], measureArr[1]);
        var arrange = new Size(arrangeArr[0], arrangeArr[1]);
        var mock = new CanvasMock();
        var lineControl = new LineControl
        {
            Color               = Colors.Green,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment   = verticalAlignment,
            Margin              = new Thickness(),
            Padding             = new Thickness(),
            Orientation         = orientation,
            Length              = new Length(lengthValue, lengthMode),
            Clip                = false,
            Thickness           = new Length(thicknessValue, thicknessMode),
        };
        Assert.Equal(measure, lineControl.Measure(pageBounds, CultureInfo.InvariantCulture));
        Assert.Equal(arrange, lineControl.Arrange(pageBounds, CultureInfo.InvariantCulture));
        lineControl.Render(mock, pageBounds, CultureInfo.InvariantCulture);
        mock.AssertState();
        mock.AssertDrawLine(
            Colors.Green,
            lineControl.Thickness.ToPixels(
                orientation == EOrientation.Horizontal ? pageBounds.Height : pageBounds.Width),
            expectedStartX,
            expectedStartY,
            expectedEndX,
            expectedEndY);
    }
}