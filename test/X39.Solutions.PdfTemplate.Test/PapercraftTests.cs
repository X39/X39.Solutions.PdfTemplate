using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Controls.QrCode;
using X39.Solutions.Papercraft.Controls.ZXing;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;
using X39.Solutions.Papercraft.Rendering.SkiaSharp;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PapercraftTests
{
    private const string EscPosMediaType = "application/vnd.papercraft.escpos";

    [Fact]
    public void AddPapercraftRegistersDefaultRendererAndGenerator()
    {
        var services = new ServiceCollection();
        services.AddPapercraft();

        using var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetRequiredService<global::X39.Solutions.Papercraft.Papercraft>());
        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        var renderer = Assert.Single(generator.Backends);
        Assert.Equal("skiasharp", renderer.Capabilities.RendererId);
        Assert.IsType<SkiaSharpRenderBackend>(renderer);
        Assert.True(renderer.Capabilities.OutputKinds.HasFlag(RendererOutputKind.Pdf));
        Assert.Contains(
            renderer.Capabilities.MediaTypes,
            (q) => string.Equals(q, PapercraftMediaTypes.ApplicationPdf, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void AddPapercraftCoreDoesNotRegisterRendererRuntime()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore();

        using var serviceProvider = services.BuildServiceProvider();

        Assert.Empty(serviceProvider.GetServices<IPapercraftRenderBackend>());
        Assert.NotNull(serviceProvider.GetRequiredService<global::X39.Solutions.Papercraft.Papercraft>());
        Assert.Contains(services, (q) => q.ServiceType == typeof(PapercraftGenerator));
        Assert.Null(serviceProvider.GetService<Generator>());
        Assert.NotEmpty(serviceProvider.GetServices<ITransformer>());
    }

    [Fact]
    public async Task PapercraftSessionCanRenderLoweredXmlResultWithoutBackendsOrRegisteredControls()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore();

        await using var serviceProvider = services.BuildServiceProvider();
        var papercraft = serviceProvider.GetRequiredService<global::X39.Solutions.Papercraft.Papercraft>();
        await using var session = papercraft.CreateSession();
        session.TemplateData.SetVariable("Value", "kept");
        using var reader = CreateReader("""<notRegistered value="@Value">debug node</notRegistered>""");

        var result = await session.RenderAsync(
            reader,
            RenderTarget.LoweredXml,
            CultureInfo.InvariantCulture);

        Assert.Equal(RenderTarget.LoweredXml, result.Target);
        Assert.Equal(PapercraftMediaTypes.ApplicationPapercraftLoweredXml, result.MediaType);
        Assert.True(result.Length > 0);
        XNamespace ns = Constants.ControlsNamespace;
        var document = XDocument.Parse(result.ReadText());
        var lowered = document.Root?.Element(ns + "body")?.Element(ns + "notRegistered");
        Assert.NotNull(lowered);
        Assert.Equal("debug node", lowered!.Value);
        Assert.Equal("kept", (string?)lowered.Attribute("value"));
    }

    [Fact]
    public async Task PapercraftSessionExtensionsRenderPdfAndLoweredXmlHelpers()
    {
        var services = new ServiceCollection();
        services.AddPapercraft();

        await using var serviceProvider = services.BuildServiceProvider();
        var papercraft = serviceProvider.GetRequiredService<global::X39.Solutions.Papercraft.Papercraft>();
        await using var session = papercraft.CreateSession();
        session.TemplateData.SetVariable("Name", "helper diagnostics");

        await using var output = new MemoryStream();
        using (var reader = CreateReader("<text>helper pdf</text>"))
        {
            await session.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);
        }

        AssertPdfHeader(output);

        PapercraftRenderResult pdf;
        using (var reader = CreateReader("<text>helper pdf result</text>"))
        {
            pdf = await session.RenderPdfAsync(reader, CultureInfo.InvariantCulture);
        }

        Assert.Equal(RenderTarget.Pdf, pdf.Target);
        Assert.StartsWith("%PDF", Encoding.ASCII.GetString(pdf.ToArray(), 0, 4), StringComparison.Ordinal);

        using (var reader = CreateReader("<text>@Name</text>"))
        {
            var loweredXml = await session.ReadLoweredXmlAsync(reader, CultureInfo.InvariantCulture);
            XNamespace ns = Constants.ControlsNamespace;
            var document = XDocument.Parse(loweredXml);
            Assert.Equal("helper diagnostics", document.Root?.Element(ns + "body")?.Element(ns + "text")?.Value);
        }

        await using var loweredOutput = new MemoryStream();
        using (var reader = CreateReader("<text>@Name</text>"))
        {
            await session.GenerateLoweredXmlAsync(loweredOutput, reader, CultureInfo.InvariantCulture);
        }

        var loweredText = Encoding.UTF8.GetString(loweredOutput.ToArray());
        Assert.Contains("helper diagnostics", loweredText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PapercraftSessionsIsolateTemplateDataWhenRenderedConcurrently()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore();

        await using var serviceProvider = services.BuildServiceProvider();
        var papercraft = serviceProvider.GetRequiredService<global::X39.Solutions.Papercraft.Papercraft>();

        var values = await Task.WhenAll(
            RenderLoweredTextAsync(papercraft, "first"),
            RenderLoweredTextAsync(papercraft, "second"));

        Assert.Equal(new[] { "first", "second" }, values);

        static async Task<string> RenderLoweredTextAsync(global::X39.Solutions.Papercraft.Papercraft papercraft, string value)
        {
            await using var session = papercraft.CreateSession();
            session.TemplateData.SetVariable("Name", value);
            using var reader = CreateReader("<text>@Name</text>");

            var result = await session.RenderAsync(
                reader,
                RenderTarget.LoweredXml,
                CultureInfo.InvariantCulture);

            XNamespace ns = Constants.ControlsNamespace;
            var document = XDocument.Parse(result.ReadText());
            return document.Root?.Element(ns + "body")?.Element(ns + "text")?.Value
                   ?? throw new InvalidOperationException("Lowered XML result did not contain the expected text node.");
        }
    }

    [Fact]
    public void AddPapercraftSkiaSharpRendererRegistersRendererRuntimeAndCoreServices()
    {
        var services = new ServiceCollection();
        services.AddPapercraftSkiaSharpRenderer();

        using var serviceProvider = services.BuildServiceProvider();

        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        var renderer = Assert.Single(generator.Backends);
        Assert.IsType<SkiaSharpRenderBackend>(renderer);
        Assert.Equal("skiasharp", renderer.Capabilities.RendererId);
    }

    [Fact]
    public void PapercraftRegistrationsCanBeComposedWithoutDuplicatingDefaults()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore();
        services.AddPapercraftSkiaSharpRenderer();
        services.AddPapercraft();
        services.AddPdfTemplateService();

        Assert.Single(services, (q) => q.ServiceType == typeof(PapercraftGenerator));
        Assert.Single(
            services,
            (q) => q.ServiceType == typeof(IPapercraftRenderBackend)
                   && q.ImplementationType == typeof(SkiaSharpRenderBackend));
        Assert.Single(services, (q) => q.ServiceType == typeof(Generator));

        var controlTypes = services
            .Where((q) => q.ServiceType == typeof(ControlRegistration))
            .Select((q) => ((ControlRegistration)q.ImplementationInstance!).Type)
            .ToArray();
        Assert.Equal(controlTypes.Length, controlTypes.Distinct().Count());

        var transformerTypes = services
            .Where((q) => q.ServiceType == typeof(ITransformer))
            .Select((q) => q.ImplementationType)
            .ToArray();
        Assert.Equal(transformerTypes.Length, transformerTypes.Distinct().Count());

        using var serviceProvider = services.BuildServiceProvider();
        Assert.Single(serviceProvider.GetServices<IPapercraftRenderBackend>());
        Assert.NotNull(serviceProvider.GetRequiredService<PapercraftRenderer>());
        Assert.NotNull(serviceProvider.GetRequiredService<Generator>());
    }

    [Fact]
    public async Task AddPdfTemplateServiceKeepsLegacyGeneratorGeneratePdfAsync()
    {
        var services = new ServiceCollection();
        services.AddPdfTemplateService();

        await using var serviceProvider = services.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        await using var output = new MemoryStream();
        using var reader = CreateReader("<text>legacy compatibility</text>");

        await generator.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);

        AssertPdfHeader(output);
    }

    [Fact]
    public async Task AddPdfTemplateServiceRegistersPapercraftCompatibilityFacade()
    {
        var services = new ServiceCollection();
        services.AddPdfTemplateService();

        await using var serviceProvider = services.BuildServiceProvider();

        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        await using var output = new MemoryStream();
        using var reader = CreateReader("<text>compatibility</text>");

        await generator.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);

        AssertPdfHeader(output);
    }

    [Fact]
    public async Task AddPapercraftRegistersPapercraftGeneratorGeneratePdfAsync()
    {
        var services = new ServiceCollection();
        services.AddPapercraft();

        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        await using var output = new MemoryStream();
        using var reader = CreateReader("<text>Hello from the Papercraft facade!</text>");

        await generator.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);

        AssertPdfHeader(output);
    }

    [Fact]
    public async Task AddPapercraftWithOptionalBarcodeControlsRendersPdfAndRaster()
    {
        var services = new ServiceCollection();
        services.AddPapercraft()
                .AddQrCodeControls()
                .AddZxingBarcodeControls();

        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        await using var output = new MemoryStream();
        using var reader = CreateReader(
            """
            <qrCode value="https://example.test/qr" size="20mm" foreground="#000000" background="#FFFFFF" />
            <barcode format="Code128" value="ABC123" width="40mm" height="12mm" foreground="#000000" background="#FFFFFF" />
            <code128 value="XYZ789" width="40mm" height="12mm" foreground="#000000" background="#FFFFFF" />
            """);

        await generator.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);

        AssertPdfHeader(output);

        var pageStreams = new List<MemoryStream>();
        using var rasterReader = CreateReader(
            """
            <qrCode>https://example.test/content</qrCode>
            <barcode format="Code128">ABC123</barcode>
            <code128>XYZ789</code128>
            """);
        await generator.RenderRasterPagesAsync(
            rasterReader,
            new RasterPageRenderOutput(
                PapercraftMediaTypes.ImagePng,
                (_, _) =>
                {
                    var stream = new MemoryStream();
                    pageStreams.Add(stream);
                    return ValueTask.FromResult<Stream>(stream);
                },
                leaveStreamsOpen: true),
            CultureInfo.InvariantCulture);

        var png = Assert.Single(pageStreams).ToArray();
        Assert.True(
            png.Length >= 4
            && png[0] == 0x89
            && png[1] == 0x50
            && png[2] == 0x4E
            && png[3] == 0x47,
            "Barcode raster smoke test did not produce a PNG.");
    }

    [Fact]
    public async Task AddPapercraftSkiaSharpRendererSupportsGeneratePdfAsync()
    {
        var services = new ServiceCollection();
        services.AddPapercraftSkiaSharpRenderer();

        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        await using var output = new MemoryStream();
        using var reader = CreateReader("<text>Hello from the explicit SkiaSharp renderer!</text>");

        await generator.GeneratePdfAsync(output, reader, CultureInfo.InvariantCulture);

        AssertPdfHeader(output);
    }

    [Fact]
    public async Task PapercraftGeneratorRendersPdfThroughDefaultRenderer()
    {
        var services = new ServiceCollection();
        services.AddPapercraft();

        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        await using var output = new MemoryStream();
        using var reader = CreateReader("<text>Hello, Papercraft!</text>");

        await generator.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, output),
            CultureInfo.InvariantCulture);

        Assert.StartsWith("%PDF", Encoding.ASCII.GetString(output.ToArray(), 0, 4), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RenderAsyncWritesLoweredXmlWithoutSelectingBackend()
    {
        var backend = new RecordingRenderer("pdf", throwOnValidate: true);
        var generator = CreateRenderer(backend);
        generator.TemplateData.SetVariable("CustomerName", "Mira");
        generator.TemplateData.SetVariable("AccentColor", "#123456");
        generator.TemplateData.SetVariable("ShowNotice", true);
        await using var output = new MemoryStream();
        using var reader = CreateReader(
            """
            <body.style>
                <text foreground="@AccentColor"/>
            </body.style>
            <text>Hello @CustomerName</text>
            @if ShowNotice {
                <text>Shown</text>
            }
            """);

        await generator.RenderAsync(
            reader,
            new RenderOutput(RenderTarget.LoweredXml, output),
            CultureInfo.InvariantCulture);

        Assert.Equal(0, backend.ValidateCount);
        Assert.Equal(0, backend.RenderCount);

        var xml = Encoding.UTF8.GetString(output.ToArray());
        var document = XDocument.Parse(xml);
        XNamespace ns = Constants.ControlsNamespace;
        var body = document.Root?.Element(ns + "body");

        Assert.NotNull(body);
        Assert.Null(body!.Element(ns + "body.style"));
        var texts = body.Elements(ns + "text").ToArray();
        Assert.Collection(
            texts,
            (text) =>
            {
                Assert.Equal("Hello Mira", text.Value);
                Assert.Equal("#123456", (string?)text.Attribute("foreground"));
            },
            (text) =>
            {
                Assert.Equal("Shown", text.Value);
                Assert.Equal("#123456", (string?)text.Attribute("foreground"));
            });
    }

    [Fact]
    public async Task AddPapercraftCoreCanRenderLoweredXmlWithoutBackends()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore();

        await using var serviceProvider = services.BuildServiceProvider();
        var renderer = serviceProvider.GetRequiredService<PapercraftRenderer>();
        await using var output = new MemoryStream();
        using var reader = CreateReader("<text>core diagnostics</text>");

        await renderer.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPapercraftLoweredXml, output),
            CultureInfo.InvariantCulture);

        var document = XDocument.Parse(Encoding.UTF8.GetString(output.ToArray()));
        XNamespace ns = Constants.ControlsNamespace;
        Assert.Equal("core diagnostics", document.Root?.Element(ns + "body")?.Element(ns + "text")?.Value);
    }

    [Fact]
    public async Task LoweredXmlRenderDoesNotRequireRegisteredControls()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore();

        await using var serviceProvider = services.BuildServiceProvider();
        var renderer = serviceProvider.GetRequiredService<PapercraftRenderer>();
        renderer.TemplateData.SetVariable("Value", "kept");
        await using var output = new MemoryStream();
        using var reader = CreateReader("""<notRegistered value="@Value">debug node</notRegistered>""");

        await renderer.GenerateLoweredXmlAsync(output, reader, CultureInfo.InvariantCulture);

        var document = XDocument.Parse(Encoding.UTF8.GetString(output.ToArray()));
        XNamespace ns = Constants.ControlsNamespace;
        var lowered = document.Root?.Element(ns + "body")?.Element(ns + "notRegistered");
        Assert.NotNull(lowered);
        Assert.Equal("debug node", lowered!.Value);
        Assert.Equal("kept", (string?)lowered.Attribute("value"));
    }

    [Fact]
    public async Task ValidateAsyncSupportsLoweredXmlWithoutSelectingBackend()
    {
        var backend = new RecordingRenderer("pdf", throwOnValidate: true);
        var generator = CreateRenderer(backend);
        using var reader = CreateReader("<text>diagnostic</text>");

        var validation = await generator.ValidateAsync(
            reader,
            RenderTarget.LoweredXml,
            CultureInfo.InvariantCulture);

        Assert.Same(RenderValidationResult.Supported, validation);
        Assert.Equal(0, backend.ValidateCount);
        Assert.Equal(0, backend.RenderCount);
    }

    [Fact]
    public async Task GenerateLoweredXmlAsyncWritesLoweredXml()
    {
        var services = new ServiceCollection();
        services.AddPapercraft();

        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        generator.TemplateData.SetVariable("Name", "diagnostics");
        await using var output = new MemoryStream();
        using var reader = CreateReader("<text>@Name</text>");

        await generator.GenerateLoweredXmlAsync(output, reader, CultureInfo.InvariantCulture);

        var document = XDocument.Parse(Encoding.UTF8.GetString(output.ToArray()));
        XNamespace ns = Constants.ControlsNamespace;
        Assert.Equal("diagnostics", document.Root?.Element(ns + "body")?.Element(ns + "text")?.Value);
    }

    [Fact]
    public async Task PapercraftGeneratorReadsLoweredXmlTree()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore();

        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftGenerator>();
        generator.TemplateData.SetVariable("Name", "tree diagnostics");
        using var reader = CreateReader("<text>@Name</text>");

        var lowered = await generator.ReadLoweredXmlAsync(reader, CultureInfo.InvariantCulture);

        Assert.Equal("tree diagnostics", lowered["body", Constants.ControlsNamespace]?["text", Constants.ControlsNamespace]?.TextContent);
    }

    [Fact]
    public async Task CompatibilityGeneratorWritesLoweredXml()
    {
        var services = new ServiceCollection();
        services.AddPdfTemplateService();

        await using var serviceProvider = services.BuildServiceProvider();
        using var generator = serviceProvider.GetRequiredService<Generator>();
        generator.TemplateData.SetVariable("Name", "legacy diagnostics");
        await using var output = new MemoryStream();
        using var reader = CreateReader("<text>@Name</text>");

        await generator.GenerateLoweredXmlAsync(output, reader, CultureInfo.InvariantCulture);

        var document = XDocument.Parse(Encoding.UTF8.GetString(output.ToArray()));
        XNamespace ns = Constants.ControlsNamespace;
        Assert.Equal("legacy diagnostics", document.Root?.Element(ns + "body")?.Element(ns + "text")?.Value);
    }

    [Fact]
    public async Task GeneratedDocumentCannotRenderLoweredXml()
    {
        var generator = CreateRenderer(new RecordingRenderer("pdf"));
        var document = new PapercraftDocument(
            new[]
            {
                new PapercraftPage(0, 1, 1, new Size(1, 1), 96 / 25.4F, new DisplayList()),
            },
            CultureInfo.InvariantCulture,
            DocumentOptions.Default);
        await using var output = new MemoryStream();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await generator.RenderAsync(
                document,
                new RenderOutput(RenderTarget.LoweredXml, output)));

        Assert.Contains("template-based", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RenderAsyncSelectsRendererByRendererId()
    {
        var first = new RecordingRenderer("first", throwOnValidate: true);
        var selected = new RecordingRenderer("custom");
        var generator = CreateRenderer(
            new IPapercraftRenderBackend[]
        {
            first,
            selected,
        });
        using var reader = CreateReader("<spacer height=\"1px\" />");
        using var output = new MemoryStream();

        await generator.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, output),
            CultureInfo.InvariantCulture,
            new PapercraftRenderOptions { BackendId = "CUSTOM" });

        Assert.Equal(0, first.ValidateCount);
        Assert.Equal(0, first.RenderCount);
        Assert.Equal(1, selected.ValidateCount);
        Assert.Equal(1, selected.RenderCount);
        Assert.Equal(RenderTarget.Pdf, selected.LastTarget);
    }

    [Fact]
    public async Task RenderAsyncUsesSelectedRendererTextService()
    {
        var unusedTextService = new RecordingTextService("unused", throwOnUse: true);
        var selectedTextService = new RecordingTextService("selected");
        var first = new RecordingRenderer("first", textService: unusedTextService);
        var selected = new RecordingRenderer("custom", textService: selectedTextService);
        var generator = CreateRenderer(first, selected);
        using var reader = CreateReader("<text>Hello selected backend</text>");
        using var output = new MemoryStream();

        await generator.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, output),
            CultureInfo.InvariantCulture,
            new PapercraftRenderOptions { BackendId = "CUSTOM" });

        Assert.Equal(0, unusedTextService.MeasureCount);
        Assert.Equal(0, unusedTextService.DrawCount);
        Assert.True(selectedTextService.MeasureCount > 0);
        Assert.True(selectedTextService.DrawCount > 0);
        Assert.Equal(1, selected.RenderCount);
    }

    [Fact]
    public async Task LegacyRenderAsyncUsesSelectedRendererTextService()
    {
        var registeredTextService = new RecordingTextService("registered", throwOnUse: true);
        var skippedTextService = new RecordingTextService("skipped", throwOnUse: true);
        var selectedTextService = new RecordingTextService("selected");
        var skipped = new RecordingRenderer("skipped", textService: skippedTextService);
        var selected = new RecordingRenderer("selected", textService: selectedTextService);
        var services = new ServiceCollection();
        services.AddPapercraftCore();
        services.AddSingleton<ITextService>(registeredTextService);
        services.AddSingleton<IPapercraftRenderBackend>(skipped);
        services.AddSingleton<IPapercraftRenderBackend>(selected);
        await using var serviceProvider = services.BuildServiceProvider();
        var renderer = new PapercraftRenderer(
            serviceProvider.GetRequiredService<PapercraftGenerator>(),
            serviceProvider.GetServices<IPapercraftRenderBackend>());
        using var reader = CreateReader("<text>Hello selected legacy backend</text>");
        using var output = new MemoryStream();

        await renderer.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, output),
            CultureInfo.InvariantCulture,
            new PapercraftRenderOptions { BackendId = "selected" });

        Assert.Equal(0, registeredTextService.MeasureCount);
        Assert.Equal(0, registeredTextService.DrawCount);
        Assert.Equal(0, skippedTextService.MeasureCount);
        Assert.Equal(0, skippedTextService.DrawCount);
        Assert.True(selectedTextService.MeasureCount > 0);
        Assert.True(selectedTextService.DrawCount > 0);
        Assert.Equal(1, selected.RenderCount);
    }

    [Fact]
    public async Task RenderRasterPagesAsyncSelectsRasterRendererAndWritesPage()
    {
        var first = new RecordingRenderer("pdf", throwOnValidate: true);
        var selected = new RecordingRenderer(
            "raster",
            outputKinds: RendererOutputKind.RasterImage,
            mediaTypes: new[] { PapercraftMediaTypes.ImagePng });
        var generator = CreateRenderer(
            new IPapercraftRenderBackend[]
        {
            first,
            selected,
        });
        using var reader = CreateReader("<spacer height=\"1px\" />");
        var pages = new List<(RasterPageInfo Info, MemoryStream Stream)>();

        await generator.RenderRasterPagesAsync(
            reader,
            new RasterPageRenderOutput(
                PapercraftMediaTypes.ImagePng,
                (info, _) =>
                {
                    var stream = new MemoryStream();
                    pages.Add((info, stream));
                    return ValueTask.FromResult<Stream>(stream);
                },
                leaveStreamsOpen: true),
            CultureInfo.InvariantCulture);

        Assert.Equal(0, first.ValidateCount);
        Assert.Equal(0, first.RenderCount);
        Assert.Equal(0, first.RasterPageRenderCount);
        Assert.Equal(1, selected.ValidateCount);
        Assert.Equal(0, selected.RenderCount);
        Assert.Equal(1, selected.RasterPageRenderCount);
        Assert.Equal(RendererOutputKind.RasterImage, selected.LastTarget?.OutputKind);
        var page = Assert.Single(pages);
        Assert.Equal(0, page.Info.PageIndex);
        Assert.Equal(1, page.Info.PageNumber);
        Assert.Equal(PapercraftMediaTypes.ImagePng, page.Info.MediaType);
        Assert.Equal(1, page.Info.PixelWidth);
        Assert.Equal(1, page.Info.PixelHeight);
        Assert.Equal(new byte[] { 1 }, page.Stream.ToArray());
    }

    [Fact]
    public async Task RenderRasterPagesAsyncUsesTargetSelectedRendererTextService()
    {
        var pdfTextService = new RecordingTextService("pdf", throwOnUse: true);
        var rasterTextService = new RecordingTextService("raster");
        var first = new RecordingRenderer("pdf", textService: pdfTextService);
        var selected = new RecordingRenderer(
            "raster",
            outputKinds: RendererOutputKind.RasterImage,
            mediaTypes: new[] { PapercraftMediaTypes.ImagePng },
            textService: rasterTextService);
        var generator = CreateRenderer(first, selected);
        using var reader = CreateReader("<text>Hello raster backend</text>");
        var pages = new List<(RasterPageInfo Info, MemoryStream Stream)>();

        await generator.RenderRasterPagesAsync(
            reader,
            new RasterPageRenderOutput(
                PapercraftMediaTypes.ImagePng,
                (info, _) =>
                {
                    var stream = new MemoryStream();
                    pages.Add((info, stream));
                    return ValueTask.FromResult<Stream>(stream);
                },
                leaveStreamsOpen: true),
            CultureInfo.InvariantCulture);

        Assert.Equal(0, pdfTextService.MeasureCount);
        Assert.Equal(0, pdfTextService.DrawCount);
        Assert.True(rasterTextService.MeasureCount > 0);
        Assert.True(rasterTextService.DrawCount > 0);
        Assert.Equal(1, selected.RasterPageRenderCount);
        Assert.Single(pages);
    }

    [Fact]
    public async Task ValidateAsyncFailsWhenRendererIdIsMissing()
    {
        var available = new RecordingRenderer("available");
        var generator = CreateRenderer(available);
        using var reader = CreateReader("<spacer height=\"1px\" />");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await generator.ValidateAsync(
                reader,
                RenderTarget.Pdf,
                CultureInfo.InvariantCulture,
                new PapercraftRenderOptions { BackendId = "missing" }));

        Assert.Contains("missing", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, available.ValidateCount);
        Assert.Equal(0, available.RenderCount);
    }

    [Fact]
    public async Task RenderAsyncFailsBeforeRenderingWhenStrictValidationSeesDegradedDiagnostics()
    {
        var validation = new RenderValidationResult(
            new[]
            {
                new RenderDiagnostic(
                    "TEST-DEGRADED",
                    RendererSupportLevel.Degraded,
                    RendererFeatures.Color,
                    "Color output will be degraded.",
                    "The selected backend cannot preserve the requested colors."),
            });
        var renderer = new RecordingRenderer("degraded", validationResult: validation);
        var generator = CreateRenderer(renderer);
        using var reader = CreateReader("<spacer height=\"1px\" />");
        using var output = new MemoryStream();

        var exception = await Assert.ThrowsAsync<RenderValidationException>(
            async () => await generator.RenderAsync(
                reader,
                new RenderOutput(PapercraftMediaTypes.ApplicationPdf, output),
                CultureInfo.InvariantCulture,
                new PapercraftRenderOptions { TreatDegradedAsUnsupported = true }));

        Assert.Equal(validation.SupportLevel, exception.ValidationResult.SupportLevel);
        Assert.Equal(validation.Diagnostics, exception.ValidationResult.Diagnostics);
        Assert.Equal(1, renderer.ValidateCount);
        Assert.Equal(0, renderer.RenderCount);
    }

    [Fact]
    public async Task RenderAsyncFailsBeforeRenderingWhenSelectedRendererReportsUnsupportedTarget()
    {
        var renderer = new RecordingRenderer("pdf-only");
        var generator = CreateRenderer(renderer);
        using var reader = CreateReader("<spacer height=\"1px\" />");
        using var output = new MemoryStream();

        var exception = await Assert.ThrowsAsync<RenderValidationException>(
            async () => await generator.RenderAsync(
                reader,
                new RenderOutput(PapercraftMediaTypes.ImagePng, output),
                CultureInfo.InvariantCulture));

        Assert.Equal(RendererSupportLevel.Unsupported, exception.ValidationResult.SupportLevel);
        Assert.Contains(
            exception.ValidationResult.Diagnostics,
            (diagnostic) => diagnostic.Code == RenderDiagnosticCodes.UnsupportedOutputKind);
        Assert.Contains(
            exception.ValidationResult.Diagnostics,
            (diagnostic) => diagnostic.Code == RenderDiagnosticCodes.UnsupportedMediaType);
        Assert.Equal(1, renderer.ValidateCount);
        Assert.Equal(0, renderer.RenderCount);
        Assert.Equal(RendererOutputKind.RasterImage, renderer.LastTarget?.OutputKind);
    }

    [Fact]
    public async Task RenderRasterPagesAsyncFailsBeforeOpeningPageStreamWhenSelectedRendererReportsUnsupportedTarget()
    {
        var renderer = new RecordingRenderer("pdf-only");
        var generator = CreateRenderer(renderer);
        using var reader = CreateReader("<spacer height=\"1px\" />");
        var callbackCount = 0;

        var exception = await Assert.ThrowsAsync<RenderValidationException>(
            async () => await generator.RenderRasterPagesAsync(
                reader,
                new RasterPageRenderOutput(
                    PapercraftMediaTypes.ImagePng,
                    (_, _) =>
                    {
                        callbackCount++;
                        return ValueTask.FromResult<Stream>(new MemoryStream());
                    }),
                CultureInfo.InvariantCulture));

        Assert.Equal(RendererSupportLevel.Unsupported, exception.ValidationResult.SupportLevel);
        Assert.Contains(
            exception.ValidationResult.Diagnostics,
            (diagnostic) => diagnostic.Code == RenderDiagnosticCodes.UnsupportedOutputKind);
        Assert.Contains(
            exception.ValidationResult.Diagnostics,
            (diagnostic) => diagnostic.Code == RenderDiagnosticCodes.UnsupportedMediaType);
        Assert.Equal(1, renderer.ValidateCount);
        Assert.Equal(0, renderer.RasterPageRenderCount);
        Assert.Equal(0, callbackCount);
    }

    [Fact]
    public async Task RenderRasterPagesAsyncFailsBeforeOpeningPageStreamWhenStrictValidationSeesDegradedDiagnostics()
    {
        var validation = new RenderValidationResult(
            new[]
            {
                new RenderDiagnostic(
                    "TEST-DEGRADED",
                    RendererSupportLevel.Degraded,
                    RendererFeatures.Color,
                    "Color output will be degraded.",
                    "The selected backend cannot preserve the requested colors."),
            });
        var renderer = new RecordingRenderer(
            "degraded-raster",
            validationResult: validation,
            outputKinds: RendererOutputKind.RasterImage,
            mediaTypes: new[] { PapercraftMediaTypes.ImagePng });
        var generator = CreateRenderer(renderer);
        using var reader = CreateReader("<spacer height=\"1px\" />");
        var callbackCount = 0;

        var exception = await Assert.ThrowsAsync<RenderValidationException>(
            async () => await generator.RenderRasterPagesAsync(
                reader,
                new RasterPageRenderOutput(
                    PapercraftMediaTypes.ImagePng,
                    (_, _) =>
                    {
                        callbackCount++;
                        return ValueTask.FromResult<Stream>(new MemoryStream());
                    }),
                CultureInfo.InvariantCulture,
                new PapercraftRenderOptions { TreatDegradedAsUnsupported = true }));

        Assert.Equal(validation.SupportLevel, exception.ValidationResult.SupportLevel);
        Assert.Equal(validation.Diagnostics, exception.ValidationResult.Diagnostics);
        Assert.Equal(1, renderer.ValidateCount);
        Assert.Equal(0, renderer.RasterPageRenderCount);
        Assert.Equal(0, callbackCount);
    }

    [Fact]
    public void RenderValidationResultAggregatesSupportedDiagnostics()
    {
        var validation = RenderValidationResult.Supported;

        Assert.Equal(RendererSupportLevel.Supported, validation.SupportLevel);
        Assert.True(validation.IsSupported);
        Assert.False(validation.HasDegradedDiagnostics);
        Assert.False(validation.HasUnsupportedDiagnostics);
    }

    [Fact]
    public void RenderDiagnosticCodesAreStable()
    {
        Assert.Equal("PAPERCRAFT001", RenderDiagnosticCodes.UnsupportedOutputKind);
        Assert.Equal("PAPERCRAFT002", RenderDiagnosticCodes.UnsupportedMediaType);
        Assert.Equal("PAPERCRAFT003", RenderDiagnosticCodes.UnsupportedFeature);
        Assert.Equal("PAPERCRAFT004", RenderDiagnosticCodes.DegradedFeature);
        Assert.Equal("PAPERCRAFT005", RenderDiagnosticCodes.MissingFontSubstitution);
        Assert.Equal("PAPERCRAFT006", RenderDiagnosticCodes.FontFaceSubstitution);
    }

    [Fact]
    public async Task ValidateAsyncReportsUnsupportedTemplateFeatureFromCapabilities()
    {
        var renderer = new FeatureLimitedRenderer();
        var generator = CreateRenderer(renderer);
        using var reader = CreateReader("<image source=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII=\" />");

        var validation = await generator.ValidateAsync(
            reader,
            RenderTarget.Pdf,
            CultureInfo.InvariantCulture);

        Assert.Equal(RendererSupportLevel.Unsupported, validation.SupportLevel);
        var diagnostic = Assert.Single(validation.Diagnostics, (q) => q.Code == RenderDiagnosticCodes.UnsupportedFeature);
        Assert.Equal(RendererFeatures.Images, diagnostic.Feature);
        Assert.Null(diagnostic.Location);
        Assert.Equal(1, renderer.ValidateCount);
    }

    [Fact]
    public void RenderValidationResultAggregatesDegradedDiagnostics()
    {
        var validation = new RenderValidationResult(
            new[]
            {
                new RenderDiagnostic(
                    "TEST001",
                    RendererSupportLevel.Supported,
                    RendererFeatures.PdfOutput,
                    "PDF output is supported."),
                new RenderDiagnostic(
                    "TEST002",
                    RendererSupportLevel.Degraded,
                    RendererFeatures.Color,
                    "Color output will be approximated."),
            });

        Assert.Equal(RendererSupportLevel.Degraded, validation.SupportLevel);
        Assert.True(validation.IsSupported);
        Assert.True(validation.HasDegradedDiagnostics);
        Assert.False(validation.HasUnsupportedDiagnostics);
        validation.ThrowIfUnsupportedOrStrictDegraded(false);
        Assert.Throws<RenderValidationException>(() => validation.ThrowIfUnsupportedOrStrictDegraded(true));
    }

    [Fact]
    public void RenderValidationResultAggregatesUnsupportedDiagnostics()
    {
        var validation = new RenderValidationResult(
            new[]
            {
                new RenderDiagnostic(
                    "TEST001",
                    RendererSupportLevel.Degraded,
                    RendererFeatures.Color,
                    "Color output will be approximated."),
                new RenderDiagnostic(
                    "TEST002",
                    RendererSupportLevel.Unsupported,
                    RendererFeatures.Images,
                    "Images cannot be rendered."),
            });

        Assert.Equal(RendererSupportLevel.Unsupported, validation.SupportLevel);
        Assert.False(validation.IsSupported);
        Assert.True(validation.HasDegradedDiagnostics);
        Assert.True(validation.HasUnsupportedDiagnostics);
        Assert.Throws<RenderValidationException>(() => validation.ThrowIfUnsupported());
    }

    [Fact]
    public async Task ValidateAsyncReportsUnsupportedOutputBeforeRendering()
    {
        var services = new ServiceCollection();
        services.AddPapercraft();

        await using var serviceProvider = services.BuildServiceProvider();
        var generator = serviceProvider.GetRequiredService<PapercraftRenderer>();
        using var reader = CreateReader("<text>Hello, printer.</text>");

        var validation = await generator.ValidateAsync(
            reader,
            new RenderTarget(EscPosMediaType, RendererOutputKind.PrinterCommands),
            CultureInfo.InvariantCulture);

        Assert.Equal(RendererSupportLevel.Unsupported, validation.SupportLevel);
        Assert.Equal(2, validation.Diagnostics.Count);
        var outputKindDiagnostic = Assert.Single(
            validation.Diagnostics,
            (q) => q.Code == RenderDiagnosticCodes.UnsupportedOutputKind);
        Assert.Equal(RendererSupportLevel.Unsupported, outputKindDiagnostic.Level);
        Assert.Equal(RendererFeatures.PrinterCommandOutput, outputKindDiagnostic.Feature);
        Assert.Contains("PrinterCommands", outputKindDiagnostic.Message, StringComparison.Ordinal);
        Assert.Contains("Pdf", outputKindDiagnostic.BackendLimitation, StringComparison.Ordinal);

        var mediaTypeDiagnostic = Assert.Single(
            validation.Diagnostics,
            (q) => q.Code == RenderDiagnosticCodes.UnsupportedMediaType);
        Assert.Equal(RendererSupportLevel.Unsupported, mediaTypeDiagnostic.Level);
        Assert.Equal(RendererFeatures.PrinterCommandOutput, mediaTypeDiagnostic.Feature);
        Assert.Contains(EscPosMediaType, mediaTypeDiagnostic.Message, StringComparison.Ordinal);
        Assert.Contains(PapercraftMediaTypes.ApplicationPdf, mediaTypeDiagnostic.BackendLimitation, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidateAsyncReturnsConstrainedPrinterDiagnosticsFromSelectedRenderer()
    {
        var printerRenderer = new ConstrainedPrinterRenderer();
        var generator = CreateRenderer(
            new IPapercraftRenderBackend[]
        {
            new ThrowingPdfOnlyRenderer(),
            printerRenderer,
        });
        using var reader = CreateReader("<spacer height=\"1px\" />");

        var validation = await generator.ValidateAsync(
            reader,
            new RenderTarget(EscPosMediaType, RendererOutputKind.PrinterCommands),
            CultureInfo.InvariantCulture);

        Assert.True(printerRenderer.WasValidated);
        Assert.Equal(RendererSupportLevel.Unsupported, validation.SupportLevel);
        Assert.Collection(
            validation.Diagnostics,
            (diagnostic) =>
            {
                Assert.Equal("TEST-PRINTER-COLOR", diagnostic.Code);
                Assert.Equal(RendererSupportLevel.Degraded, diagnostic.Level);
                Assert.Equal(RendererFeatures.Color, diagnostic.Feature);
                Assert.Contains("monochrome", diagnostic.BackendLimitation, StringComparison.OrdinalIgnoreCase);
            },
            (diagnostic) =>
            {
                Assert.Equal("TEST-PRINTER-IMAGE", diagnostic.Code);
                Assert.Equal(RendererSupportLevel.Unsupported, diagnostic.Level);
                Assert.Equal(RendererFeatures.Images, diagnostic.Feature);
                Assert.Contains("image", diagnostic.Message, StringComparison.OrdinalIgnoreCase);
            });
    }

    private static XmlReader CreateReader(string body)
    {
        var xml = $$"""
<?xml version="1.0" encoding="utf-8"?>
<template xmlns="{{Constants.ControlsNamespace}}">
    <body>
        {{body}}
    </body>
</template>
""";
        return XmlReader.Create(new MemoryStream(Encoding.UTF8.GetBytes(xml)));
    }

    private static void AssertPdfHeader(MemoryStream output)
        => Assert.StartsWith("%PDF", Encoding.ASCII.GetString(output.ToArray(), 0, 4), StringComparison.Ordinal);

    private static PapercraftRenderer CreateRenderer(params IPapercraftRenderBackend[] backends)
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore();
        foreach (var backend in backends)
        {
            services.AddSingleton(backend);
        }

        return services.BuildServiceProvider().GetRequiredService<PapercraftRenderer>();
    }

    private sealed class ThrowingPdfOnlyRenderer : IPapercraftRenderBackend
    {
        public RendererCapabilities Capabilities { get; } = new(
            "pdf-only",
            "PDF only",
            RendererOutputKind.Pdf,
            new[] { PapercraftMediaTypes.ApplicationPdf });

        public ITextService TextService { get; } = new RecordingTextService("pdf-only");

        public ValueTask<RenderValidationResult> ValidateAsync(
            PapercraftDocument document,
            RenderTarget target,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("The PDF-only renderer should not validate printer targets.");

        public ValueTask RenderAsync(
            PapercraftDocument document,
            RenderOutput output,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask RenderRasterPagesAsync(
            PapercraftDocument document,
            RasterPageRenderOutput output,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class RecordingRenderer : IPapercraftRenderBackend
    {
        private readonly RenderValidationResult? _validationResult;
        private readonly bool _throwOnValidate;

        public RecordingRenderer(
            string rendererId,
            RenderValidationResult? validationResult = null,
            bool throwOnValidate = false,
            RendererOutputKind outputKinds = RendererOutputKind.Pdf,
            IEnumerable<string>? mediaTypes = null,
            ITextService? textService = null)
        {
            Capabilities = new RendererCapabilities(
                rendererId,
                rendererId,
                outputKinds,
                mediaTypes ?? new[] { PapercraftMediaTypes.ApplicationPdf });
            _validationResult = validationResult;
            _throwOnValidate = throwOnValidate;
            TextService = textService ?? new RecordingTextService(rendererId);
        }

        public RendererCapabilities Capabilities { get; }

        public ITextService TextService { get; }

        public int ValidateCount { get; private set; }

        public int RenderCount { get; private set; }

        public int RasterPageRenderCount { get; private set; }

        public RenderTarget? LastTarget { get; private set; }

        public ValueTask<RenderValidationResult> ValidateAsync(
            PapercraftDocument document,
            RenderTarget target,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(target);
            cancellationToken.ThrowIfCancellationRequested();
            ValidateCount++;
            LastTarget = target;
            if (_throwOnValidate)
                throw new InvalidOperationException($"Renderer '{Capabilities.RendererId}' should not be selected.");
            return ValueTask.FromResult(_validationResult ?? Capabilities.ValidateTarget(target));
        }

        public ValueTask RenderAsync(
            PapercraftDocument document,
            RenderOutput output,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(output);
            cancellationToken.ThrowIfCancellationRequested();
            RenderCount++;
            return ValueTask.CompletedTask;
        }

        public async ValueTask RenderRasterPagesAsync(
            PapercraftDocument document,
            RasterPageRenderOutput output,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(output);
            cancellationToken.ThrowIfCancellationRequested();
            RasterPageRenderCount++;
            var pageStream = await output.OpenPageStreamAsync(
                    new RasterPageInfo(0, 1, output.MediaType, 1, 1, 96 / 25.4f),
                    cancellationToken)
                .ConfigureAwait(false)
                             ?? throw new InvalidOperationException("The test raster callback must return a stream.");
            try
            {
                await pageStream.WriteAsync(new byte[] { 1 }, cancellationToken)
                    .ConfigureAwait(false);
                await pageStream.FlushAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                if (!output.LeaveStreamsOpen)
                    await pageStream.DisposeAsync()
                        .ConfigureAwait(false);
            }
        }
    }

    private sealed class ConstrainedPrinterRenderer : IPapercraftRenderBackend
    {
        private readonly RenderDiagnostic[] _diagnostics =
        {
            new(
                "TEST-PRINTER-COLOR",
                RendererSupportLevel.Degraded,
                RendererFeatures.Color,
                "Color output will be converted to monochrome printer commands.",
                "The constrained printer supports monochrome output only."),
            new(
                "TEST-PRINTER-IMAGE",
                RendererSupportLevel.Unsupported,
                RendererFeatures.Images,
                "Image drawing is not available for the constrained printer.",
                "The constrained printer backend has image commands disabled."),
        };

        public RendererCapabilities Capabilities { get; } = new(
            "constrained-printer",
            "Constrained printer",
            RendererOutputKind.PrinterCommands,
            new[] { EscPosMediaType },
            new Dictionary<string, RendererSupportLevel>(StringComparer.OrdinalIgnoreCase)
            {
                [RendererFeatures.Color] = RendererSupportLevel.Degraded,
                [RendererFeatures.Images] = RendererSupportLevel.Unsupported,
            });

        public ITextService TextService { get; } = new RecordingTextService("constrained-printer");

        public bool WasValidated { get; private set; }

        public ValueTask<RenderValidationResult> ValidateAsync(
            PapercraftDocument document,
            RenderTarget target,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(document);
            cancellationToken.ThrowIfCancellationRequested();
            WasValidated = true;
            var targetValidation = Capabilities.ValidateTarget(target);
            return ValueTask.FromResult(
                targetValidation.IsSupported
                    ? new RenderValidationResult(_diagnostics)
                    : targetValidation);
        }

        public ValueTask RenderAsync(
            PapercraftDocument document,
            RenderOutput output,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask RenderRasterPagesAsync(
            PapercraftDocument document,
            RasterPageRenderOutput output,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FeatureLimitedRenderer : IPapercraftRenderBackend
    {
        public RendererCapabilities Capabilities { get; } = new(
            "feature-limited",
            "Feature limited",
            RendererOutputKind.Pdf,
            new[] { PapercraftMediaTypes.ApplicationPdf },
            new Dictionary<string, RendererSupportLevel>
            {
                [RendererFeatures.Images] = RendererSupportLevel.Unsupported,
            });

        public ITextService TextService { get; } = new RecordingTextService("feature-limited");

        public int ValidateCount { get; private set; }

        public ValueTask<RenderValidationResult> ValidateAsync(
            PapercraftDocument document,
            RenderTarget target,
            CancellationToken cancellationToken = default)
        {
            ValidateCount++;
            return ValueTask.FromResult(Capabilities.ValidateTarget(target));
        }

        public ValueTask RenderAsync(
            PapercraftDocument document,
            RenderOutput output,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public ValueTask RenderRasterPagesAsync(
            PapercraftDocument document,
            RasterPageRenderOutput output,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class RecordingTextService : ITextService
    {
        private readonly string _name;
        private readonly bool _throwOnUse;

        public RecordingTextService(string name, bool throwOnUse = false)
        {
            _name = name;
            _throwOnUse = throwOnUse;
        }

        public int MeasureCount { get; private set; }

        public int DrawCount { get; private set; }

        public Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
        {
            if (_throwOnUse)
                throw new InvalidOperationException($"Text service '{_name}' should not be used.");

            MeasureCount++;
            var fontSize = Math.Max(1F, textStyle.FontSize * dpi / 72.272F * Math.Max(0.01F, textStyle.Scale));
            var width = text.Length * fontSize * 0.5F;
            if (float.IsFinite(maxWidth) && maxWidth > 0F)
                width = Math.Min(width, maxWidth);

            return new Size(Math.Max(1F, width), fontSize);
        }

        public void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
        {
            ArgumentNullException.ThrowIfNull(canvas);
            if (_throwOnUse)
                throw new InvalidOperationException($"Text service '{_name}' should not be used.");

            DrawCount++;
            canvas.DrawText(textStyle, dpi, text.ToString(), 0F, textStyle.FontSize);
        }
    }
}
