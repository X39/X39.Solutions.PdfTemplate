using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Test.ExpressionTests;

namespace X39.Solutions.PdfTemplate.Test.Samples;

[Collection("Samples")]
public class TableSample : SampleBase
{
    [Fact]
    public async Task SimpleTable()
    {
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(
            Encoding.UTF8.GetBytes(
                $$"""
                  <?xml version="1.0" encoding="utf-8"?>
                  <template xmlns="{{Constants.ControlsNamespace}}">
                      <body>
                         <table>
                             <th>
                                 <td><text>Header 1</text></td>
                                 <td><text>Header 2</text></td>
                                 <td><text>Header 3</text></td>
                             </th>
                             @for i from 1 to 101 {
                                 <tr>
                                     <td><text>Row @i Column 1</text></td>
                                     <td><text>Row @i Column 2</text></td>
                                     <td><text>Row @i Column 3</text></td>
                                 </tr>
                             }
                         </table>
                      </body>
                  </template>
                  """
            )
        );
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader  = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(
            pdfStream,
            xmlReader,
            CultureInfo.InvariantCulture
        );
    }

    [Fact]
    public async Task WidthWeightedTable()
    {
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(
            Encoding.UTF8.GetBytes(
                $$"""
                  <?xml version="1.0" encoding="utf-8"?>
                  <template xmlns="{{Constants.ControlsNamespace}}">
                      <body>
                         <table>
                             <th>
                                 <td width="1*"><text>Header 1</text></td>
                                 <td width="50%"><text>Header 2</text></td>
                                 <td width="2*"><text>Header 3</text></td>
                                 <td width="auto"><text>Header 4</text></td>
                             </th>
                             @for i from 1 to 101 {
                                 <tr>
                                     <td><text>Row @i Column 1</text></td>
                                     <td><text>Row @i Column 2</text></td>
                                     <td><text>Row @i Column 3</text></td>
                                     <td><text>Row @i Column 4</text></td>
                                 </tr>
                             }
                         </table>
                      </body>
                  </template>
                  """
            )
        );
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader  = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(
            pdfStream,
            xmlReader,
            CultureInfo.InvariantCulture
        );
    }

    [Fact]
    public async Task LongTableRows()
    {
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(
            Encoding.UTF8.GetBytes(
                $$"""
                  <?xml version="1.0" encoding="utf-8"?>
                  <template xmlns="{{Constants.ControlsNamespace}}">
                      <header>
                          <text margin="0 0 0 5%">Text present in header</text>
                          <line thickness="1px" length="100%"/>
                      </header>
                      <body>
                         <table>
                             <th>
                                 <td><text>Header 1</text></td>
                                 <td><text>Header 2</text></td>
                                 <td><text>Header 3</text></td>
                             </th>
                             @for i from 0 to 100 {
                                 <tr>
                                     <td><text>The row @i at column 1 is way to long to be displayed in a single row so it has to be split by the system automagically without any user input ever done but providing text</text></td>
                                     <td><text>The row @i at column 1 is way to long to be displayed in a single row so it has to be split by the system automagically without any user input ever done but providing text</text></td>
                                     <td><text>The row @i at column 1 is way to long to be displayed in a single row so it has to be split by the system automagically without any user input ever done but providing text</text></td>
                                 </tr>
                             }
                         </table>
                      </body>
                      <footer>
                          <line thickness="1px" length="100%"/>
                          <text margin="0 5% 0 0">More text in the footer</text>
                      </footer>
                  </template>
                  """
            )
        );
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader  = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(
            pdfStream,
            xmlReader,
            CultureInfo.InvariantCulture
        );
    }

    [Fact]
    public async Task SomeSampleForInventoryKeeping()
    {
        var random = new Random();
        using var generator = CreateGenerator(
            new DummyValueFunction(
                "rnd",
                (para) =>
                {
                    if (para.Length != 2)
                        throw new ArgumentException("Invalid arguments.", nameof(para));
                    if (para[0] is not int para1 || para[1] is not int para2)
                        throw new ArgumentException("Invalid arguments.", nameof(para));
                    return random.Next(para1, para2);
                },
            new[] {typeof(int), typeof(int)}));
        using var xmlStream = new MemoryStream(
            Encoding.UTF8.GetBytes(
                $$"""
                  <?xml version="1.0" encoding="utf-8"?>
                  <template xmlns="{{Constants.ControlsNamespace}}">
                      <body>
                        <table>
                            <th>
                                <th.style>
                                    <text fontsize="20" weight="bold"/>
                                </th.style>
                                <td width="1*"><text>Storage</text></td>
                                <td width="auto"><text>Item</text></td>
                                <td width="auto"><text>Expected</text></td>
                                <td width="auto"><text>Difference</text></td>
                                <td width="auto"><text>Actual</text></td>
                            </th>
                            @for i from 0 to 100 {
                                <tr>
                                    <td><text>Box @i</text></td>
                                    <td><text>Item @rnd(1000, 100000)</text></td>
                                    <td><text>@rnd(1, 100)</text></td>
                                    <td></td>
                                    <td></td>
                                </tr>
                            }
                        </table>
                      </body>
                  </template>
                  """
            )
        );
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader  = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(
            pdfStream,
            xmlReader,
            CultureInfo.InvariantCulture
        );
    }
}
