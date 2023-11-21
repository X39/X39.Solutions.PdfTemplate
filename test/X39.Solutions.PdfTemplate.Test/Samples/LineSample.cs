using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;

namespace X39.Solutions.PdfTemplate.Test.Samples;

[Collection("Samples")]
public class LineSample : SampleBase
{

    [Fact]
    public async Task SimpleLineSample()
    {
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(
            Encoding.UTF8.GetBytes(
                $"""
                 <?xml version="1.0" encoding="utf-8"?>
                 <template xmlns="{Constants.ControlsNamespace}">
                     <template.style>
                        <!-- Important: Style is applied only to elements after the style element. -->
                        <!-- You can use multiple style elements in a single template element. -->
                        <line margin="2px" padding="2px"/>
                     </template.style>
                     <body>
                         <line thickness="1px" length="10%" color="red" padding="0"/>
                         <line thickness="2px" length="20%" color="green" margin="0" padding="0"/>
                         <line thickness="3px" length="30%" color="blue" margin="0"/>
                         <line thickness="4px" length="40%" color="red" padding="0"/>
                         <line thickness="5px" length="50%" color="green" margin="0" padding="0"/>
                         <line thickness="6px" length="60%" color="blue" margin="0"/>
                         <line thickness="7px" length="70%" color="red" padding="0"/>
                         <line thickness="8px" length="80%" color="green" margin="0" padding="0"/>
                         <line thickness="9px" length="90%" color="blue" margin="0"/>
                         <line thickness="10px" length="100%" color="black" margin="0"/>
                         
                         <line thickness="1px" length="50px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="51px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="52px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="53px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="54px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="55px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="56px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="57px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="58px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="59px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="60px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="61px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="62px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="63px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="64px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="65px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="66px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="67px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="68px" color="purple" padding="0" margin="0"/>
                         <line thickness="1px" length="69px" color="purple" padding="0" margin="0"/>
                         
                         <line thickness="1px" length="50px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="51px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="52px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="53px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="54px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="55px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="56px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="57px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="58px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="59px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="60px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="61px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="62px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="63px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="64px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="65px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="66px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="67px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="68px" color="orange" padding="1px" margin="0"/>
                         <line thickness="1px" length="69px" color="orange" padding="1px" margin="0"/>
                         
                         <line thickness="1px" length="50px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="51px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="52px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="53px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="54px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="55px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="56px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="57px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="58px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="59px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="60px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="61px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="62px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="63px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="64px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="65px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="66px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="67px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="68px" color="purple" padding="0" margin="1px"/>
                         <line thickness="1px" length="69px" color="purple" padding="0" margin="1px"/>
                     </body>
                 </template>
                 """));
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(
            pdfStream,
            xmlReader,
            CultureInfo.InvariantCulture);
    }

    [Fact]
    public async Task PageBreak()
    {
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(
            Encoding.UTF8.GetBytes(
                $"""
                 <?xml version="1.0" encoding="utf-8"?>
                 <template xmlns="{Constants.ControlsNamespace}">
                     <body>
                         <!-- Will create 2 pages with a vertical line on each page -->
                         <line orientation="vertical" thickness="10%" length="200%" color="red" padding="0"/>
                     </body>
                 </template>
                 """));
        using var disposable = CreateStream(out var pdfStream);
        using var xmlReader = XmlReader.Create(xmlStream);
        await generator.GeneratePdfAsync(
            pdfStream,
            xmlReader,
            CultureInfo.InvariantCulture);
    }
}