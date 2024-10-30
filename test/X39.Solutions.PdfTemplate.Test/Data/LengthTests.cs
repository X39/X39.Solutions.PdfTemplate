using System.Globalization;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Test.Data;

public class LengthTests
{
    [Theory]
    [InlineData(10, ELengthUnit.Millimeters, 1)]
    [InlineData(100, ELengthUnit.Millimeters, 10)]
    [InlineData(1000, ELengthUnit.Millimeters, 100)]
    [InlineData(1, ELengthUnit.Centimeters, 1)]
    [InlineData(10, ELengthUnit.Centimeters, 10)]
    [InlineData(100, ELengthUnit.Centimeters, 100)]
    public void MeterTests(float value, ELengthUnit unit, float expectedCentiMeter, int decimalPlaces = 4)
    {
        const float dpiOneCentiMeter = 1 / 0.393701F;
        var length = new Length(value, unit);
        Assert.Equal(expectedCentiMeter, length.ToPixels(100, dpiOneCentiMeter), decimalPlaces);
    }

    [Theory]
    [InlineData("1mm", 1, ELengthUnit.Millimeters)]
    [InlineData("10mm", 10, ELengthUnit.Millimeters)]
    [InlineData("100mm", 100, ELengthUnit.Millimeters)]
    [InlineData("1cm", 1, ELengthUnit.Centimeters)]
    [InlineData("10cm", 10, ELengthUnit.Centimeters)]
    [InlineData("100cm", 100, ELengthUnit.Centimeters)]
    [InlineData("1%", 0.01, ELengthUnit.Percent)]
    [InlineData("10%", 0.1, ELengthUnit.Percent)]
    [InlineData("100%", 1, ELengthUnit.Percent)]
    [InlineData("1px", 1, ELengthUnit.Pixel)]
    [InlineData("10px", 10, ELengthUnit.Pixel)]
    [InlineData("100px", 100, ELengthUnit.Pixel)]
    [InlineData("1pt", 1, ELengthUnit.Points)]
    [InlineData("10pt", 10, ELengthUnit.Points)]
    [InlineData("100pt", 100, ELengthUnit.Points)]
    public void ParseTest(string input, float value, ELengthUnit unit, int decimalPlaces = 4)
    {
        var lenght = Length.Parse(input, CultureInfo.InvariantCulture);
        Assert.Equal(value, lenght.Value, decimalPlaces);
        Assert.Equal(unit, lenght.Unit);
    }
}