# X39.Solutions.Papercraft.Rendering.SkiaSharp

`X39.Solutions.Papercraft.Rendering.SkiaSharp` is the default Papercraft render backend.
It renders Papercraft display lists through SkiaSharp and supports PDF output plus PNG raster output.

Use this package when you want to choose the SkiaSharp backend explicitly.
Most application code can reference [`X39.Solutions.Papercraft`](../X39.Solutions.Papercraft/README.md) instead, which registers this renderer automatically.

## Package Role

| Area | Provided by this package |
|------|--------------------------|
| DI entry point | `services.AddPapercraftSkiaSharpRenderer()` |
| Backend implementation | `SkiaSharpRenderBackend` |
| Display-list renderer | `SkiaSharpDisplayListRenderer` |
| Runtime services | `SkPaintCache`, SkiaSharp text service |
| Supported media types | `application/pdf`, `image/png` |
| PDF attachments | Embedded-file name tree and associated-file structures |

The backend declares support for PDF output, raster image output, multipage documents, text measurement and drawing, images, clipping, transparency, fonts, color, absolute positioning and PDF document attachments.

## Register The Renderer

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Rendering.SkiaSharp;

var services = new ServiceCollection();
services.AddPapercraftSkiaSharpRenderer();
```

`AddPapercraftSkiaSharpRenderer()` also registers Papercraft Core services.
After registration, resolve `Papercraft` from the service provider and create a `PapercraftSession` to render through the selected backend.

## Output Paths

Render a PDF:

```csharp
var papercraft = serviceProvider.GetRequiredService<Papercraft>();
await using var session = papercraft.CreateSession();

await session.RenderAsync(
    xmlReader,
    new RenderOutput(RenderTarget.Pdf, outputStream),
    CultureInfo.InvariantCulture);
```

Render each page as PNG:

```csharp
await session.RenderRasterPagesAsync(
    xmlReader,
    new RasterPageRenderOutput(
        PapercraftMediaTypes.ImagePng,
        static (page, cancellationToken) =>
            ValueTask.FromResult<Stream>(File.Create($"page-{page.PageNumber}.png"))),
    CultureInfo.InvariantCulture);
```

Single-stream PNG output is supported for one-page documents. For multi-page raster output, use `RenderRasterPagesAsync`.
Attachments are supported only for PDF output; raster output reports an unsupported-feature diagnostic. Because
SkiaSharp has no attachment API, attached PDFs are completed in an in-memory seekable buffer before being copied
to the destination stream. SkiaSharp's PDF/A-2b mode is disabled for documents with attachments, and the resulting
PDF does not claim PDF/A-3 conformance.

## Platform Notes

This package references `SkiaSharp`.
Applications running on Linux should also reference the matching SkiaSharp native assets package, for example:

```shell
dotnet add package SkiaSharp.NativeAssets.Linux
```

The test and benchmark projects reference native asset packages explicitly because they execute renderer code.

## Related Projects

- [`X39.Solutions.Papercraft`](../X39.Solutions.Papercraft/README.md): default facade that registers this backend automatically.
- [`X39.Solutions.Papercraft.Core`](../X39.Solutions.Papercraft.Core/README.md): renderer-neutral contracts consumed by this backend.
- [`X39.Solutions.PdfTemplate`](../X39.Solutions.PdfTemplate/README.md): compatibility package that also uses this renderer.
