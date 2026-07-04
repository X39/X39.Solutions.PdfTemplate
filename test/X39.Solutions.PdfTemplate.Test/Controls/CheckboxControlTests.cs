using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class CheckboxControlTests
{
    private const float Dpi = 90F;
    private static readonly Size PageSize = new(100F, 100F);
    private static readonly TextStyle LabelTextStyle = new();

    [Fact]
    public async Task XmlParametersAreApplied()
    {
        var control = await """
                            <checkbox
                                checked="true"
                                size="8px"
                                label="Approved"
                                gap="3px"
                                strokeColor="red"
                                fill="blue"
                                checkColor="green"
                                strokeThickness="2px">
                                <text>Child label</text>
                            </checkbox>
                            """.ToControl<CheckboxControl>();

        Assert.True(control.Checked);
        Assert.Equal(new Length(8F, ELengthUnit.Pixel), control.Size);
        Assert.Equal("Approved", control.Label);
        Assert.Equal(new Length(3F, ELengthUnit.Pixel), control.Gap);
        Assert.Equal(Colors.Red, control.StrokeColor);
        Assert.Equal(Colors.Blue, control.Fill);
        Assert.Equal(Colors.Green, control.CheckColor);
        Assert.Equal(new Length(2F, ELengthUnit.Pixel), control.StrokeThickness);
        Assert.IsType<TextControl>(Assert.Single(control.Children));
    }

    [Fact]
    public async Task ElementTextPopulatesLabel()
    {
        var control = await "<checkbox>Element Label</checkbox>".ToControl<CheckboxControl>();

        Assert.Equal("Element Label", control.Label);
        Assert.Empty(control.Children);
    }

    [Fact]
    public void UncheckedCheckboxDrawsSquareWithoutFillOrCheckMark()
    {
        var control = CreateCheckbox();
        control.Size = 12F;
        control.StrokeThickness = 2F;
        control.StrokeColor = Colors.Red;
        var canvas = CreateCanvas();

        var additionalSize = ArrangeAndRender(control, canvas);

        Assert.Equal(Size.Zero, additionalSize);
        Assert.Equal(0, canvas.DrawRectCount);
        Assert.Equal(0, canvas.DrawTextCount);
        canvas.AssertState();
        canvas.AssertDrawLine(
            (Colors.Red, 2F, 0F, 0F, 12F, 0F),
            (Colors.Red, 2F, 12F, 0F, 12F, 12F),
            (Colors.Red, 2F, 12F, 12F, 0F, 12F),
            (Colors.Red, 2F, 0F, 12F, 0F, 0F));
    }

    [Fact]
    public void CheckedCheckboxDrawsFillBorderAndCheckMark()
    {
        var control = CreateCheckbox();
        control.Checked = true;
        control.Size = 12F;
        control.StrokeThickness = 2F;
        control.StrokeColor = Colors.Red;
        control.Fill = Colors.Blue;
        control.CheckColor = Colors.Green;
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        canvas.AssertState();
        canvas.AssertDrawRect((new Rectangle(0F, 0F, 12F, 12F), Colors.Blue));
        canvas.AssertDrawLine(
            (Colors.Red, 2F, 0F, 0F, 12F, 0F),
            (Colors.Red, 2F, 12F, 0F, 12F, 12F),
            (Colors.Red, 2F, 12F, 12F, 0F, 12F),
            (Colors.Red, 2F, 0F, 12F, 0F, 0F),
            (Colors.Green, 2F, 3F, 6F, 6F, 9F),
            (Colors.Green, 2F, 6F, 9F, 9F, 3F));
    }

    [Fact]
    public void LabelTextRendersNextToBoxAndContributesToLayout()
    {
        var control = CreateCheckbox();
        control.Size = 12F;
        control.Gap = 3F;
        control.Label = "Approved";
        var canvas = CreateCanvas();

        var measured = control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var arranged = control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(55F, 12F), measured);
        Assert.Equal(new Size(55F, 12F), arranged);
        canvas.AssertState();
        canvas.AssertDrawText(LabelTextStyle, "Approved", 15F, 10F);
    }

    [Fact]
    public void LabelTextWinsOverChildControls()
    {
        var control = CreateCheckbox();
        control.Size = 12F;
        control.Gap = 3F;
        control.Label = "Label";
        control.Add(new DrawingControl(new Size(10F, 10F), Colors.Magenta));
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas);

        canvas.AssertState();
        Assert.Equal(4, canvas.DrawLineCount);
        canvas.AssertDrawText(LabelTextStyle, "Label", 15F, 10F);
    }

    [Fact]
    public void ChildrenRenderToRightAndAdditionalHeightIsPropagated()
    {
        var control = CreateCheckbox();
        control.Size = 12F;
        control.Gap = 3F;
        control.StrokeThickness = 1F;
        control.Add(new AdditionalHeightControl(new Size(20F, 5F), 7F, Colors.Black));
        control.Add(new DrawingControl(new Size(10F, 5F), Colors.Magenta));
        var canvas = CreateCanvas();

        var measured = control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var arranged = control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(35F, 12F), measured);
        Assert.Equal(new Size(35F, 12F), arranged);
        Assert.Equal(new Size(0F, 7F), additionalSize);
        canvas.AssertState();
        canvas.AssertDrawLine(
            (Colors.Black, 1F, 0F, 0F, 12F, 0F),
            (Colors.Black, 1F, 12F, 0F, 12F, 12F),
            (Colors.Black, 1F, 12F, 12F, 0F, 12F),
            (Colors.Black, 1F, 0F, 12F, 0F, 0F),
            (Colors.Black, 1F, 15F, 0F, 16F, 1F),
            (Colors.Magenta, 1F, 15F, 12F, 16F, 13F));
    }

    [Fact]
    public void ChildAdditionalHeightExpandsCheckboxClip()
    {
        var control = CreateCheckbox();
        control.Clip = true;
        control.Size = 12F;
        control.Gap = 3F;
        control.Add(new AdditionalHeightControl(new Size(20F, 10F), 15F, Colors.Black));
        var canvas = CreateCanvas();

        control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(0F, 15F), additionalSize);
        canvas.AssertClip(0, new Rectangle(0F, 0F, 35F, 27F));
    }

    private static CheckboxControl CreateCheckbox()
        => new(new FixedTextService())
        {
            Clip = false,
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };

    private static Size ArrangeAndRender(CheckboxControl control, DeferredCanvasMock canvas)
    {
        control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        return control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);
    }

    private static DeferredCanvasMock CreateCanvas()
        => new() {ActualPageSize = PageSize, PageSize = PageSize};

    private sealed class FixedTextService : ITextService
    {
        public Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
            => text.IsEmpty
                ? Size.Zero
                : new Size(text.Length * 5F, 10F);

        public void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
        {
            if (text.IsEmpty)
                return;

            canvas.DrawText(textStyle, dpi, text.ToString(), 0F, 10F);
        }
    }

    private class DrawingControl(Size size, Color color) : IControl
    {
        public Size Measure(
            float dpi,
            in Size fullPageSize,
            in Size framedPageSize,
            in Size remainingSize,
            CultureInfo cultureInfo)
            => size;

        public Size Arrange(
            float dpi,
            in Size fullPageSize,
            in Size framedPageSize,
            in Size remainingSize,
            CultureInfo cultureInfo)
            => size;

        public virtual Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        {
            canvas.DrawLine(color, 1F, 0F, 0F, 1F, 1F);
            return Size.Zero;
        }
    }

    private sealed class AdditionalHeightControl(Size size, float additionalHeight, Color color)
        : DrawingControl(size, color)
    {
        public override Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        {
            base.Render(canvas, dpi, parentSize, cultureInfo);
            return new Size(0F, additionalHeight);
        }
    }
}
