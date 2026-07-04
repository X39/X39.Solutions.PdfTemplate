using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Exceptions;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class ColumnsControlTests
{
    private const float Dpi = 90F;
    private static readonly Size PageSize = new(100F, 100F);

    [Fact]
    public async Task XmlActivatesColumnsControlAndAppliesParameters()
    {
        var control = await """
                            <columns
                                count="3"
                                gap="7px"
                                ruleThickness="2px"
                                ruleColor="red"
                                padding="1px"
                                margin="2px"
                                clip="false"
                                horizontalAlignment="right"
                                verticalAlignment="bottom">
                                <mock width="10px" height="20px"/>
                            </columns>
                            """.ToControl<ColumnsControl>();

        Assert.Equal(3, control.ColumnCount);
        Assert.Equal(new Length(7F, ELengthUnit.Pixel), control.Gap);
        Assert.Equal(new Length(2F, ELengthUnit.Pixel), control.RuleThickness);
        Assert.Equal(Colors.Red, control.RuleColor);
        Assert.Equal(new Thickness(1F), control.Padding);
        Assert.Equal(new Thickness(2F), control.Margin);
        Assert.False(control.Clip);
        Assert.Equal(EHorizontalAlignment.Right, control.HorizontalAlignment);
        Assert.Equal(EVerticalAlignment.Bottom, control.VerticalAlignment);
        Assert.IsType<MockControl>(Assert.Single(control.Children));
    }

    [Fact]
    public async Task XmlRejectsRemovedBalanceAttribute()
    {
        var exception = await Assert.ThrowsAsync<FailedToCreateControlException>(
            async () => await """
                              <columns balance="true">
                                  <mock width="10px" height="20px"/>
                              </columns>
                              """.ToControl<ColumnsControl>());

        var parameterException = Assert.IsType<ControlParameterIsNotExistingException>(exception.InnerException);
        Assert.Contains("BALANCE", parameterException.MissingParameters);
    }

    [Fact]
    public async Task XmlAcceptsHyphenatedRuleAliasesFromPlan()
    {
        var control = await """
                            <columns rule-thickness="3px" rule-color="blue">
                                <mock width="10px" height="20px"/>
                            </columns>
                            """.ToControl<ColumnsControl>();

        Assert.Equal(new Length(3F, ELengthUnit.Pixel), control.RuleThickness);
        Assert.Equal(Colors.Blue, control.RuleColor);
    }

    [Fact]
    public void CountIsClampedToAtLeastOne()
    {
        var control = CreateColumns();
        control.ColumnCount = 0;

        Assert.Equal(1, control.ColumnCount);
    }

    [Fact]
    public void ArrangeComputesColumnWidthFromCountAndGap()
    {
        var child = new RecordingControl(new Size(10F, 10F));
        var control = CreateColumns();
        control.ColumnCount = 3;
        control.Gap = 10F;
        control.Add(child);

        var arranged = Arrange(control, new Size(100F, 100F));

        Assert.Equal(new Size(100F, 10F), arranged);
        Assert.Equal(80F / 3F, child.ArrangeRemainingSize.Width, precision: 4);
        Assert.Equal(100F, child.ArrangeRemainingSize.Height);
    }

    [Fact]
    public void ShortContentStaysInFirstColumn()
    {
        var control = CreateColumns();
        control.ColumnCount = 2;
        control.Gap = 10F;
        control.Add(new DrawingControl(new Size(10F, 20F), Colors.Black));
        control.Add(new DrawingControl(new Size(10F, 20F), Colors.Magenta));
        var canvas = CreateCanvas();

        var arranged = ArrangeAndRender(control, canvas, new Size(100F, 100F));

        Assert.Equal(new Size(100F, 40F), arranged);
        canvas.AssertState();
        canvas.AssertDrawLine(
            (Colors.Black, 1F, 0F, 0F, 1F, 1F),
            (Colors.Magenta, 1F, 0F, 20F, 1F, 21F));
    }

    [Fact]
    public void ContentMovesToSecondColumnWhenFirstColumnHeightIsExceeded()
    {
        var control = CreateColumns();
        control.ColumnCount = 2;
        control.Gap = 10F;
        control.Add(new DrawingControl(new Size(10F, 40F), Colors.Black));
        control.Add(new DrawingControl(new Size(10F, 20F), Colors.Magenta));
        var canvas = CreateCanvas();

        var arranged = ArrangeAndRender(control, canvas, new Size(100F, 50F));

        Assert.Equal(new Size(100F, 40F), arranged);
        canvas.AssertState();
        canvas.AssertDrawLine(
            (Colors.Black, 1F, 0F, 0F, 1F, 1F),
            (Colors.Magenta, 1F, 55F, 0F, 56F, 1F));
    }

    [Fact]
    public void ContentBeyondAllColumnsStacksAnotherColumnSet()
    {
        var control = CreateColumns();
        control.ColumnCount = 2;
        control.Gap = 10F;
        control.Add(new DrawingControl(new Size(10F, 40F), Colors.Black));
        control.Add(new DrawingControl(new Size(10F, 40F), Colors.Magenta));
        control.Add(new DrawingControl(new Size(10F, 40F), Colors.Blue));
        var canvas = CreateCanvas();

        var arranged = ArrangeAndRender(control, canvas, new Size(100F, 50F));

        Assert.Equal(new Size(100F, 90F), arranged);
        canvas.AssertState();
        canvas.AssertDrawLine(
            (Colors.Black, 1F, 0F, 0F, 1F, 1F),
            (Colors.Magenta, 1F, 55F, 0F, 56F, 1F),
            (Colors.Blue, 1F, 0F, 50F, 1F, 51F));
    }

    [Fact]
    public void OversizedChildIsPlacedInEmptyColumn()
    {
        var control = CreateColumns();
        control.ColumnCount = 2;
        control.Gap = 10F;
        control.Add(new DrawingControl(new Size(10F, 75F), Colors.Black));
        control.Add(new DrawingControl(new Size(10F, 10F), Colors.Magenta));
        var canvas = CreateCanvas();

        var arranged = ArrangeAndRender(control, canvas, new Size(100F, 50F));

        Assert.Equal(new Size(100F, 75F), arranged);
        canvas.AssertState();
        canvas.AssertDrawLine(
            (Colors.Black, 1F, 0F, 0F, 1F, 1F),
            (Colors.Magenta, 1F, 55F, 0F, 56F, 1F));
    }

    [Fact]
    public void RuleDrawingUsesGapMidpointsAndColumnSetHeight()
    {
        var control = CreateColumns();
        control.ColumnCount = 3;
        control.Gap = 12F;
        control.RuleThickness = 2F;
        control.RuleColor = Colors.Red;
        control.Add(new FixedSizeControl(new Size(10F, 20F)));
        var canvas = CreateCanvas();

        ArrangeAndRender(control, canvas, new Size(120F, 100F));

        canvas.AssertState();
        canvas.AssertDrawLine(
            (Colors.Red, 2F, 38F, 0F, 38F, 20F),
            (Colors.Red, 2F, 82F, 0F, 82F, 20F));
    }

    [Fact]
    public void ChildAdditionalHeightExpandsColumnsClip()
    {
        var control = CreateColumns();
        control.Clip = true;
        control.Add(new AdditionalHeightControl(new Size(10F, 10F), 15F, Colors.Black));
        var canvas = CreateCanvas();

        var arranged = Arrange(control, new Size(100F, 100F));
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(100F, 10F), arranged);
        Assert.Equal(new Size(0F, 15F), additionalSize);
        canvas.AssertClip(0, new Rectangle(0F, 0F, 100F, 25F));
    }

    [Fact]
    public void CanAddAllowsArbitraryChildControls()
    {
        var control = CreateColumns();

        Assert.True(control.CanAdd(typeof(TextControl)));
        Assert.True(control.CanAdd(typeof(BlockControl)));
        Assert.True(control.CanAdd(typeof(MockControl)));
    }

    private static ColumnsControl CreateColumns()
        => new()
        {
            Clip = false,
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };

    private static Size ArrangeAndRender(
        ColumnsControl control,
        DeferredCanvasMock canvas,
        Size availableSize)
    {
        var arranged = Arrange(control, availableSize);
        control.Render(canvas, Dpi, availableSize, CultureInfo.InvariantCulture);
        return arranged;
    }

    private static Size Arrange(ColumnsControl control, Size availableSize)
    {
        control.Measure(Dpi, PageSize, availableSize, availableSize, CultureInfo.InvariantCulture);
        return control.Arrange(Dpi, PageSize, availableSize, availableSize, CultureInfo.InvariantCulture);
    }

    private static DeferredCanvasMock CreateCanvas()
        => new() { ActualPageSize = PageSize, PageSize = PageSize };

    private sealed class FixedSizeControl(Size size) : IControl
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

        public Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
            => Size.Zero;
    }

    private sealed class RecordingControl(Size size) : IControl
    {
        public Size ArrangeRemainingSize { get; private set; }

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
        {
            ArrangeRemainingSize = remainingSize;
            return size;
        }

        public Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
            => Size.Zero;
    }

    private sealed class DrawingControl(Size size, Color color) : IControl
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

        public Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        {
            canvas.DrawLine(color, 1F, 0F, 0F, 1F, 1F);
            return Size.Zero;
        }
    }

    private sealed class AdditionalHeightControl(Size size, float additionalHeight, Color color) : IControl
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

        public Size Render(IDeferredCanvas canvas, float dpi, in Size parentSize, CultureInfo cultureInfo)
        {
            canvas.DrawLine(color, 1F, 0F, 0F, 1F, 1F);
            return new Size(0F, additionalHeight);
        }
    }
}
