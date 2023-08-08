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
                        var process = Process.Start(new ProcessStartInfo
                        {
                            FileName = tmpFile,
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
        using var generator = new Generator(_serviceProvider.GetRequiredService<SkPaintCache>(), _serviceProvider.GetRequiredService<ControlExpressionCache>());
        generator.AddDefaultControls();
        using var xmlStream = new MemoryStream(
            Encoding.UTF8.GetBytes(
                $"""
                 <?xml version="1.0" encoding="utf-8"?>
                 <template xmlns="{Constants.ControlsNamespace}">
                     <body>
                         <line thickness="1px" length="10%" color="red" padding="0"/>
                         <line thickness="2px" length="20%" color="green" margin="0" padding="0"/>
                         <line thickness="3px" length="30%" color="blue" margin="0"/>
                         <line thickness="3px" length="40%" color="red" padding="0"/>
                         <line thickness="2px" length="50%" color="green" margin="0" padding="0"/>
                         <line thickness="1px" length="60%" color="blue" margin="0"/>
                         <line thickness="1px" length="70%" color="red" padding="0"/>
                         <line thickness="2px" length="80%" color="green" margin="0" padding="0"/>
                         <line thickness="3px" length="90%" color="blue" margin="0"/>
                         <line thickness="10px" length="100%" color="black" margin="0"/>
                     </body>
                     <template.style>
                         <line margin="4px" padding="4px"/>
                     </template.style>
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