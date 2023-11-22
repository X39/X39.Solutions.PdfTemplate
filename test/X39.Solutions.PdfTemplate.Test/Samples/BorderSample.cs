using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;

namespace X39.Solutions.PdfTemplate.Test.Samples;

[Collection("Samples")]
public class BorderSample : SampleBase
{

    [Fact]
    public async Task SomeSample()
    {
        using var generator = CreateGenerator();
        using var xmlStream = new MemoryStream(
            Encoding.UTF8.GetBytes(
                $$"""
                  <?xml version="1.0" encoding="utf-8"?>
                  <template xmlns="{{Constants.ControlsNamespace}}">
                      <body>
                          <border clip="true" thickness="1px" color="red" background="blue" padding="1px" margin="1px">
                            <border clip="true" thickness="1px" color="green" background="yellow" padding="1px" margin="1px">
                              <border clip="true" thickness="1px" color="red" background="blue" padding="1px" margin="1px">
                                <border clip="true" thickness="1px" color="green" background="yellow" padding="1px" margin="1px">
                                    <text foreground="white">Single line of text</text>
                                 </border>
                               </border>
                             </border>
                           </border>
                          <border clip="true" thickness="5px" color="red" background="blue" padding="5px" margin="5px">
                            <border clip="true" thickness="5px" color="green" background="yellow" padding="5px" margin="5px">
                              <border clip="true" thickness="5px" color="red" background="blue" padding="5px" margin="5px">
                                <border clip="true" thickness="5px" color="green" background="yellow" padding="5px" margin="5px">
                                    <text foreground="white">Single line of text</text>
                                 </border>
                               </border>
                             </border>
                           </border>
                          <border clip="false" thickness="1px" color="red" background="blue" padding="1px" margin="1px">
                            <border clip="false" thickness="1px" color="green" background="yellow" padding="1px" margin="1px">
                                <border clip="false" thickness="1px" color="red" background="blue" padding="1px" margin="1px">
                                  <border clip="false" thickness="1px" color="green" background="yellow" padding="1px" margin="1px">
                                      <text foreground="white">Single line of text</text>
                                   </border>
                                 </border>
                             </border>
                           </border>
                          <border clip="false" thickness="5px" color="red" background="blue" padding="5px" margin="5px">
                            <border clip="false" thickness="5px" color="green" background="yellow" padding="5px" margin="5px">
                                <border clip="false" thickness="5px" color="red" background="blue" padding="5px" margin="5px">
                                  <border clip="false" thickness="5px" color="green" background="yellow" padding="5px" margin="5px">
                                      <text foreground="white">Single line of text</text>
                                   </border>
                                 </border>
                             </border>
                           </border>
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