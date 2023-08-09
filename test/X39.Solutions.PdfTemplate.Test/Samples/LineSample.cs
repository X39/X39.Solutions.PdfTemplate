using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate.Services;
using X39.Util;

namespace X39.Solutions.PdfTemplate.Test.Samples;

[Collection("Samples")]
public class LineSample : IAsyncDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public LineSample()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ControlExpressionCache>();
        serviceCollection.AddSingleton<SkPaintCache>();
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
    }

    private static IDisposable CreateStream(out Stream stream)
    {
        if (!Debugger.IsAttached)
            return stream = new VoidStream();
        var tmpPath = Path.GetTempPath();
        while (true)
        {
            try
            {
                var tmpFile = Path.Combine(tmpPath, Path.GetRandomFileName() + ".pdf");
                var tmp = stream = new FileStream(tmpFile, FileMode.Create, FileAccess.Write, FileShare.Read);
                return new Disposable(
                    () =>
                    {
                        tmp.Dispose();
                        var process = Process.Start(
                            new ProcessStartInfo
                            {
                                FileName        = tmpFile,
                                UseShellExecute = true,
                            });
                        if (process is null)
                            throw new InvalidOperationException("Could not start process.");
                        process.WaitForExit();
                        File.Delete(tmpFile);
                    });
            }
            catch (IOException)
            {
                /* empty */
            }
        }
    }

    [Fact, Conditional("DEBUG")]
    public void SimpleLineSample()
    {
        using var generator = new Generator(
            _serviceProvider.GetRequiredService<SkPaintCache>(),
            _serviceProvider.GetRequiredService<ControlExpressionCache>());
        generator.AddDefaultControls();
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
        generator.Generate(
            pdfStream,
            xmlReader,
            CultureInfo.InvariantCulture);
    }
}