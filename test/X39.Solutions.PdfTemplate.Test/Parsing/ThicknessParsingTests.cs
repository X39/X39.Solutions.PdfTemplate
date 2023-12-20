using System.Globalization;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Test.Parsing;

public class ThicknessParsingTests
{
    [Fact]
    public void ParseSinglePixelValue()
    {
        var thickness = Thickness.Parse("1px", CultureInfo.InvariantCulture);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Left);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Top);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Right);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Bottom);
    }
    
    [Fact]
    public void ParseSinglePercentValue()
    {
        var thickness = Thickness.Parse("1%", CultureInfo.InvariantCulture);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Left);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Top);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Right);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Bottom);
    }
    
    [Fact]
    public void ParseTwoPixelValues()
    {
        var thickness = Thickness.Parse("1px 2px", CultureInfo.InvariantCulture);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Left);
        Assert.Equal(new Length(2, ELengthUnit.Pixel), thickness.Top);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Right);
        Assert.Equal(new Length(2, ELengthUnit.Pixel), thickness.Bottom);
    }
    
    [Fact]
    public void ParseTwoPercentValues()
    {
        var thickness = Thickness.Parse("1% 2%", CultureInfo.InvariantCulture);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Left);
        Assert.Equal(new Length(0.02F, ELengthUnit.Percent), thickness.Top);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Right);
        Assert.Equal(new Length(0.02F, ELengthUnit.Percent), thickness.Bottom);
    }
    
    [Fact]
    public void ParseFourPixelValues()
    {
        var thickness = Thickness.Parse("1px 2px 3px 4px", CultureInfo.InvariantCulture);
        Assert.Equal(new Length(1, ELengthUnit.Pixel), thickness.Left);
        Assert.Equal(new Length(2, ELengthUnit.Pixel), thickness.Top);
        Assert.Equal(new Length(3, ELengthUnit.Pixel), thickness.Right);
        Assert.Equal(new Length(4, ELengthUnit.Pixel), thickness.Bottom);
    }
    
    [Fact]
    public void ParseFourPercentValues()
    {
        var thickness = Thickness.Parse("1% 2% 3% 4%", CultureInfo.InvariantCulture);
        Assert.Equal(new Length(0.01F, ELengthUnit.Percent), thickness.Left);
        Assert.Equal(new Length(0.02F, ELengthUnit.Percent), thickness.Top);
        Assert.Equal(new Length(0.03F, ELengthUnit.Percent), thickness.Right);
        Assert.Equal(new Length(0.04F, ELengthUnit.Percent), thickness.Bottom);
    }
}