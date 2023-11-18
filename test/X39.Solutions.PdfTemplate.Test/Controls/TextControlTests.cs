using System.Globalization;
using X39.Solutions.PdfTemplate.Controls;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services;
using X39.Solutions.PdfTemplate.Services.TextService;
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
    public void SizeGreaterZero(string s)
    {
        const string text = "The quick brown fox jumps over the lazy dog";
        var pageBounds = new Size(595, 842);
        var mock = new CanvasMock();
        var fontPath = GetTestFont();
        var control = new TextControl(new TextService(_paintCache))
        {
            Text       = text,
            FontSize   = 12,
            Style      = EFontStyle.Italic,
            FontFamily = fontPath,
        };
        control.Measure(pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        control.Arrange(pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        control.Render(mock, pageBounds, CultureInfo.InvariantCulture);
        mock.AssertState();
        mock.AssertAllClip((rectangle) => rectangle is {Width: > 0, Height: > 0});
    }

    [Fact(Skip = "This test is not working in CI environment")]
    public void LeftAlignedText()
    {
        const string text = "The quick brown fox jumps over the lazy dog";
        var pageBounds = new Size(595, 842);
        var mock = new CanvasMock();
        var fontPath = GetTestFont();
        var control = new TextControl(new TextService(_paintCache))
        {
            Text       = text,
            FontSize   = 12,
            Style      = EFontStyle.Italic,
            FontFamily = fontPath,
        };
        var textStyle = control.GetTextStyle();
        control.Measure(pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        control.Arrange(pageBounds, pageBounds, pageBounds, CultureInfo.InvariantCulture);
        control.Render(mock, pageBounds, CultureInfo.InvariantCulture);
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
}