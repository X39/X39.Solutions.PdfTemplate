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
public class AreaSample : SampleBase
{
    [Fact]
    public async Task AreaInEachCorner()
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
                      <areas>
                          <area width="20%" height="5%" left="0" top="0">
                            <border background="red"><text>TOP-LEFT</text></border>
                          </area>
                          <area width="20%" height="5%" left="0" bottom="0">
                            <border background="red"><text>BOTTOM-LEFT</text></border>
                          </area>
                          <area width="20%" height="5%" right="0" top="0">
                            <border background="red"><text>TOP-RIGHT</text></border>
                          </area>
                          <area width="20%" height="5%" right="0" bottom="0">
                            <border background="red"><text>BOTTOM-RIGHT</text></border>
                          </area>
                      </areas>
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
    public async Task AreaInEachCornerWithTextOnBody()
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
                    <areas>
                        <area width="20%" height="5%" left="0" top="0">
                          <border background="red"><text>TOP-LEFT</text></border>
                        </area>
                        <area width="20%" height="5%" left="0" bottom="0">
                          <border background="red"><text>BOTTOM-LEFT</text></border>
                        </area>
                        <area width="20%" height="5%" right="0" top="0">
                          <border background="red"><text>TOP-RIGHT</text></border>
                        </area>
                        <area width="20%" height="5%" right="0" bottom="0">
                          <border background="red"><text>BOTTOM-RIGHT</text></border>
                        </area>
                    </areas>
                    <body>
                        @for i from 0 to 1000 {
                          <text>Lorem ipsum dolor sit amet, consectetur adipiscing elit. Amet exercitation sea facilisi velit lorem invidunt. Elit option sadipscing accumsan quod congue. Ea kasd eleifend tincidunt amet deserunt nostrud, iure facer molestie in augue fugiat pariatur eros blandit eleifend volutpat at est. Assum sanctus zzril sed sunt invidunt exerci. Ullamcorper duis in.</text>
                        }
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
}
