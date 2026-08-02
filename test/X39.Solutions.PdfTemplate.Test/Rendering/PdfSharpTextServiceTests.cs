using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Rendering.PdfSharp.Services;
using X39.Solutions.PdfTemplate.Test.Mock;

namespace X39.Solutions.PdfTemplate.Test.Rendering;

public sealed class PdfSharpTextServiceTests
{
    private const float Dpi = 900F;

    [Theory]
    [InlineData(0.5F)]
    [InlineData(2F)]
    public void MeasureReturnsZeroForEmptyTextWithNonDefaultLineHeight(float lineHeight)
    {
        var textService = new PdfSharpTextService();
        var textStyle = new TextStyle
        {
            LineHeight = lineHeight,
        };

        var measured = textService.Measure(textStyle, Dpi, ReadOnlySpan<char>.Empty, 100F);

        Assert.Equal(Size.Zero, measured);
    }

    [Fact]
    public void LayoutAndDrawAreNoOpsForEmptyText()
    {
        var textService = new PdfSharpTextService();
        var textStyle = new TextStyle
        {
            LineHeight = 2F,
        };
        var canvas = new DeferredCanvasMock
        {
            ActualPageSize = new Size(100F, 100F),
            PageSize = new Size(100F, 100F),
        };

        var layout = textService.Layout(textStyle, Dpi, ReadOnlySpan<char>.Empty, 100F);
        textService.Draw(canvas, textStyle, Dpi, ReadOnlySpan<char>.Empty, 100F);

        Assert.Empty(layout);
        Assert.Equal(0, canvas.DrawTextCount);
        canvas.AssertState();
    }

    [Fact]
    public void MeasureUsesActualGlyphWidths()
    {
        var textService = new PdfSharpTextService();
        var textStyle = CreateTextStyleWithTestFont();

        var narrow = textService.Measure(textStyle, Dpi, "iiiiiiiiii".AsSpan(), float.MaxValue);
        var wide = textService.Measure(textStyle, Dpi, "WWWWWWWWWW".AsSpan(), float.MaxValue);

        Assert.True(wide.Width > narrow.Width, $"Expected W glyphs ({wide.Width}) to measure wider than i glyphs ({narrow.Width}).");
    }

    [Fact]
    public void MeasureUsesFullPdfSharpFontHeightIncludingDescent()
    {
        var textService = new PdfSharpTextService();
        var textStyle = CreateTextStyleWithTestFont();
        var font = PdfSharpFontHelper.CreateFont(textStyle, Dpi);
        var expectedHeight = (float) font.GetHeight();

        var measured = textService.Measure(textStyle, Dpi, "glyph".AsSpan(), float.MaxValue);
        var layout = Assert.Single(textService.Layout(textStyle, Dpi, "glyph".AsSpan(), float.MaxValue));

        Assert.Equal(expectedHeight, measured.Height, 3);
        Assert.Equal(expectedHeight, layout.Height, 3);
        Assert.Equal(0F, layout.Top);
        Assert.True(layout.BaselineY < layout.Height);
        Assert.True(layout.BaselineY > 0F);
    }

    [Fact]
    public void MeasureAppliesLineHeightToBaselineAdvanceWithoutClippingLastLine()
    {
        var textService = new PdfSharpTextService();
        var textStyle = CreateTextStyleWithTestFont() with { LineHeight = 0.5F };
        var font = PdfSharpFontHelper.CreateFont(textStyle, Dpi);
        var fontHeight = (float) font.GetHeight();

        var measured = textService.Measure(textStyle, Dpi, "first\nsecond".AsSpan(), float.MaxValue);
        var layout = textService.Layout(textStyle, Dpi, "first\nsecond".AsSpan(), float.MaxValue);

        Assert.Equal(2, layout.Count);
        Assert.Equal(fontHeight * 1.5F, measured.Height, 3);
        Assert.Equal(fontHeight * 0.5F, layout[1].Top, 3);
        Assert.Equal(fontHeight, layout[1].Height, 3);
    }

    [Fact]
    public void LayoutAtHighDpiWrapsByMeasuredWidthInsteadOfCharacterCount()
    {
        var textService = new PdfSharpTextService();
        var textStyle = CreateTextStyleWithTestFont();
        const string narrowText = "iiiiiiiiii";
        const string wideText = "WWWWWWWWWW";
        var narrowWidth = textService.Measure(textStyle, Dpi, narrowText.AsSpan(), float.MaxValue).Width;
        var wideWidth = textService.Measure(textStyle, Dpi, wideText.AsSpan(), float.MaxValue).Width;
        var maxWidth = (narrowWidth + wideWidth) / 2F;

        var narrowLayout = textService.Layout(textStyle, Dpi, narrowText.AsSpan(), maxWidth);
        var wideLayout = textService.Layout(textStyle, Dpi, wideText.AsSpan(), maxWidth);

        Assert.Single(narrowLayout);
        Assert.True(wideLayout.Count > 1);
        Assert.Equal(wideText, string.Concat(wideLayout.Select((q) => q.Text)));
    }

    [Fact]
    public void LayoutSplitsLongUnbrokenTokensToFitMaxWidth()
    {
        var textService = new PdfSharpTextService();
        var textStyle = new TextStyle();
        const string text = "WWWWWWWWWWWW";
        var glyphWidth = textService.Measure(textStyle, Dpi, "W".AsSpan(), float.MaxValue).Width;
        var maxWidth = glyphWidth * 3.5F;

        var layout = textService.Layout(textStyle, Dpi, text.AsSpan(), maxWidth);

        Assert.True(layout.Count > 1);
        Assert.Equal(text, string.Concat(layout.Select((q) => q.Text)));
        Assert.All(layout, (line) => Assert.True(
            line.Width <= maxWidth + 0.001F,
            $"Line '{line.Text}' measured {line.Width}, exceeding max width {maxWidth}."));
    }

    [Fact]
    public void FontHelperMapsWeightThresholdToBold()
    {
        var regular = PdfSharpFontHelper.CreateFont(
            new TextStyle
            {
                FontFamily = Font.Default with { Weight = new FontWeight(599) },
            },
            Dpi);
        var bold = PdfSharpFontHelper.CreateFont(
            new TextStyle
            {
                FontFamily = Font.Default with { Weight = new FontWeight(600) },
            },
            Dpi);

        Assert.False(regular.Bold);
        Assert.True(bold.Bold);
    }

    private static TextStyle CreateTextStyleWithTestFont()
        => new()
        {
            FontFamily = new Font(GetTestFont()),
        };

    private static string GetTestFont()
    {
        var fontPath = Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "fonts",
                "Nunito_Sans",
                "NunitoSans-VariableFont_YTLC,opsz,wdth,wght.ttf"));
        Assert.True(File.Exists(fontPath), $"Font file not found: {fontPath}");
        return fontPath;
    }
}
