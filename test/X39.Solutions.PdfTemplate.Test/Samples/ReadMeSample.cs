using System.Globalization;
using System.Text;
using System.Xml;
using SkiaSharp;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Test.Samples;

[Collection("Samples")]
public class ReadMeSample : SampleBase
{
    [Fact]
    public async Task SimpleTable()
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
                      <template.style>
                          <text padding="0.125cm" />
                      </template.style>
                      <header>
                        <text fontsize="20">Invoice #1234</text>
                        <text>Issue date 12.12.2024</text>
                        <text>Due date 12.12.2024</text>
                      </header>
                      <body>
                         <table margin="0 1cm">
                            <tr>
                                <td width="45%">
                                    <text>From:</text>
                                    <line thickness="1pt" length="100%"/>
                                    <text>John Doe</text>
                                    <text>1234 Main Street</text>
                                    <text>Springfield, IL 62701</text>
                                    <text>United States</text>
                                </td>
                                <td width="10%"/>
                                <td width="45%">
                                    <text>To:</text>
                                    <line thickness="1pt" length="100%"/>
                                    <text>Jane Doe</text>
                                    <text>1234 Main Street</text>
                                    <text>Springfield, IL 62701</text>
                                    <text>United States</text>
                                </td>
                            </tr>
                         </table>
                         
                         <table>
                            <th>
                                <td>
                                    <border color="black" thickness="0 0 0 1pt">
                                        <text>#</text>
                                    </border>
                                </td>
                                <td>
                                    <border color="black" thickness="0 0 0 1pt">
                                        <text>Product</text>
                                    </border>
                                </td>
                                <td>
                                    <border color="black" thickness="0 0 0 1pt">
                                        <text horizontalAlignment="right">Price</text>
                                    </border>
                                </td>
                                <td>
                                    <border color="black" thickness="0 0 0 1pt">
                                        <text horizontalAlignment="right">Quantity</text>
                                    </border>
                                </td>
                                <td>
                                    <border color="black" thickness="0 0 0 1pt">
                                        <text horizontalAlignment="right">Total</text>
                                    </border>
                                </td>
                            </th>
                            @alternate on value with ["#f0f0f0", "#ffffff"] {
                            <tr>
                                <td><border background="@value"><text>1</text></border></td>
                                <td><border background="@value"><text>Fancy shirt</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">100.00 $</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">1</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">100.00 $</text></border></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr>
                                <td><border background="@value"><text>2</text></border></td>
                                <td><border background="@value"><text>Shoes</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">50.00 $</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">2</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">100.00 $</text></border></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr>
                                <td><border background="@value"><text>3</text></border></td>
                                <td><border background="@value"><text>Jeans</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">74.99 $</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">1</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">74.99 $</text></border></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr>
                                <td><border background="@value"><text>4</text></border></td>
                                <td><border background="@value"><text>Hat</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">24.99 $</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">1</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">24.99 $</text></border></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr>
                                <td><border background="@value"><text>5</text></border></td>
                                <td><border background="@value"><text>Watch</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">200.00 $</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">1</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">200.00 $</text></border></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr>
                                <td><border background="@value"><text>6</text></border></td>
                                <td><border background="@value"><text>Exquisite Fountain Pen</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">300.00 $</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">1</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">300.00 $</text></border></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr>
                                <td><border background="@value"><text>7</text></border></td>
                                <td><border background="@value"><text>Leather-bound Notebook</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">70.00 $</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">3</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">210.00 $</text></border></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr>
                                <td><border background="@value"><text>8</text></border></td>
                                <td><border background="@value"><text>Vintage Vinyl Records</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">45.00 $</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">5</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">225.00 $</text></border></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr>
                                <td><border background="@value"><text>9</text></border></td>
                                <td><border background="@value"><text>Retro Game Console</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">120.00 $</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">1</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">120.00 $</text></border></td>
                            </tr>
                            }
                            @alternate on value {
                            <tr>
                                <td><border background="@value"><text>10</text></border></td>
                                <td><border background="@value"><text>Handcrafted Chess Set</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">150.00 $</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">1</text></border></td>
                                <td><border background="@value"><text horizontalAlignment="right">150.00 $</text></border></td>
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
        // var       bitmaps = await generator.GenerateBitmapsAsync(xmlReader, CultureInfo.InvariantCulture, docOptions);
        // using var fstream = new FileStream("output.png", FileMode.Create);
        // {
        //     bitmaps.First().Encode(fstream, SKEncodedImageFormat.Png, 100);
        // }
        await generator.GeneratePdfAsync(pdfStream, xmlReader, CultureInfo.InvariantCulture, docOptions);
    }
}
