using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;
using X39.Solutions.Papercraft.Rendering.PdfSharp;
using X39.Solutions.Papercraft.Rendering.PdfSharp.Services;
using X39.Solutions.Papercraft.Rendering.Svg;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PapercraftAdditionalBackendTests
{
    [Fact]
    public void SvgRendererRegistrationAddsSvgBackend()
    {
        var services = new ServiceCollection();

        services.AddPapercraftSvgRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();

        Assert.Contains(renderer.Backends, (q) => q.Capabilities.RendererId == "svg");
        Assert.NotNull(provider.GetService<ITextService>());
    }

    [Fact]
    public async Task SvgBackendRendersDisplayListAsSvg()
    {
        var backend = new SvgRenderBackend();
        var document = CreateSimpleDocument(includeText: true);
        await using var stream = new MemoryStream();

        await backend.RenderAsync(
            document,
            new RenderOutput(RenderTarget.FromMediaType(SvgRenderBackend.MediaType), stream),
            CancellationToken.None);

        stream.Position = 0;
        var svgDocument = XDocument.Load(stream);
        XNamespace svg = "http://www.w3.org/2000/svg";

        Assert.Equal(svg + "svg", svgDocument.Root?.Name);
        Assert.Equal("Hello Papercraft", svgDocument.Descendants(svg + "text").Single().Value);
        Assert.Contains(svgDocument.Descendants(svg + "rect"), (q) => q.Attribute("fill")?.Value == "#336699");
        Assert.Contains(svgDocument.Descendants(svg + "a"), (q) => q.Attribute("href")?.Value == "https://example.com");
    }

    [Fact]
    public async Task SvgRendererGeneratesXmlTemplateWithText()
    {
        var services = new ServiceCollection();
        services.AddPapercraftSvgRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();
        await using var stream = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader(CreateTextTemplate("Hello SVG")));

        await renderer.RenderAsync(
            reader,
            new RenderOutput(RenderTarget.FromMediaType(SvgRenderBackend.MediaType), stream),
            CultureInfo.InvariantCulture);

        stream.Position = 0;
        var svgDocument = XDocument.Load(stream);
        XNamespace svg = "http://www.w3.org/2000/svg";

        Assert.Contains(svgDocument.Descendants(svg + "text"), (q) => q.Value == "Hello SVG");
    }

    [Fact]
    public async Task SvgBackendFreezesClipBeforeLaterTranslation()
    {
        var backend = new SvgRenderBackend();
        var document = CreateTranslatedClipDocument();
        await using var stream = new MemoryStream();

        await backend.RenderAsync(
            document,
            new RenderOutput(RenderTarget.FromMediaType(SvgRenderBackend.MediaType), stream),
            CancellationToken.None);

        stream.Position = 0;
        var svgDocument = XDocument.Load(stream);
        XNamespace svg = "http://www.w3.org/2000/svg";

        Assert.Contains(
            svgDocument.Descendants(svg + "clipPath").SelectMany((q) => q.Elements(svg + "rect")),
            (q) => HasRectangle(q, 0, 20, 100, 10));

        var drawnRectangle = Assert.Single(
            svgDocument.Descendants(svg + "rect"),
            (q) => q.Attribute("fill")?.Value == "#112233");
        Assert.True(HasRectangle(drawnRectangle, 0, 25, 100, 10));
    }

    [Fact]
    public void PdfSharpRendererRegistrationAddsPdfSharpBackend()
    {
        var services = new ServiceCollection();

        services.AddPapercraftPdfSharpRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();

        Assert.Contains(renderer.Backends, (q) => q.Capabilities.RendererId == "pdfsharp");
        Assert.NotNull(provider.GetService<ITextService>());
    }

    [Fact]
    public async Task PdfSharpBackendRendersDisplayListAsPdf()
    {
        var backend = new PdfSharpRenderBackend();
        var document = CreateSimpleDocument(includeText: false);
        await using var stream = new MemoryStream();

        await backend.RenderAsync(
            document,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream),
            CancellationToken.None);

        var bytes = stream.ToArray();

        Assert.True(bytes.Length > 100);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public void PdfSharpDisplayListRendererFreezesClipBeforeLaterTranslation()
    {
        using var pdfDocument = new PdfDocument();
        pdfDocument.Options.NoCompression = true;
        var page = pdfDocument.AddPage();
        page.Width = XUnit.FromPoint(100);
        page.Height = XUnit.FromPoint(100);
        var displayList = CreateTranslatedClipDisplayList();

        using (var graphics = XGraphics.FromPdfPage(
                   page,
                   XGraphicsPdfPageOptions.Replace,
                   XGraphicsUnit.Point,
                   XPageDirection.Downwards))
        {
            new PdfSharpDisplayListRenderer().Render(graphics, page, displayList);
        }

        var content = GetPdfContent(page);

        Assert.Contains("0 80 m\n100 80 l\n100 70 l\n0 70 l\nh", content, StringComparison.Ordinal);
        Assert.Contains("0 65 100 10 re", content, StringComparison.Ordinal);
    }

    [Fact]
    public void PdfSharpDisplayListRendererOmitsTextFullyOutsideClip()
    {
        using var pdfDocument = new PdfDocument();
        pdfDocument.Options.NoCompression = true;
        var page = pdfDocument.AddPage();
        page.Width = XUnit.FromPoint(100);
        page.Height = XUnit.FromPoint(100);
        var displayList = new DisplayList();
        displayList.Add(new PushStateCommand());
        displayList.Add(new ClipCommand(new DisplayRectangle(0F, 0F, 100F, 20F)));
        displayList.Add(CreateDrawTextCommand("Partially visible text", 5F, 25F));
        displayList.Add(new TranslateCommand(new DisplayPoint(0F, 60F)));
        displayList.Add(CreateDrawTextCommand("Clipped text", 5F, 15F));
        displayList.Add(new PopStateCommand());
        displayList.Add(CreateDrawTextCommand("Visible text", 5F, 15F));

        using (var graphics = XGraphics.FromPdfPage(
                   page,
                   XGraphicsPdfPageOptions.Replace,
                   XGraphicsUnit.Point,
                   XPageDirection.Downwards))
        {
            new PdfSharpDisplayListRenderer().Render(graphics, page, displayList);
        }

        var content = GetPdfContent(page);

        Assert.DoesNotContain("(Clipped text) Tj", content, StringComparison.Ordinal);
        Assert.Contains("(Partially visible text) Tj", content, StringComparison.Ordinal);
        Assert.Contains("(Visible text) Tj", content, StringComparison.Ordinal);
    }

    private static DrawTextCommand CreateDrawTextCommand(string text, float x, float y)
        => new(
            new DisplayTextStyle
            {
                Foreground = DisplayColor.Black,
                FontFamily = DisplayFont.Default,
                FontSize = 10F,
            },
            72.272F,
            text,
            x,
            y);

    [Fact]
    public async Task PdfSharpRendererGeneratesXmlTemplateWithText()
    {
        var services = new ServiceCollection();
        services.AddPapercraftPdfSharpRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();
        await using var stream = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader(CreateTextTemplate("Hello PDFsharp")));

        await renderer.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream),
            CultureInfo.InvariantCulture);

        var bytes = stream.ToArray();

        Assert.True(bytes.Length > 100);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task PdfSharpRendererReportsMissingFontThroughRenderOutput()
    {
        var services = new ServiceCollection();
        services.AddPapercraftPdfSharpRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();
        var diagnostics = new List<RenderDiagnostic>();
        await using var stream = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader(CreateMissingFontTemplate()));

        await renderer.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream, diagnostics.Add),
            CultureInfo.InvariantCulture,
            new PapercraftRenderOptions { BackendId = PdfSharpRenderBackend.RendererId });

        Assert.Contains(
            diagnostics,
            (q) => q.Code == RenderDiagnosticCodes.MissingFontSubstitution
                   && q.Level is RendererSupportLevel.Degraded);
        var bytes = stream.ToArray();
        Assert.True(bytes.Length > 100);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }

    [Fact]
    public async Task PdfSharpRendererTreatsMissingFontAsUnsupportedInStrictDegradedMode()
    {
        var services = new ServiceCollection();
        services.AddPapercraftPdfSharpRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();
        await using var stream = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader(CreateMissingFontTemplate()));

        var exception = await Assert.ThrowsAsync<RenderValidationException>(
            async () => await renderer.RenderAsync(
                reader,
                new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream),
                CultureInfo.InvariantCulture,
                new PapercraftRenderOptions
                {
                    BackendId = PdfSharpRenderBackend.RendererId,
                    TreatDegradedAsUnsupported = true,
                }));

        Assert.Contains(
            exception.ValidationResult.Diagnostics,
            (q) => q.Code == RenderDiagnosticCodes.MissingFontSubstitution);
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public async Task PdfSharpRendererKeepsCalibriRegularAndBoldFacesDistinct()
    {
        if (!HasCalibriRegularAndBold())
        {
            return;
        }

        var services = new ServiceCollection();
        services.AddPapercraftPdfSharpRenderer();
        using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();
        await using var stream = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader(CreateCalibriTableTemplate()));

        await renderer.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream),
            CultureInfo.InvariantCulture,
            new PapercraftRenderOptions
            {
                BackendId = PdfSharpRenderBackend.RendererId,
                DocumentOptions = new DocumentOptions
                {
                    DotsPerInch = 96,
                    PageWidthInMillimeters = 90,
                    PageHeightInMillimeters = 45,
                },
            });

        var baseFonts = ExtractBaseFontNames(stream.ToArray());
        var calibriFonts = baseFonts
            .Where((q) => q.Contains("Calibri", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Contains(calibriFonts, (q) => q.Contains("Calibri,Bold", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(
            calibriFonts,
            (q) => q.Contains("Calibri", StringComparison.OrdinalIgnoreCase)
                   && !q.Contains("Bold", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task PdfSharpRendererUsesPdfSharpMetricsForRightAlignedHeaderText()
    {
        if (!HasCalibriRegularAndBold())
            return;

        const float dpi = 600F;
        const string title = "Ausgangslieferrechnung";
        var services = new ServiceCollection();
        services.AddPapercraft();
        services.AddPapercraftPdfSharpRenderer();
        await using var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<PapercraftRenderer>();
        await using var stream = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader(CreateRightAlignedHeaderTemplate(title)));

        await renderer.RenderAsync(
            reader,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream),
            CultureInfo.InvariantCulture,
            new PapercraftRenderOptions
            {
                BackendId = PdfSharpRenderBackend.RendererId,
                DocumentOptions = new DocumentOptions
                {
                    DotsPerInch = dpi,
                    PageWidthInMillimeters = 210F,
                    PageHeightInMillimeters = 297F,
                },
            });

        var content = GetFirstPagePdfContent(stream.ToArray());
        var clip = FindClipBeforeText(content, title);
        var expectedWidth = MeasurePdfSharpWidth(
            title,
            new TextStyle
            {
                FontSize = 12F,
                FontFamily = new Font("Calibri") { Weight = FontWeights.Bold },
            },
            dpi);

        Assert.True(
            clip.Width >= expectedWidth - 0.5F,
            $"The generated clip width {clip.Width.ToString(CultureInfo.InvariantCulture)} is narrower than the PDFsharp text width {expectedWidth.ToString(CultureInfo.InvariantCulture)}.");
    }

    [Fact]
    public async Task PdfSharpLegacyRendererDoesNotUseSkiaMetricsForNarrowRightAlignedCell()
    {
        if (!HasCalibriRegularAndBold())
            return;

        const string title = "Ausgangslieferrechnung";
        var services = new ServiceCollection();
        services.AddPapercraft();
        services.AddPapercraftPdfSharpRenderer();
        await using var provider = services.BuildServiceProvider();
        var renderer = new PapercraftRenderer(
            provider.GetRequiredService<PapercraftGenerator>(),
            provider.GetServices<IPapercraftRenderBackend>());
        await using var stream = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader(CreateNarrowRightAlignedCellTemplate(title)));

        await renderer.GeneratePdfAsync(
            stream,
            reader,
            CultureInfo.InvariantCulture,
            new PapercraftRenderOptions
            {
                BackendId = PdfSharpRenderBackend.RendererId,
                DocumentOptions = new DocumentOptions
                {
                    DotsPerInch = 600F,
                    PageWidthInMillimeters = 210F,
                    PageHeightInMillimeters = 297F,
                    Margin = new Thickness(98, 78, 78, 78),
                },
            });

        var content = GetFirstPagePdfContent(stream.ToArray());

        Assert.DoesNotContain($"({title}) Tj", content, StringComparison.Ordinal);
        Assert.Contains("(Ausgang", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PdfSharpBackendRendersMixedRegularAndBoldTextAsPdf()
    {
        var backend = new PdfSharpRenderBackend();
        var displayList = new DisplayList();
        displayList.Add(
            new DrawTextCommand(
                new DisplayTextStyle
                {
                    Foreground = DisplayColor.Black,
                    FontFamily = DisplayFont.Default with { Weight = 599 },
                    FontSize = 12F,
                },
                72.272F,
                "Regular",
                4,
                20));
        displayList.Add(
            new DrawTextCommand(
                new DisplayTextStyle
                {
                    Foreground = DisplayColor.Black,
                    FontFamily = DisplayFont.Default with { Weight = 600 },
                    FontSize = 12F,
                },
                72.272F,
                "Bold",
                4,
                40));
        var document = new PapercraftDocument(
            new[]
            {
                new PapercraftPage(
                    0,
                    1,
                    1,
                    new Size(120, 80),
                    DocumentOptions.Default.DotsPerMillimeter,
                    displayList),
            },
            CultureInfo.InvariantCulture,
            DocumentOptions.Default);
        await using var stream = new MemoryStream();

        await backend.RenderAsync(
            document,
            new RenderOutput(PapercraftMediaTypes.ApplicationPdf, stream),
            CancellationToken.None);

        var bytes = stream.ToArray();

        Assert.True(bytes.Length > 100);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }

    private static PapercraftDocument CreateSimpleDocument(bool includeText)
    {
        var displayList = new DisplayList();
        displayList.Add(new DrawRectangleCommand(new DisplayRectangle(4, 6, 48, 20), new DisplayColor(0x33, 0x66, 0x99)));
        displayList.Add(new DrawLineCommand(new DisplayColor(0xCC, 0x22, 0x22), 2, 4, 34, 70, 34));
        displayList.Add(new LinkAnnotationCommand("https://example.com", new DisplayRectangle(4, 6, 48, 20)));
        if (includeText)
        {
            displayList.Add(
                new DrawTextCommand(
                    new DisplayTextStyle
                    {
                        Foreground = DisplayColor.Black,
                        FontFamily = DisplayFont.Default,
                        FontSize = 12F,
                    },
                    72.272F,
                    "Hello Papercraft",
                    4,
                    54));
        }

        return new PapercraftDocument(
            new[]
            {
                new PapercraftPage(
                    0,
                    1,
                    1,
                    new Size(96, 72),
                    DocumentOptions.Default.DotsPerMillimeter,
                    displayList),
            },
            CultureInfo.InvariantCulture,
            DocumentOptions.Default);
    }

    private static PapercraftDocument CreateTranslatedClipDocument()
        => new(
            new[]
            {
                new PapercraftPage(
                    0,
                    1,
                    1,
                    new Size(100, 100),
                    DocumentOptions.Default.DotsPerMillimeter,
                    CreateTranslatedClipDisplayList()),
            },
            CultureInfo.InvariantCulture,
            DocumentOptions.Default);

    private static DisplayList CreateTranslatedClipDisplayList()
    {
        var displayList = new DisplayList();
        displayList.Add(new PushStateCommand());
        displayList.Add(new TranslateCommand(new DisplayPoint(0, 20)));
        displayList.Add(new ClipCommand(new DisplayRectangle(0, 0, 100, 10)));
        displayList.Add(new TranslateCommand(new DisplayPoint(0, -20)));
        displayList.Add(new DrawRectangleCommand(new DisplayRectangle(0, 25, 100, 10), new DisplayColor(0x11, 0x22, 0x33)));
        displayList.Add(new PopStateCommand());
        return displayList;
    }

    private static bool HasRectangle(XElement element, float x, float y, float width, float height)
        => HasNumber(element, "x", x)
           && HasNumber(element, "y", y)
           && HasNumber(element, "width", width)
           && HasNumber(element, "height", height);

    private static bool HasNumber(XElement element, string attributeName, float expected)
        => float.TryParse(
               element.Attribute(attributeName)?.Value,
               NumberStyles.Float,
               CultureInfo.InvariantCulture,
               out var actual)
           && Math.Abs(actual - expected) < 0.001F;

    private static string GetPdfContent(PdfPage page)
    {
        var content = page.Contents.CreateSingleContent();
        var bytes = content.Stream?.UnfilteredValue ?? content.Stream?.Value ?? Array.Empty<byte>();
        return Encoding.ASCII.GetString(bytes);
    }

    private static string GetFirstPagePdfContent(byte[] pdfBytes)
    {
        using var stream = new MemoryStream(pdfBytes);
        using var document = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);
        return GetPdfContent(document.Pages[0]);
    }

    private static DisplayRectangle FindClipBeforeText(string content, string text)
    {
        var textIndex = content.IndexOf($"({text}) Tj", StringComparison.Ordinal);
        Assert.True(textIndex >= 0, $"The generated PDF content did not contain '{text}'.");

        var precedingContent = content[..textIndex];
        var clips = MatchPathClips(precedingContent)
            .Concat(MatchRectangleClips(precedingContent))
            .OrderBy((q) => q.Index)
            .ToArray();
        Assert.NotEmpty(clips);
        return clips[^1].Rectangle;
    }

    private static IEnumerable<(int Index, DisplayRectangle Rectangle)> MatchPathClips(string content)
    {
        const string number = @"-?\d+(?:\.\d+)?";
        var pattern =
            $@"(?<x1>{number})\s+(?<y1>{number})\s+m\s+" +
            $@"(?<x2>{number})\s+(?<y2>{number})\s+l\s+" +
            $@"(?<x3>{number})\s+(?<y3>{number})\s+l\s+" +
            $@"(?<x4>{number})\s+(?<y4>{number})\s+l\s+h\s+W\* n";
        foreach (Match match in Regex.Matches(content, pattern))
        {
            var xs = new[]
            {
                ParseFloat(match.Groups["x1"].Value),
                ParseFloat(match.Groups["x2"].Value),
                ParseFloat(match.Groups["x3"].Value),
                ParseFloat(match.Groups["x4"].Value),
            };
            var ys = new[]
            {
                ParseFloat(match.Groups["y1"].Value),
                ParseFloat(match.Groups["y2"].Value),
                ParseFloat(match.Groups["y3"].Value),
                ParseFloat(match.Groups["y4"].Value),
            };
            yield return (
                match.Index,
                new DisplayRectangle(
                    xs.Min(),
                    ys.Min(),
                    xs.Max() - xs.Min(),
                    ys.Max() - ys.Min()));
        }
    }

    private static IEnumerable<(int Index, DisplayRectangle Rectangle)> MatchRectangleClips(string content)
    {
        const string number = @"-?\d+(?:\.\d+)?";
        var pattern = $@"(?<x>{number})\s+(?<y>{number})\s+(?<width>{number})\s+(?<height>{number})\s+re\s+W\* n";
        foreach (Match match in Regex.Matches(content, pattern))
        {
            yield return (
                match.Index,
                new DisplayRectangle(
                    ParseFloat(match.Groups["x"].Value),
                    ParseFloat(match.Groups["y"].Value),
                    ParseFloat(match.Groups["width"].Value),
                    ParseFloat(match.Groups["height"].Value)));
        }
    }

    private static float MeasurePdfSharpWidth(string text, TextStyle textStyle, float dpi)
    {
        using var graphics = PdfSharpFontHelper.CreateMeasureContext();
        var font = PdfSharpFontHelper.CreateFont(textStyle, dpi);
        return (float)graphics.MeasureString(text, font).Width;
    }

    private static float ParseFloat(string value)
        => float.Parse(value, CultureInfo.InvariantCulture);

    private static bool HasCalibriRegularAndBold()
    {
        var fontsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
        return OperatingSystem.IsWindows()
               && File.Exists(Path.Combine(fontsDirectory, "calibri.ttf"))
               && File.Exists(Path.Combine(fontsDirectory, "calibrib.ttf"));
    }

    private static IReadOnlyCollection<string> ExtractBaseFontNames(byte[] pdfBytes)
    {
        var pdfText = Encoding.Latin1.GetString(pdfBytes);
        return Regex.Matches(pdfText, @"/BaseFont\s*/(?<name>[^\s/<>\[\]\(\)]+)")
            .Select((q) => DecodePdfName(q.Groups["name"].Value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string DecodePdfName(string name)
        => Regex.Replace(
            name,
            "#(?<hex>[0-9A-Fa-f]{2})",
            (match) => ((char)Convert.ToByte(match.Groups["hex"].Value, 16)).ToString());

    private static string CreateTextTemplate(string text)
        => $$"""
             <?xml version="1.0" encoding="utf-8"?>
             <template xmlns="X39.Solutions.PdfTemplate.Controls">
                 <body>
                     <text>{{text}}</text>
                 </body>
             </template>
             """;

    private static string CreateCalibriTableTemplate()
        => """
           <?xml version="1.0" encoding="utf-8"?>
           <template xmlns="X39.Solutions.PdfTemplate.Controls">
               <body>
                   <table margin="2mm">
                       <tr>
                           <td width="25mm">
                               <text fontFamily="Calibri" fontSize="9" horizontalAlignment="right">11,11 EUR</text>
                           </td>
                           <td width="25mm">
                               <text fontFamily="Calibri" fontSize="9" weight="bold" horizontalAlignment="right">22,22 EUR</text>
                           </td>
                       </tr>
                   </table>
               </body>
           </template>
           """;

    private static string CreateRightAlignedHeaderTemplate(string title)
        => $$"""
             <?xml version="1.0" encoding="utf-8"?>
             <template xmlns="X39.Solutions.PdfTemplate.Controls">
                 <header>
                     <text fontFamily="Calibri" fontSize="12" weight="bold" horizontalAlignment="right">{{title}}</text>
                 </header>
                 <body>
                     <text>Body</text>
                 </body>
             </template>
             """;

    private static string CreateNarrowRightAlignedCellTemplate(string title)
        => $$"""
             <?xml version="1.0" encoding="utf-8"?>
             <template xmlns="X39.Solutions.PdfTemplate.Controls">
                 <template.style>
                     <text fontFamily="Calibri" fontSize="9"/>
                 </template.style>
                 <areas>
                     <area left="125mm" top="32mm" width="70mm" height="50mm">
                         <table>
                             <tr>
                                 <td width="1*"/>
                                 <td width="1*">
                                     <text fontSize="12" weight="bold" foreground="#00695C" horizontalAlignment="right">{{title}}</text>
                                 </td>
                             </tr>
                         </table>
                     </area>
                 </areas>
                 <body>
                     <text>Body</text>
                 </body>
             </template>
             """;

    private static string CreateMissingFontTemplate()
        => """
           <?xml version="1.0" encoding="utf-8"?>
           <template xmlns="X39.Solutions.PdfTemplate.Controls">
               <body>
                   <text fontFamily="Papercraft Missing Font 7B2ED63A3D53415C">Missing font</text>
               </body>
           </template>
           """;
}
