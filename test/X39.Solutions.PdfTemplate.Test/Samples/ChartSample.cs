using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using SkiaSharp;
using X39.Solutions.PdfTemplate.Data;
using Xunit;

namespace X39.Solutions.PdfTemplate.Test.Samples;

[Collection("Samples")]
public class ChartSample : SampleBase
{
    [Fact]
    public async Task Sample1()
    {
        var       docOptions = new DocumentOptions
        {
            Margin = new Thickness(new Length(2, ELengthUnit.Centimeters)),
        };
        using var generator  = CreateGenerator();
        using var xmlStream = new MemoryStream(
            Encoding.UTF8.GetBytes(
                $$"""
                  <?xml version="1.0" encoding="utf-8"?>
                  <template xmlns="{{Constants.ControlsNamespace}}">
                      <body>
                        <chart>
                          <lineChart>
                            <data x="0" y="0" y-label="09-11-2024" />
                            <data x="2" y="1" y-label="10-11-2024" />
                            <data x="1" y="2" y-label="11-11-2024" />
                            <data x="5" y="3" y-label="12-11-2024" />
                            <data x="3" y="4" y-label="13-11-2024" />
                          </lineChart>
                          <barChart>
                            <data x="0" y="0" y-label="09-11-2024" />
                            <data x="2" y="1" y-label="10-11-2024" />
                            <data x="1" y="2" y-label="11-11-2024" />
                            <data x="5" y="3" y-label="12-11-2024" />
                            <data x="3" y="4" y-label="13-11-2024" />
                          </barChart>
                        </chart>
                      </body>
                  </template>
                  """
            )
        );
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader  = XmlReader.Create(xmlStream);
        // var       bitmaps = await generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture, docOptions);
        // using var fstream = new FileStream("output.png", FileMode.Create);
        // {
        //     bitmaps.First().Encode(fstream, SKEncodedImageFormat.Png, 100);
        // }
        await generator.GeneratePdfAsync(pdfStream, xmlReader, CultureInfo.InvariantCulture, docOptions);
    }

    [Fact]
    public async Task LineChart_SimpleData()
    {
        var docOptions = new DocumentOptions { Margin = new Thickness(new Length(2, ELengthUnit.Centimeters)) };
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(
            $$"""
            <?xml version="1.0" encoding="utf-8"?>
            <template xmlns="{{Constants.ControlsNamespace}}">
                <body>
                    <chart>
                        <lineChart height="300px" title="Sales Trend">
                            <data x="0" y="100" label="Jan" />
                            <data x="1" y="150" label="Feb" />
                            <data x="2" y="120" label="Mar" />
                            <data x="3" y="180" label="Apr" />
                            <data x="4" y="200" label="May" />
                        </lineChart>
                    </chart>
                </body>
            </template>
            """
        ));
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(pdfStream, xmlReader, CultureInfo.InvariantCulture, docOptions);
    }

    [Fact]
    public async Task LineChart_WithNegativeValues()
    {
        var docOptions = new DocumentOptions { Margin = new Thickness(new Length(2, ELengthUnit.Centimeters)) };
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(
            $$"""
            <?xml version="1.0" encoding="utf-8"?>
            <template xmlns="{{Constants.ControlsNamespace}}">
                <body>
                    <chart>
                        <lineChart height="300px" title="Temperature Changes">
                            <data x="0" y="-10" />
                            <data x="1" y="5" />
                            <data x="2" y="-5" />
                            <data x="3" y="15" />
                            <data x="4" y="0" />
                        </lineChart>
                    </chart>
                </body>
            </template>
            """
        ));
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(pdfStream, xmlReader, CultureInfo.InvariantCulture, docOptions);
    }

    [Fact]
    public async Task BarChart_VerticalBars()
    {
        var docOptions = new DocumentOptions { Margin = new Thickness(new Length(2, ELengthUnit.Centimeters)) };
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(
            $$"""
            <?xml version="1.0" encoding="utf-8"?>
            <template xmlns="{{Constants.ControlsNamespace}}">
                <body>
                    <chart>
                        <barChart height="300px" title="Revenue by Quarter" orientation="Vertical">
                            <data x="0" y="50" label="Q1" />
                            <data x="1" y="75" label="Q2" />
                            <data x="2" y="60" label="Q3" />
                            <data x="3" y="90" label="Q4" />
                        </barChart>
                    </chart>
                </body>
            </template>
            """
        ));
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(pdfStream, xmlReader, CultureInfo.InvariantCulture, docOptions);
    }

    [Fact]
    public async Task BarChart_HorizontalBars()
    {
        var docOptions = new DocumentOptions { Margin = new Thickness(new Length(2, ELengthUnit.Centimeters)) };
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(
            $$"""
            <?xml version="1.0" encoding="utf-8"?>
            <template xmlns="{{Constants.ControlsNamespace}}">
                <body>
                    <chart>
                        <barChart height="300px" title="Horizontal Bars" orientation="Horizontal">
                            <data x="0" y="30" />
                            <data x="1" y="45" />
                            <data x="2" y="25" />
                        </barChart>
                    </chart>
                </body>
            </template>
            """
        ));
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(pdfStream, xmlReader, CultureInfo.InvariantCulture, docOptions);
    }

    [Fact]
    public async Task PieChart_BasicPie()
    {
        var docOptions = new DocumentOptions { Margin = new Thickness(new Length(2, ELengthUnit.Centimeters)) };
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(
            $$"""
            <?xml version="1.0" encoding="utf-8"?>
            <template xmlns="{{Constants.ControlsNamespace}}">
                <body>
                    <chart>
                        <pieChart height="400px" title="Market Share">
                            <data y="35" label="Product A" />
                            <data y="25" label="Product B" />
                            <data y="20" label="Product C" />
                            <data y="20" label="Product D" />
                        </pieChart>
                    </chart>
                </body>
            </template>
            """
        ));
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(pdfStream, xmlReader, CultureInfo.InvariantCulture, docOptions);
    }

    [Fact]
    public async Task PieChart_DonutChart()
    {
        var docOptions = new DocumentOptions { Margin = new Thickness(new Length(2, ELengthUnit.Centimeters)) };
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(
            $$"""
            <?xml version="1.0" encoding="utf-8"?>
            <template xmlns="{{Constants.ControlsNamespace}}">
                <body>
                    <chart>
                        <pieChart height="400px" title="Donut Chart" inner-radius="50%">
                            <data y="40" label="Category 1" />
                            <data y="30" label="Category 2" />
                            <data y="30" label="Category 3" />
                        </pieChart>
                    </chart>
                </body>
            </template>
            """
        ));
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(pdfStream, xmlReader, CultureInfo.InvariantCulture, docOptions);
    }

    [Fact]
    public async Task MixedCharts_LineAndBar()
    {
        var docOptions = new DocumentOptions { Margin = new Thickness(new Length(2, ELengthUnit.Centimeters)) };
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(
            $$"""
            <?xml version="1.0" encoding="utf-8"?>
            <template xmlns="{{Constants.ControlsNamespace}}">
                <body>
                    <chart>
                        <lineChart height="250px" title="Line Chart">
                            <data x="0" y="10" />
                            <data x="1" y="20" />
                            <data x="2" y="15" />
                        </lineChart>
                        <barChart height="250px" title="Bar Chart">
                            <data x="0" y="30" />
                            <data x="1" y="40" />
                            <data x="2" y="35" />
                        </barChart>
                    </chart>
                </body>
            </template>
            """
        ));
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(pdfStream, xmlReader, CultureInfo.InvariantCulture, docOptions);
    }

    [Fact]
    public async Task ComplexChart_AllFeatures()
    {
        var docOptions = new DocumentOptions { Margin = new Thickness(new Length(2, ELengthUnit.Centimeters)) };
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(
            $$"""
            <?xml version="1.0" encoding="utf-8"?>
            <template xmlns="{{Constants.ControlsNamespace}}">
                <body>
                    <chart>
                        <lineChart height="300px" title="Sales Trend with Grid" show-grid="true">
                            <data x="0" y="100" />
                            <data x="1" y="150" />
                            <data x="2" y="120" />
                            <data x="3" y="180" />
                        </lineChart>
                    </chart>
                </body>
            </template>
            """
        ));
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(pdfStream, xmlReader, CultureInfo.InvariantCulture, docOptions);
    }

    [Fact]
    public async Task EdgeCase_SingleDataPoint()
    {
        var docOptions = new DocumentOptions { Margin = new Thickness(new Length(2, ELengthUnit.Centimeters)) };
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(
            $$"""
            <?xml version="1.0" encoding="utf-8"?>
            <template xmlns="{{Constants.ControlsNamespace}}">
                <body>
                    <chart>
                        <lineChart height="200px" title="Single Point">
                            <data x="5" y="100" />
                        </lineChart>
                        <barChart height="200px" title="Single Bar">
                            <data x="0" y="50" />
                        </barChart>
                    </chart>
                </body>
            </template>
            """
        ));
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(pdfStream, xmlReader, CultureInfo.InvariantCulture, docOptions);
    }
}
