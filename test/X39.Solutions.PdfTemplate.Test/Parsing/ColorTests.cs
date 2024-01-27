using System.ComponentModel;
using System.Globalization;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Test.Parsing;

public class ColorTests
{
    [Theory]
    [InlineData("#000",      0x00, 0x00, 0x00, 0xFF)]
    [InlineData("#0000",     0x00, 0x00, 0x00, 0x00)]
    [InlineData("#FFF",      0xFF, 0xFF, 0xFF, 0xFF)]
    [InlineData("#FFFF",     0xFF, 0xFF, 0xFF, 0xFF)]
    [InlineData("#000000",   0x00, 0x00, 0x00, 0xFF)]
    [InlineData("#FFFFFF",   0xFF, 0xFF, 0xFF, 0xFF)]
    [InlineData("#00000000", 0x00, 0x00, 0x00, 0x00)]
    [InlineData("#11111111", 0x11, 0x11, 0x11, 0x11)]
    [InlineData("#22222222", 0x22, 0x22, 0x22, 0x22)]
    [InlineData("#33333333", 0x33, 0x33, 0x33, 0x33)]
    [InlineData("#44444444", 0x44, 0x44, 0x44, 0x44)]
    [InlineData("#55555555", 0x55, 0x55, 0x55, 0x55)]
    [InlineData("#66666666", 0x66, 0x66, 0x66, 0x66)]
    [InlineData("#77777777", 0x77, 0x77, 0x77, 0x77)]
    [InlineData("#88888888", 0x88, 0x88, 0x88, 0x88)]
    [InlineData("#99999999", 0x99, 0x99, 0x99, 0x99)]
    [InlineData("#AAAAAAAA", 0xAA, 0xAA, 0xAA, 0xAA)]
    [InlineData("#BBBBBBBB", 0xBB, 0xBB, 0xBB, 0xBB)]
    [InlineData("#CCCCCCCC", 0xCC, 0xCC, 0xCC, 0xCC)]
    [InlineData("#DDDDDDDD", 0xDD, 0xDD, 0xDD, 0xDD)]
    [InlineData("#EEEEEEEE", 0xEE, 0xEE, 0xEE, 0xEE)]
    [InlineData("#FFFFFFFF", 0xFF, 0xFF, 0xFF, 0xFF)]
    public void TestNumberWithParse(string input, byte r, byte g, byte b, byte a)
    {
        var actual = Color.Parse(input, CultureInfo.InvariantCulture);
        Assert.Equal(r,     actual.Red);
        Assert.Equal(g,     actual.Green);
        Assert.Equal(b,     actual.Blue);
        Assert.Equal(a,     actual.Alpha);
    }

    [Theory]
    [InlineData("#000",      0x00, 0x00, 0x00, 0xFF)]
    [InlineData("#0000",     0x00, 0x00, 0x00, 0x00)]
    [InlineData("#FFF",      0xFF, 0xFF, 0xFF, 0xFF)]
    [InlineData("#FFFF",     0xFF, 0xFF, 0xFF, 0xFF)]
    [InlineData("#000000",   0x00, 0x00, 0x00, 0xFF)]
    [InlineData("#FFFFFF",   0xFF, 0xFF, 0xFF, 0xFF)]
    [InlineData("#00000000", 0x00, 0x00, 0x00, 0x00)]
    [InlineData("#11111111", 0x11, 0x11, 0x11, 0x11)]
    [InlineData("#22222222", 0x22, 0x22, 0x22, 0x22)]
    [InlineData("#33333333", 0x33, 0x33, 0x33, 0x33)]
    [InlineData("#44444444", 0x44, 0x44, 0x44, 0x44)]
    [InlineData("#55555555", 0x55, 0x55, 0x55, 0x55)]
    [InlineData("#66666666", 0x66, 0x66, 0x66, 0x66)]
    [InlineData("#77777777", 0x77, 0x77, 0x77, 0x77)]
    [InlineData("#88888888", 0x88, 0x88, 0x88, 0x88)]
    [InlineData("#99999999", 0x99, 0x99, 0x99, 0x99)]
    [InlineData("#AAAAAAAA", 0xAA, 0xAA, 0xAA, 0xAA)]
    [InlineData("#BBBBBBBB", 0xBB, 0xBB, 0xBB, 0xBB)]
    [InlineData("#CCCCCCCC", 0xCC, 0xCC, 0xCC, 0xCC)]
    [InlineData("#DDDDDDDD", 0xDD, 0xDD, 0xDD, 0xDD)]
    [InlineData("#EEEEEEEE", 0xEE, 0xEE, 0xEE, 0xEE)]
    [InlineData("#FFFFFFFF", 0xFF, 0xFF, 0xFF, 0xFF)]
    public void TestNumberWithTypeConverter(string input, byte r, byte g, byte b, byte a)
    {
        var converter = TypeDescriptor.GetConverter(typeof(Color));
        var actual    = converter.ConvertFromInvariantString(input);
        Assert.NotNull(actual);
        Assert.IsType<Color>(actual);
        Assert.Equal(r,     ((Color) actual).Red);
        Assert.Equal(g,     ((Color) actual).Green);
        Assert.Equal(b,     ((Color) actual).Blue);
        Assert.Equal(a,     ((Color) actual).Alpha);
    }

    [Theory]
    [InlineData("red",         0xFF, 0x00, 0x00, 0xFF)]
    [InlineData("green",       0x00, 0xFF, 0x00, 0xFF)]
    [InlineData("blue",        0x00, 0x00, 0xFF, 0xFF)]
    [InlineData("yellow",      0xFF, 0xFF, 0x00, 0xFF)]
    [InlineData("cyan",        0x00, 0xFF, 0xFF, 0xFF)]
    [InlineData("magenta",     0xFF, 0x00, 0xFF, 0xFF)]
    [InlineData("black",       0x00, 0x00, 0x00, 0xFF)]
    [InlineData("white",       0xFF, 0xFF, 0xFF, 0xFF)]
    [InlineData("transparent", 0x00, 0x00, 0x00, 0x00)]
    public void TestNamedColorsWithTypeConverter(string input, byte r, byte g, byte b, byte a)
    {
        var converter = TypeDescriptor.GetConverter(typeof(Color));
        var actual    = converter.ConvertFromInvariantString(input);
        Assert.NotNull(actual);
        Assert.IsType<Color>(actual);
        Assert.Equal(r,     ((Color) actual).Red);
        Assert.Equal(g,     ((Color) actual).Green);
        Assert.Equal(b,     ((Color) actual).Blue);
        Assert.Equal(a,     ((Color) actual).Alpha);
    }
}
