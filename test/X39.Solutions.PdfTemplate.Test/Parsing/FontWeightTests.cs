using System.ComponentModel;
using System.Globalization;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Test.Parsing;

public class FontWeightTests
{
    [Theory]
    [InlineData("100", 100)]
    [InlineData("200", 200)]
    [InlineData("300", 300)]
    [InlineData("400", 400)]
    [InlineData("500", 500)]
    [InlineData("600", 600)]
    [InlineData("700", 700)]
    [InlineData("800", 800)]
    [InlineData("900", 900)]
    public void TestParse(string input, ushort expected)
    {
        var actual = FontWeight.Parse(input, CultureInfo.InvariantCulture);
        Assert.Equal(expected, actual.Value);
        Assert.Equal(input, actual.ToString());
    }
    
    [Theory]
    [InlineData("thin", 100)]
    [InlineData("extralight", 200)]
    [InlineData("light", 300)]
    [InlineData("normal", 400)]
    [InlineData("medium", 500)]
    [InlineData("semibold", 600)]
    [InlineData("bold", 700)]
    [InlineData("extrabold", 800)]
    [InlineData("black", 900)]
    public void TestParseCommonNamesWithTypeConverter(string input, ushort expected)
    {
        var converter = TypeDescriptor.GetConverter(typeof(FontWeight));
        var actual = converter.ConvertFromInvariantString(input);
        Assert.NotNull(actual);
        Assert.IsType<FontWeight>(actual);
        Assert.Equal(expected, ((FontWeight)actual).Value);
    }
    
    [Theory]
    [InlineData("100", 100)]
    [InlineData("200", 200)]
    [InlineData("300", 300)]
    [InlineData("400", 400)]
    [InlineData("500", 500)]
    [InlineData("600", 600)]
    [InlineData("700", 700)]
    [InlineData("800", 800)]
    [InlineData("900", 900)]
    public void TestParseCommonNames(string input, ushort expected)
    {
        var converter = TypeDescriptor.GetConverter(typeof(FontWeight));
        var actual    = converter.ConvertFromInvariantString(input);
        Assert.NotNull(actual);
        Assert.IsType<FontWeight>(actual);
        Assert.Equal(expected, ((FontWeight)actual).Value);
    }
    
}
