using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Exceptions;
using X39.Solutions.Papercraft.Services.TextService;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class ListControlTests
{
    private const float Dpi = 90F;
    private static readonly Size PageSize = new(100, 100);
    private static readonly TextStyle MarkerTextStyle = new();

    [Fact]
    public async Task XmlActivatesListControlsAndAppliesParameters()
    {
        var unordered = await """
                              <ul marker="circle" indent="10px" markerWidth="3px" itemSpacing="2px">
                                  <li><text>First</text></li>
                              </ul>
                              """.ToControl<UnorderedListControl>();
        var ordered = await """
                            <ol start="5" markerFormat="({0})" indent="12px" markerWidth="4px" itemSpacing="1px">
                                <li><text>First</text></li>
                            </ol>
                            """.ToControl<OrderedListControl>();

        Assert.Equal(EListMarkerStyle.Circle, unordered.Marker);
        Assert.Equal(new Length(10F, ELengthUnit.Pixel), unordered.Indent);
        Assert.Equal(new Length(3F, ELengthUnit.Pixel), unordered.MarkerWidth);
        Assert.Equal(new Length(2F, ELengthUnit.Pixel), unordered.ItemSpacing);
        Assert.IsType<ListItemControl>(Assert.Single(unordered.Children));

        Assert.Equal(5, ordered.Start);
        Assert.Equal("({0})", ordered.MarkerFormat);
        Assert.Equal(new Length(12F, ELengthUnit.Pixel), ordered.Indent);
        Assert.Equal(new Length(4F, ELengthUnit.Pixel), ordered.MarkerWidth);
        Assert.Equal(new Length(1F, ELengthUnit.Pixel), ordered.ItemSpacing);
        Assert.IsType<ListItemControl>(Assert.Single(ordered.Children));
    }

    [Fact]
    public void ListsOnlyAcceptListItems()
    {
        var control = new UnorderedListControl(new FixedTextService());

        Assert.True(control.CanAdd(typeof(ListItemControl)));
        Assert.False(control.CanAdd(typeof(TextControl)));
        Assert.Throws<ArgumentException>(() => control.Add(new TextControl(new FixedTextService())));
    }

    [Theory]
    [InlineData("ul")]
    [InlineData("ol")]
    public async Task TemplateCreationRejectsDirectNonListItemChildren(string elementName)
    {
        var xml = $"<{elementName}><text>bad</text></{elementName}>";

        var exception = await Assert.ThrowsAsync<FailedToCreateControlException>(
            async () =>
            {
                if (elementName == "ul")
                    await xml.ToControl<UnorderedListControl>();
                else
                    await xml.ToControl<OrderedListControl>();
            });

        Assert.True(
            HasInnerException<ContentControlDoesNotSupportTheProvidedChildException>(exception),
            $"Expected {nameof(ContentControlDoesNotSupportTheProvidedChildException)} in the exception chain.");
    }

    [Fact]
    public async Task ListItemsAcceptNormalControlsAndNestedLists()
    {
        var control = await """
                            <ul>
                                <li>
                                    <text>Parent</text>
                                    <ol start="3">
                                        <li><text>Nested</text></li>
                                    </ol>
                                </li>
                            </ul>
                            """.ToControl<UnorderedListControl>();

        var listItem = Assert.IsType<ListItemControl>(Assert.Single(control.Children));
        Assert.Collection(
            listItem.Children,
            (child) => Assert.IsType<TextControl>(child),
            (child) =>
            {
                var ordered = Assert.IsType<OrderedListControl>(child);
                Assert.Equal(3, ordered.Start);
                Assert.IsType<ListItemControl>(Assert.Single(ordered.Children));
            });
    }

    [Fact]
    public void UnorderedMarkersRenderAsAsciiFallback()
    {
        var canvas = CreateCanvas();
        var control = CreateUnorderedList();
        control.Add(new ListItemControl {Clip = false});
        control.Add(new ListItemControl {Clip = false});

        ArrangeAndRender(control, canvas);

        canvas.AssertState();
        canvas.AssertDrawText(
            (MarkerTextStyle, "*", 0F, 10F),
            (MarkerTextStyle, "*", 0F, 20F));
    }

    [Fact]
    public void OrderedMarkersUseStartAndMarkerFormat()
    {
        var canvas = CreateCanvas();
        var control = new OrderedListControl(new FixedTextService())
        {
            Clip = false,
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
            Indent = 20F,
            MarkerWidth = 5F,
            Start = 4,
            MarkerFormat = "({0})",
        };
        control.Add(new ListItemControl {Clip = false});
        control.Add(new ListItemControl {Clip = false});

        ArrangeAndRender(control, canvas);

        canvas.AssertState();
        canvas.AssertDrawText(
            (MarkerTextStyle, "(4)", 0F, 10F),
            (MarkerTextStyle, "(5)", 0F, 20F));
    }

    [Fact]
    public void ContentIsTranslatedByIndentAndItemsUseConfiguredSpacing()
    {
        var canvas = CreateCanvas();
        var control = CreateUnorderedList();
        control.ItemSpacing = 2F;
        var firstItem = new ListItemControl {Clip = false};
        firstItem.Add(new DrawingControl(new Size(10, 10), Colors.Black));
        var secondItem = new ListItemControl {Clip = false};
        secondItem.Add(new DrawingControl(new Size(10, 10), Colors.Magenta));
        control.Add(firstItem);
        control.Add(secondItem);

        ArrangeAndRender(control, canvas);

        canvas.AssertState();
        canvas.AssertDrawText(
            (MarkerTextStyle, "*", 0F, 10F),
            (MarkerTextStyle, "*", 0F, 22F));
        canvas.AssertDrawLine(
            (Colors.Black, 1F, 20F, 0F, 21F, 1F),
            (Colors.Magenta, 1F, 20F, 12F, 21F, 13F));
    }

    [Fact]
    public void NestedListsApplyCumulativeIndentation()
    {
        var canvas = CreateCanvas();
        var outerList = CreateUnorderedList();
        var innerList = CreateUnorderedList(indent: 10F);
        var innerItem = new ListItemControl {Clip = false};
        innerItem.Add(new DrawingControl(new Size(10, 10), Colors.Black));
        innerList.Add(innerItem);
        var outerItem = new ListItemControl {Clip = false};
        outerItem.Add(innerList);
        outerList.Add(outerItem);

        ArrangeAndRender(outerList, canvas);

        canvas.AssertState();
        canvas.AssertDrawText(
            (MarkerTextStyle, "*", 0F, 10F),
            (MarkerTextStyle, "*", 20F, 10F));
        canvas.AssertDrawLine((Colors.Black, 1F, 30F, 0F, 31F, 1F));
    }

    [Fact]
    public void AdditionalChildRenderHeightShiftsFollowingItems()
    {
        var canvas = CreateCanvas();
        var control = CreateUnorderedList();
        var firstItem = new ListItemControl {Clip = false};
        firstItem.Add(new AdditionalHeightControl(new Size(10, 10), 15F, Colors.Black));
        var secondItem = new ListItemControl {Clip = false};
        secondItem.Add(new DrawingControl(new Size(10, 10), Colors.Magenta));
        control.Add(firstItem);
        control.Add(secondItem);

        control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(0F, 15F), additionalSize);
        canvas.AssertState();
        canvas.AssertDrawText(
            (MarkerTextStyle, "*", 0F, 10F),
            (MarkerTextStyle, "*", 0F, 35F));
        canvas.AssertDrawLine(
            (Colors.Black, 1F, 20F, 0F, 21F, 1F),
            (Colors.Magenta, 1F, 20F, 25F, 21F, 26F));
    }

    [Fact]
    public void ListItemChildAdditionalHeightExpandsListItemClip()
    {
        var canvas = CreateCanvas();
        var control = new ListItemControl
        {
            Clip = true,
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        control.Add(new AdditionalHeightControl(new Size(10F, 10F), 15F, Colors.Black));

        control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(0F, 15F), additionalSize);
        canvas.AssertClip(0, new Rectangle(0F, 0F, 10F, 25F));
    }

    [Fact]
    public void ItemAdditionalHeightExpandsListClip()
    {
        var canvas = CreateCanvas();
        var control = CreateUnorderedList();
        control.Clip = true;
        var item = new ListItemControl {Clip = false};
        item.Add(new AdditionalHeightControl(new Size(10F, 10F), 15F, Colors.Black));
        control.Add(item);

        control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(0F, 15F), additionalSize);
        canvas.AssertClip(0, new Rectangle(0F, 0F, 30F, 25F));
    }

    private static UnorderedListControl CreateUnorderedList(float indent = 20F)
        => new(new FixedTextService())
        {
            Clip = false,
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
            Indent = indent,
            MarkerWidth = 5F,
        };

    private static void ArrangeAndRender(ListControlBase control, DeferredCanvasMock canvas)
    {
        control.Measure(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Arrange(Dpi, PageSize, PageSize, PageSize, CultureInfo.InvariantCulture);
        control.Render(canvas, Dpi, PageSize, CultureInfo.InvariantCulture);
    }

    private static DeferredCanvasMock CreateCanvas()
        => new() {ActualPageSize = PageSize, PageSize = PageSize};

    private static bool HasInnerException<TException>(Exception exception)
        where TException : Exception
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is TException)
                return true;
        }

        return false;
    }

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
