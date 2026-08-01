# X39.Solutions.Papercraft.Rendering.PdfSharp

`X39.Solutions.Papercraft.Rendering.PdfSharp` is a Papercraft render backend for PDF output through PDFsharp.
It provides an alternative PDF backend for applications that want a managed PDFsharp-based renderer.

Use this package when you want to choose the PDFsharp backend explicitly.
Most application code can reference [`X39.Solutions.Papercraft`](../X39.Solutions.Papercraft/README.md) instead, which registers the default SkiaSharp renderer automatically.

## Package Role

| Area | Provided by this package |
|------|--------------------------|
| DI entry point | `services.AddPapercraftPdfSharpRenderer()` |
| Backend implementation | `PdfSharpRenderBackend` |
| Display-list renderer | `PdfSharpDisplayListRenderer` |
| Supported media types | `application/pdf` |
| PDF attachments | Embedded-file name tree and associated-file structures |

The backend declares support for PDF output, multipage documents, text drawing, images, rectangular clipping, transparency, fonts, color, absolute positioning, link annotations and document attachments.

## Register The Renderer

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft.Rendering.PdfSharp;

var services = new ServiceCollection();
services.AddPapercraftPdfSharpRenderer();
```

`AddPapercraftPdfSharpRenderer()` also registers Papercraft Core services.
After registration, resolve `Papercraft` from the service provider, create a `PapercraftSession`, and render with `PapercraftRenderOptions.BackendId = PdfSharpRenderBackend.RendererId` when multiple PDF-capable backends are registered.

## Output Notes

This backend produces PDF only and does not support raster output.
Attachments include names, original bytes, MIME types, descriptions, associated-file relationships, timestamps,
sizes and MD5 checksums. The output does not claim PDF/A-3 conformance.
PDFsharp font settings are global; applications with strict font requirements should configure PDFsharp font resolution before rendering.
The package includes a lightweight text measurement service for template generation; exact wrapping and metrics can differ from the SkiaSharp backend.

## Related Projects

- [`X39.Solutions.Papercraft.Core`](../X39.Solutions.Papercraft.Core/README.md): renderer-neutral contracts consumed by this backend.
- [`X39.Solutions.Papercraft.Rendering.SkiaSharp`](../X39.Solutions.Papercraft.Rendering.SkiaSharp/README.md): default PDF and raster backend.
- [`X39.Solutions.Papercraft`](../X39.Solutions.Papercraft/README.md): default facade that registers the SkiaSharp backend automatically.
