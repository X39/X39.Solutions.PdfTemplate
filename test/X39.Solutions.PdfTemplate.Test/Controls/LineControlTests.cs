using System.Globalization;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class LineControlTests
{
    [Theory]
    // @formatter:max_line_length 2000
    [InlineData(EHorizontalAlignment.Center, EVerticalAlignment.Center, EOrientation.Horizontal, 0.5F, ELengthUnit.Percent, 0.1F, ELengthUnit.Percent, new[] {100F, 10F}, new[] {50.0F, 1.0F}, new[] {50.0F, 1.0F}, new[] {25F, 5F, 75F, 5F})]
    [InlineData(EHorizontalAlignment.Center, EVerticalAlignment.Center, EOrientation.Horizontal, 50.0F, ELengthUnit.Pixel, 1.0F, ELengthUnit.Pixel, new[] {100F, 10F}, new[] {50.0F, 1.0F}, new[] {50.0F, 1.0F}, new[] {25F, 5F, 75F, 5F})]
    [InlineData(EHorizontalAlignment.Center, EVerticalAlignment.Center, EOrientation.Vertical, 0.5F, ELengthUnit.Percent, 0.1F, ELengthUnit.Percent, new[] {10F, 100F}, new[] {1.0F, 50.0F}, new[] {1.0F, 50.0F}, new[] {5F, 25F, 5F, 75F})]
    [InlineData(EHorizontalAlignment.Center, EVerticalAlignment.Center, EOrientation.Vertical, 50.0F, ELengthUnit.Pixel, 1.0F, ELengthUnit.Pixel, new[] {10F, 100F}, new[] {1.0F, 50.0F}, new[] {1.0F, 50.0F}, new[] {5F, 25F, 5F, 75F})]
    [InlineData(EHorizontalAlignment.Left, EVerticalAlignment.Top, EOrientation.Horizontal, 0.5F, ELengthUnit.Percent, 0.1F, ELengthUnit.Percent, new[] {100F, 10F}, new[] {50.0F, 1.0F}, new[] {50.0F, 1.0F}, new[] {0F, 0.5F, 50F, 0.5F})]
    [InlineData(EHorizontalAlignment.Left, EVerticalAlignment.Top, EOrientation.Horizontal, 50.0F, ELengthUnit.Pixel, 1.0F, ELengthUnit.Pixel, new[] {100F, 10F}, new[] {50.0F, 1.0F}, new[] {50.0F, 1.0F}, new[] {0F, 0.5F, 50F, 0.5F})]
    [InlineData(EHorizontalAlignment.Left, EVerticalAlignment.Top, EOrientation.Vertical, 0.5F, ELengthUnit.Percent, 0.1F, ELengthUnit.Percent, new[] {10F, 100F}, new[] {1.0F, 50.0F}, new[] {1.0F, 50.0F}, new[] {0.5F, 0F, 0.5F, 50F})]
    [InlineData(EHorizontalAlignment.Left, EVerticalAlignment.Top, EOrientation.Vertical, 50.0F, ELengthUnit.Pixel, 1.0F, ELengthUnit.Pixel, new[] {10F, 100F}, new[] {1.0F, 50.0F}, new[] {1.0F, 50.0F}, new[] {0.5F, 0F, 0.5F, 50F})]
    [InlineData(EHorizontalAlignment.Right, EVerticalAlignment.Bottom, EOrientation.Horizontal, 0.5F, ELengthUnit.Percent, 0.1F, ELengthUnit.Percent, new[] {100F, 10F}, new[] {50.0F, 1.0F}, new[] {50.0F, 1.0F}, new[] {50F, 9.5F, 100F, 9.5F})]
    [InlineData(EHorizontalAlignment.Right, EVerticalAlignment.Bottom, EOrientation.Horizontal, 50.0F, ELengthUnit.Pixel, 1.0F, ELengthUnit.Pixel, new[] {100F, 10F}, new[] {50.0F, 1.0F}, new[] {50.0F, 1.0F}, new[] {50F, 9.5F, 100F, 9.5F})]
    [InlineData(EHorizontalAlignment.Right, EVerticalAlignment.Bottom, EOrientation.Vertical, 0.5F, ELengthUnit.Percent, 0.1F, ELengthUnit.Percent, new[] {10F, 100F}, new[] {1.0F, 50.0F}, new[] {1.0F, 50.0F}, new[] {9.5F, 50F, 9.5F, 100F})]
    [InlineData(EHorizontalAlignment.Right, EVerticalAlignment.Bottom, EOrientation.Vertical, 50.0F, ELengthUnit.Pixel, 1.0F, ELengthUnit.Pixel, new[] {10F, 100F}, new[] {1.0F, 50.0F}, new[] {1.0F, 50.0F}, new[] {9.5F, 50F, 9.5F, 100F})]
    // @formatter:max_line_length restore
    public void DrawLineTests(
        EHorizontalAlignment horizontalAlignment,
        EVerticalAlignment verticalAlignment,
        EOrientation orientation,
        float lengthValue,
        ELengthUnit lengthUnit,
        float thicknessValue,
        ELengthUnit thicknessUnit,
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
        var mock = new DeferredCanvasMock{ActualPageSize = pageBounds, PageSize = pageBounds};
        var lineControl = new LineControl
        {
            Color               = Colors.Green,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment   = verticalAlignment,
            Margin              = new Thickness(),
            Padding             = new Thickness(),
            Orientation         = orientation,
            Length              = new Length(lengthValue, lengthUnit),
            Clip                = false,
            Thickness           = new Length(thicknessValue, thicknessUnit),
        };
        Assert.Equal(
            measure,
            lineControl.Measure(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture));
        Assert.Equal(
            arrange,
            lineControl.Arrange(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture));
        lineControl.Render(mock, 90, pageBounds, CultureInfo.InvariantCulture);
        mock.AssertState();
        mock.AssertDrawLine(
            Colors.Green,
            lineControl.Thickness.ToPixels(
                orientation == EOrientation.Horizontal ? pageBounds.Height : pageBounds.Width,
                90),
            expectedStartX,
            expectedStartY,
            expectedEndX,
            expectedEndY);
    }

    [Theory]
    [InlineData(EOrientation.Horizontal)]
    [InlineData(EOrientation.Vertical)]
    public void PaddingIsApplied(EOrientation orientation)
    {
        var pageBounds = new Size(1000, 1000);
        var mock = new DeferredCanvasMock{ActualPageSize = pageBounds, PageSize = pageBounds};
        var lineControl = new LineControl
        {
            Color               = Colors.Green,
            HorizontalAlignment = EHorizontalAlignment.Stretch,
            VerticalAlignment   = EVerticalAlignment.Stretch,
            Margin              = new Thickness(),
            Padding             = new Thickness(new Length(10F, ELengthUnit.Pixel)),
            Orientation         = orientation,
            Length              = new Length(1F, ELengthUnit.Percent),
            Clip                = false,
            Thickness           = new Length(1, ELengthUnit.Pixel),
        };
        var measure = new Size(
            orientation == EOrientation.Horizontal ? 1000F : 21F,
            orientation == EOrientation.Horizontal ? 21F : 1000F);
        var arrange = new Size(
            orientation == EOrientation.Horizontal ? 1000F : 21F,
            orientation == EOrientation.Horizontal ? 21F : 1000F);
        Assert.Equal(
            measure,
            lineControl.Measure(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture));
        Assert.Equal(
            arrange,
            lineControl.Arrange(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture));
        lineControl.Render(mock, 90, pageBounds, CultureInfo.InvariantCulture);
        mock.AssertState();
        mock.AssertDrawLine(
            Colors.Green,
            lineControl.Thickness.ToPixels(
                orientation == EOrientation.Horizontal
                    ? pageBounds.Height
                    : pageBounds.Width,
                90),
            orientation == EOrientation.Horizontal ? 10F : 10.5F,
            orientation == EOrientation.Horizontal ? 10.5F : 10F,
            orientation == EOrientation.Horizontal ? 990F : 10.5F,
            orientation == EOrientation.Horizontal ? 10.5F : 990F);
    }

    [Theory]
    [InlineData(EOrientation.Horizontal)]
    [InlineData(EOrientation.Vertical)]
    public void MarginIsApplied(EOrientation orientation)
    {
        var pageBounds = new Size(1000, 1000);
        var mock = new DeferredCanvasMock{ActualPageSize = pageBounds, PageSize = pageBounds};
        var lineControl = new LineControl
        {
            Color               = Colors.Green,
            HorizontalAlignment = EHorizontalAlignment.Stretch,
            VerticalAlignment   = EVerticalAlignment.Stretch,
            Margin              = new Thickness(new Length(10F, ELengthUnit.Pixel)),
            Padding             = new Thickness(),
            Orientation         = orientation,
            Length              = new Length(1F, ELengthUnit.Percent),
            Clip                = false,
            Thickness           = new Length(1, ELengthUnit.Pixel),
        };
        var measure = new Size(
            orientation == EOrientation.Horizontal ? 1000F : 21F,
            orientation == EOrientation.Horizontal ? 21F : 1000F);
        var arrange = new Size(
            orientation == EOrientation.Horizontal ? 1000F : 21F,
            orientation == EOrientation.Horizontal ? 21F : 1000F);
        Assert.Equal(measure, lineControl.Measure(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture));
        Assert.Equal(arrange, lineControl.Arrange(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture));
        lineControl.Render(mock, 90, pageBounds, CultureInfo.InvariantCulture);
        mock.AssertState();
        mock.AssertDrawLine(
            Colors.Green,
            lineControl.Thickness.ToPixels(
                orientation == EOrientation.Horizontal
                    ? pageBounds.Height
                    : pageBounds.Width,
                90),
            orientation == EOrientation.Horizontal ? 10F : 10.5F,
            orientation == EOrientation.Horizontal ? 10.5F : 10F,
            orientation == EOrientation.Horizontal ? 990F : 10.5F,
            orientation == EOrientation.Horizontal ? 10.5F : 990F);
    }

    [Theory]
    [InlineData(EOrientation.Horizontal)]
    [InlineData(EOrientation.Vertical)]
    public void MarginAndPaddingBothAreApplied(EOrientation orientation)
    {
        var pageBounds = new Size(1000, 1000);
        var mock = new DeferredCanvasMock{ActualPageSize = pageBounds, PageSize = pageBounds};
        var lineControl = new LineControl
        {
            Color               = Colors.Green,
            HorizontalAlignment = EHorizontalAlignment.Stretch,
            VerticalAlignment   = EVerticalAlignment.Stretch,
            Margin              = new Thickness(new Length(10F, ELengthUnit.Pixel)),
            Padding             = new Thickness(new Length(10F, ELengthUnit.Pixel)),
            Orientation         = orientation,
            Length              = new Length(1F, ELengthUnit.Percent),
            Clip                = false,
            Thickness           = new Length(1, ELengthUnit.Pixel),
        };
        var measure = new Size(
            orientation == EOrientation.Horizontal ? 1000F : 41F,
            orientation == EOrientation.Horizontal ? 41F : 1000F);
        var arrange = new Size(
            orientation == EOrientation.Horizontal ? 1000F : 41F,
            orientation == EOrientation.Horizontal ? 41F : 1000F);
        Assert.Equal(measure, lineControl.Measure(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture));
        Assert.Equal(arrange, lineControl.Arrange(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture));
        lineControl.Render(mock, 90, pageBounds, CultureInfo.InvariantCulture);
        mock.AssertState();
        mock.AssertDrawLine(
            Colors.Green,
            lineControl.Thickness.ToPixels(
                orientation == EOrientation.Horizontal
                    ? pageBounds.Height
                    : pageBounds.Width,
                90),
            orientation == EOrientation.Horizontal ? 20F : 20.5F,
            orientation == EOrientation.Horizontal ? 20.5F : 20F,
            orientation == EOrientation.Horizontal ? 980F : 20.5F,
            orientation == EOrientation.Horizontal ? 20.5F : 980F);
    }
}