# Renderer Backends

Previous: [Developer integration appendix](developer-integration.md) | [Manual home](index.md) | Next: [Migration to Papercraft](migration-to-papercraft.md)

## What Is This?

Renderer backends turn a generated Papercraft document into a concrete output format.
The template parser, data binding and layout runtime live in Core; each backend decides how the generated
display list is written to a stream.

Use this chapter when an application needs to choose between PDF, raster image, SVG or printer-command output.
Lowered XML is also exposed as a render target for diagnostics, but it is not produced by a backend.

## Backend Packages

| Package | Backend id | Output | Common use |
|---------|------------|--------|------------|
| `X39.Solutions.Papercraft` | `skiasharp` | PDF and PNG raster output | Normal application setup through the default facade. |
| `X39.Solutions.Papercraft.Rendering.SkiaSharp` | `skiasharp` | `application/pdf`, `image/png` | Explicit default renderer registration, PDF output and page-by-page PNG rendering. |
| `X39.Solutions.Papercraft.Rendering.PdfSharp` | `pdfsharp` | `application/pdf` | Alternative managed PDF backend when PDFsharp integration is preferred. |
| `X39.Solutions.Papercraft.Rendering.Svg` | `svg` | `image/svg+xml` | Dependency-free vector output, previews, inspection and tooling workflows. |
| `X39.Solutions.Papercraft.Rendering.EscPos` | `escpos` | `application/vnd.papercraft.escpos` | First-pass ESC/POS receipt-printer command streams. |

`X39.Solutions.Papercraft.Core` contains the backend contracts and runtime, but it does not register a backend by itself.
It can still write lowered XML through `RenderTarget.LoweredXml`, because that target stops before backend rendering.

## Default PDF And Raster Output

Most applications should start with the facade package:

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;

services.AddPapercraft();
```

This registers Core, the built-in controls, the built-in transformers, `Papercraft`, the compatibility `PapercraftRenderer` and the SkiaSharp backend.
Use a `PapercraftSession` for the common PDF path:

```csharp
var papercraft = serviceProvider.GetRequiredService<Papercraft>();
await using var session = papercraft.CreateSession();

await session.GeneratePdfAsync(output, reader, CultureInfo.CurrentUICulture);
```

Use `RenderRasterPagesAsync(...)` when each page should be written as PNG:

```csharp
await session.RenderRasterPagesAsync(
    reader,
    new RasterPageRenderOutput(
        PapercraftMediaTypes.ImagePng,
        static (page, cancellationToken) =>
            ValueTask.FromResult<Stream>(File.Create($"page-{page.PageNumber}.png"))),
    CultureInfo.CurrentUICulture);
```

Applications running the SkiaSharp backend on Linux should reference the matching SkiaSharp native assets package.

## Explicit Backend Selection

`PapercraftSession` chooses a backend from the requested render target and registered backend capabilities.
When multiple registered backends can produce the same target, pass `BackendId`:

```csharp
using X39.Solutions.Papercraft.Rendering.PdfSharp;

await session.RenderAsync(
    reader,
    new RenderOutput(PapercraftMediaTypes.ApplicationPdf, output),
    CultureInfo.CurrentUICulture,
    new PapercraftRenderOptions
    {
        BackendId = PdfSharpRenderBackend.RendererId,
    });
```

Call `ValidateAsync(...)` before rendering when users can select an output format:

```csharp
var validation = await session.ValidateAsync(
    reader,
    RenderTarget.FromMediaType("image/svg+xml"),
    CultureInfo.CurrentUICulture);

if (!validation.IsSupported)
{
    // Show validation.Diagnostics to the user.
}
```

Unsupported diagnostics block rendering. Degraded diagnostics are warnings unless
`PapercraftRenderOptions.TreatDegradedAsUnsupported` is enabled.

## PDF Attachments

The SkiaSharp and PDFsharp backends support `DocumentOptions.EmbeddedFiles` for PDF output. Attachments are
published through the PDF embedded-file name tree and catalog associated-file array, including MIME type,
description, relationship, timestamps, size and checksum metadata. PNG, SVG, ESC/POS and lowered XML outputs
cannot carry attachments and report an unsupported-feature diagnostic instead of silently omitting them.

SkiaSharp does not expose an attachment API, so attached PDFs are finalized in a seekable in-memory buffer before
being copied to the caller's stream. This temporarily retains the generated PDF in memory. SkiaSharp's PDF/A mode
targets PDF/A-2b and is disabled for attached documents; attachment output carries associated-file structures but
does not claim PDF/A-3 conformance. PDFsharp output likewise makes no PDF/A-3 conformance claim.

## Lowered XML Diagnostics

Use lowered XML output when you need to inspect the template after data binding, function evaluation, transformer
expansion and style application, but before controls are created and before layout or backend rendering starts.

```csharp
var loweredXml = await session.ReadLoweredXmlAsync(reader, CultureInfo.CurrentUICulture);
```

The lowered XML media type is `PapercraftMediaTypes.ApplicationPapercraftLoweredXml`
(`application/vnd.papercraft.lowered+xml`).
This target bypasses backend selection, so `BackendId`, backend capability validation, backend text services,
layout and display-list rendering do not apply.

## SVG Output

Register the SVG backend when an application needs vector output without a native rendering dependency:

```csharp
using X39.Solutions.Papercraft.Rendering.Svg;

services.AddPapercraftSvgRenderer();
```

Render with the SVG media type:

```csharp
await session.RenderAsync(
    reader,
    new RenderOutput(RenderTarget.FromMediaType(SvgRenderBackend.MediaType), output),
    CultureInfo.CurrentUICulture);
```

Multi-page documents are written as one SVG with pages stacked vertically.
SVG output is useful for previews and inspection, but exact text metrics can differ from PDF backends.

## ESC/POS Printer Commands

Register the ESC/POS backend when the application wants receipt-printer command bytes instead of a PDF or image:

```csharp
using X39.Solutions.Papercraft.Rendering.EscPos;

services.AddPapercraftEscPosRenderer(
    new EscPosRenderOptions
    {
        CharactersPerLine = 42,
        CutPaper = true,
    });
```

Render to a caller-owned stream:

```csharp
await using var output = new MemoryStream();

await session.RenderAsync(
    reader,
    new RenderOutput(EscPosRenderBackend.Target, output),
    CultureInfo.CurrentUICulture,
    new PapercraftRenderOptions
    {
        BackendId = "escpos",
    });

byte[] commandBytes = output.ToArray();
```

The backend only writes ESC/POS command bytes. It does not discover printers, open USB or TCP connections,
send spooler jobs, or perform any other transport work.

The first-pass backend is text-oriented. It emits printer initialization, text, line feeds, simple emphasis commands,
simple horizontal rules, optional feed lines and optional cut commands. It reports unsupported diagnostics for
images, clipping, transparency, link annotations, filled rectangles, non-horizontal lines and backend-specific drawing callbacks.
It reports degraded diagnostics for features that cannot be preserved exactly by text-oriented ESC/POS output,
such as colors, arbitrary fonts, absolute positioning, multipage layout and rotated text.

Receipt templates should therefore stay intentionally simple: use text, predictable line order and horizontal separators.
For logos, QR codes, barcodes or exact visual layout, add semantic printer-command support before treating the backend
as a general replacement for PDF output.

## Custom Backends

Custom renderers implement `IPapercraftRenderBackend` and advertise their output through `RendererCapabilities`.
They also expose an `ITextService` that Papercraft uses while generating documents for that backend.
Register them with Core when the application does not want the default facade:

```csharp
services.AddPapercraftCore();
services.AddTransient<IPapercraftRenderBackend, MyRenderBackend>();
```

Backends should validate unsupported display-list features before rendering and write only to the supplied
`RenderOutput.Stream`. Device transport, storage and network delivery should live in the application layer.

Previous: [Developer integration appendix](developer-integration.md) | [Manual home](index.md) | Next: [Migration to Papercraft](migration-to-papercraft.md)
