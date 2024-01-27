using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate.Services;
using X39.Solutions.PdfTemplate.Test.Mock;
using X39.Util;

namespace X39.Solutions.PdfTemplate.Test.Samples;

public abstract class SampleBase : IAsyncDisposable
{
    private readonly ServiceProvider _serviceProvider;

    public SampleBase()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddPdfTemplateServices();
        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public Generator CreateGenerator(params IFunction[] functions)
    {
        var generator = new Generator(
            _serviceProvider.GetRequiredService<SkPaintCache>(),
            _serviceProvider.GetRequiredService<ControlExpressionCache>(),
            functions);
        generator.AddDefaults();
        generator.AddControl<MockControl>();
        return generator;
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
    }

    public static IDisposable CreateStream(out Stream stream)
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
}