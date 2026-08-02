using System.Globalization;
using X39.Solutions.Papercraft.Controls;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services.TextService;
using X39.Solutions.Papercraft.Services.TextService;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Controls;

public class TextControlTests : IDisposable
{
    private readonly SkPaintCache _paintCache = new();

    public void Dispose()
    {
        _paintCache.Dispose();
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    [InlineData("ABC")]
    [InlineData("ABCD")]
    [InlineData("ABCDE")]
    [InlineData("ABCDEF")]
    [InlineData("ABCDEFG")]
    [InlineData("ABCDEFGH")]
    [InlineData("ABCDEFGHI")]
    [InlineData("ABCDEFGHIJ")]
    public void SizeGreaterZero(string text)
    {
        var pageBounds = new Size(595, 842);
        var mock = new DeferredCanvasMock{ActualPageSize = pageBounds, PageSize = pageBounds};
        var fontPath = GetTestFont();
        var control = new TextControl(new TextService(_paintCache))
        {
            Text       = text,
            FontSize   = 12,
            Style      = EFontStyle.Italic,
            FontFamily = fontPath,
        };
        control.Measure(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        control.Arrange(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        control.Render(mock, 90, pageBounds, CultureInfo.InvariantCulture);
        mock.AssertState();
        mock.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
    }

    [Fact]
    public void RenderMovesLineToNextPageWhenLineWouldBePartiallyClipped()
    {
        var pageBounds = new Size(100, 100);
        var canvas = new DeferredCanvasMock{ActualPageSize = pageBounds, PageSize = pageBounds};
        var control = new TextControl(
            new FixedTextLayoutService(
                lineHeight: 15F,
                baselineOffset: 10F,
                lineTopOffset: -5F))
        {
            Text = "first\nsecond",
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        var textStyle = control.GetTextStyle();
        canvas.Translate(new Point(0F, 95F));

        control.Measure(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        control.Arrange(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, 90, pageBounds, CultureInfo.InvariantCulture);

        Assert.Equal(new Size(0F, 10F), additionalSize);
        canvas.AssertState();
        canvas.AssertClip(new Rectangle(0F, 95F, 10F, 40F));
        canvas.AssertDrawText(
            (textStyle, "first", 0F, 115F),
            (textStyle, "second", 0F, 130F));
    }

    [Fact]
    public void RenderUsesCanvasPageHeightInsteadOfContainerHeightForPagination()
    {
        var pageBounds = new Size(100, 100);
        var containerBounds = new Size(100, 40);
        var canvas = new DeferredCanvasMock{ActualPageSize = pageBounds, PageSize = pageBounds};
        var control = new TextControl(
            new FixedTextLayoutService(
                lineHeight: 15F,
                baselineOffset: 10F))
        {
            Text = "footer line",
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };
        var textStyle = control.GetTextStyle();
        canvas.Translate(new Point(0F, 30F));

        control.Measure(90, pageBounds, containerBounds, containerBounds, CultureInfo.InvariantCulture);
        control.Arrange(90, pageBounds, containerBounds, containerBounds, CultureInfo.InvariantCulture);
        var additionalSize = control.Render(canvas, 90, containerBounds, CultureInfo.InvariantCulture);

        Assert.Equal(Size.Zero, additionalSize);
        canvas.AssertState();
        canvas.AssertClip(new Rectangle(0F, 30F, 10F, 15F));
        canvas.AssertDrawText(textStyle, "footer line", 0F, 40F);
    }

    [Fact]
    public void RenderReusesArrangedLayoutForDeferredText()
    {
        var pageBounds = new Size(100, 100);
        var canvas = new DeferredCanvasMock{ActualPageSize = pageBounds, PageSize = pageBounds};
        var textService = new CountingTextLayoutService();
        var control = new TextControl(textService)
        {
            Text = "cached",
            HorizontalAlignment = EHorizontalAlignment.Left,
            VerticalAlignment = EVerticalAlignment.Top,
        };

        control.Measure(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        control.Arrange(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        control.Render(canvas, 90, pageBounds, CultureInfo.InvariantCulture);

        Assert.Equal(2, textService.MeasureCount);
        Assert.Equal(1, textService.LayoutCount);
        Assert.Equal(0, textService.DrawCount);
        canvas.AssertState();
        canvas.AssertDrawText(control.GetTextStyle(), "cached", 0F, 8F);
    }

    [Theory]
    [InlineData("underline", TextDecoration.Underline)]
    [InlineData("strikeThrough", TextDecoration.StrikeThrough)]
    [InlineData("doubleUnderline", TextDecoration.DoubleUnderline)]
    [InlineData("underline, strikeThrough", TextDecoration.Underline | TextDecoration.StrikeThrough)]
    public async Task XmlActivatesTextDecoration(string value, TextDecoration expected)
    {
        var control = await $"""<text decoration="{value}">Styled</text>""".ToControl<TextControl>();

        Assert.Equal(expected, control.Decoration);
        Assert.Equal(expected, control.GetTextStyle().Decoration);
    }

    [Fact(Skip = "This test is not working in CI environment")]
    public void LeftAlignedText()
    {
        const string text = "The quick brown fox jumps over the lazy dog";
        var pageBounds = new Size(595, 842);
        var mock = new DeferredCanvasMock{ActualPageSize = pageBounds, PageSize = pageBounds};
        var fontPath = GetTestFont();
        var control = new TextControl(new TextService(_paintCache))
        {
            Text       = text,
            FontSize   = 12,
            Style      = EFontStyle.Italic,
            FontFamily = fontPath,
        };
        var textStyle = control.GetTextStyle();
        control.Measure(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        control.Arrange(90, pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        control.Render(mock, 90, pageBounds, CultureInfo.InvariantCulture);
        mock.AssertState();
        mock.AssertDrawText(textStyle, text, 0, 12.9492188F);
    }

    private static string GetTestFont()
    {
        var fontPath = Path.GetFullPath(
            string.Join(
                Path.DirectorySeparatorChar,
                "..",
                "..",
                "..",
                "..",
                "fonts",
                "Nunito_Sans",
                "NunitoSans-Italic-VariableFont_YTLC,opsz,wdth,wght.ttf"));
        Assert.True(File.Exists(fontPath), $"Font file not found: {fontPath}");
        return fontPath;
    }

    private sealed class CountingTextLayoutService : ITextLayoutService
    {
        public int MeasureCount { get; private set; }
        public int LayoutCount { get; private set; }
        public int DrawCount { get; private set; }

        public Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
        {
            MeasureCount++;
            return text.IsEmpty ? Size.Zero : new Size(text.Length * 5F, 10F);
        }

        public void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
        {
            DrawCount++;
            foreach (var line in Layout(textStyle, dpi, text, maxWidth))
            {
                canvas.DrawText(textStyle, dpi, line.Text, line.X, line.BaselineY);
            }
        }

        public IReadOnlyList<TextLineLayout> Layout(
            TextStyle textStyle,
            float dpi,
            ReadOnlySpan<char> text,
            float maxWidth)
        {
            LayoutCount++;
            return text.IsEmpty
                ? Array.Empty<TextLineLayout>()
                : new[]
                {
                    new TextLineLayout(text.ToString(), 0F, 8F, 0F, 10F, text.Length * 5F),
                };
        }
    }
}
