# Developer Integration Appendix

Previous: [Troubleshooting](troubleshooting.md) | [Manual home](index.md) | Next: [Renderer backends](render-backends.md)

## What Is This?

The developer integration appendix is the place for application setup and extension details.
It keeps service registration, custom controls, custom transformers, functions, resource resolvers and public interfaces out of the template-author chapters.

## When Should I Use This?

Use this appendix when a template change requires application code, such as adding a new function,
supplying new data, loading images from a custom location, registering a custom control or selecting a render target.
Use [renderer backends](render-backends.md) for output-format choices such as SkiaSharp PDF/PNG, SVG, PDFsharp or ESC/POS.

## How Do I Start?

Start with the Papercraft facade package and the default renderer:
`X39.Solutions.Papercraft`, `AddPapercraft()`, `Papercraft`, `PapercraftSession`, `PapercraftRenderResult`, `PapercraftRenderOptions`,
`DocumentOptions`, `IFunction`, `ITransformer`, `IControl`, `ITemplateData`, `XmlTemplateReader`,
`XmlNodeInformation`, `IResourceResolver`, `IDrawableCanvas`, `IDeferredCanvas`, `IImmediateCanvas`,
`IPropertyAccessCache`, `ITextService` and `IParameterConverter<T>`.

Use `X39.Solutions.PdfTemplate`, `AddPdfTemplateService()` and `Generator` only when maintaining
existing compatibility-package consumers.

## Install

The library targets .NET 10.0.
Install the Papercraft facade package in the application that renders documents:

```shell
dotnet add package X39.Solutions.Papercraft
```

On Linux, also install the SkiaSharp native Linux assets package for the default renderer:

```shell
dotnet add package SkiaSharp.NativeAssets.Linux
```

Existing applications can keep the compatibility package during migration. See
[Compatibility Package And Legacy API](#compatibility-package-and-legacy-api).

## Register Services

Register the built-in parser/runtime, controls, transformers and default SkiaSharp renderer at application startup:

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;

services.AddPapercraft();
```

`AddPapercraft()` registers Papercraft Core services, the built-in controls, the built-in transformers,
`Papercraft`, `PapercraftGenerator`, the compatibility `PapercraftRenderer` and the SkiaSharp render backend.

Use the builder overload when the application needs to add or replace template features:

```csharp
services.AddPapercraft(
    (builder) => builder
        .AddFunction<MyFunction>()
        .AddControl<MyControl>()
        .AddTransformer<MyTransformer>());
```

Use the renderer-neutral setup only when the application supplies its own render backend:

```csharp
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;

services.AddPapercraftCore();
services.AddTransient<IPapercraftRenderBackend, MyRenderBackend>();
```

When a `PapercraftSession` selects a backend by `BackendId` or render target, it uses that backend's
`ITextService` while generating the document. Custom backends must expose a text service that matches
their rendering behavior. Renderer-specific package setup and output examples are documented in
[Renderer backends](render-backends.md).

## Generate A PDF

Resolve `Papercraft`, create a session, create an `XmlReader` for the template, and write the PDF to a stream:

```csharp
using System.Globalization;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.Papercraft;

await using var scope = serviceProvider.CreateAsyncScope();
var papercraft = scope.ServiceProvider.GetRequiredService<Papercraft>();
await using var session = papercraft.CreateSession();

using var reader = XmlReader.Create(xmlTemplateStream);
await using var output = File.Create("document.pdf");

await session.GeneratePdfAsync(output, reader, CultureInfo.CurrentUICulture);
```

`Papercraft` is the DI service. `PapercraftSession` owns per-workflow template data and is disposable.
Create one session for each isolated render workflow.
`PapercraftSessionExtensions` provides common helpers such as `GeneratePdfAsync`,
`RenderPdfAsync`, `ReadLoweredXmlAsync` and `GenerateLoweredXmlAsync`; these helpers delegate to the
renderer-neutral `RenderAsync` target model.

## Supply Template Data

Template authors can only use data that the application supplies.
Set variables before rendering:

```csharp
session.TemplateData.SetVariable("CustomerName", customer.Name);
session.TemplateData.SetVariable("InvoiceTotal", invoice.Total);
```

The template can then read those values:

```xml
<template>
    <body>
        <text>@CustomerName</text>
        <text>Total: @InvoiceTotal</text>
    </body>
</template>
```

If a template author needs a new value, the application should expose it as a variable or function instead of asking the author to hard-code business logic in XML.

## Configure Document Options

Pass `PapercraftRenderOptions` when page setup, backend selection or per-request context must differ from the default:

```csharp
using System.Globalization;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Data;

await session.RenderAsync(
    reader,
    new RenderOutput(RenderTarget.Pdf, output),
    CultureInfo.CurrentUICulture,
    new PapercraftRenderOptions
    {
        DocumentOptions = new DocumentOptions
        {
            Margin = new Thickness(new Length(1, ELengthUnit.Centimeters)),
            Producer = "Invoice service",
            Context = new PrintRequestContext(invoice.Id),
            EmbeddedFiles =
            [
                EmbeddedFile.FromText("invoice-data.json", invoiceJson, mediaType: "application/json") with
                {
                    Description = "Machine-readable invoice data",
                    Relationship = EmbeddedFileRelationship.Data,
                    Modified = DateTimeOffset.UtcNow,
                },
            ],
        },
    });
```

Useful options include:

| Option | Use |
|--------|-----|
| `DocumentOptions.PageWidthInMillimeters` and `DocumentOptions.PageHeightInMillimeters` | Change the page size. Defaults match A4 dimensions. |
| `DocumentOptions.DotsPerInch`, `DocumentOptions.DotsPerCentimeter` and `DocumentOptions.DotsPerMillimeter` | Change render density. |
| `DocumentOptions.Margin` | Reserve document margin before header, body and footer layout. |
| `DocumentOptions.Producer` and `DocumentOptions.Modified` | Set PDF metadata. |
| `DocumentOptions.EmbeddedFiles` | Attach eagerly captured files to PDF output. Non-PDF targets report an unsupported-feature diagnostic. |
| `DocumentOptions.Context` | Pass request-specific information to context-aware extension points. |
| `DocumentOptions.IgnoreErrors` | Instruct the generator to ignore errors where possible. Use carefully, because XML can still become invalid. |
| `BackendId` | Select a registered render backend by renderer id. |
| `TreatDegradedAsUnsupported` | Treat degraded validation diagnostics as render-blocking diagnostics. |

Embedded-file names must be unique ignoring case and must be plain file names rather than paths. Media types,
descriptions and associated-file relationships are validated before PDF rendering. The PDF backends write the
attachment name tree, associated-file entries, timestamps, content size and checksum. A prepared
`PapercraftDocument` snapshots its attachment collection and can be rendered repeatedly.

## Validate Before Rendering

Call `ValidateAsync` when an application lets users choose output formats or renderer backends:

```csharp
var result = await session.ValidateAsync(
    reader,
    RenderTarget.Pdf,
    CultureInfo.CurrentUICulture);

if (!result.IsSupported)
{
    // Show result.Diagnostics to the user.
}
```

Validation reads the XML template. Create a new `XmlReader` for the actual render after validation.
Unsupported diagnostics block rendering. Degraded diagnostics are warnings unless
`PapercraftRenderOptions.TreatDegradedAsUnsupported` is enabled.

## Inspect Lowered XML Nodes

Use lowered XML output when you need to diagnose what the template language produced before controls, layout or
render backends are involved. This is the easiest way to inspect variable substitution, function results, transformer
expansion and style application.

`PapercraftSession` handles `RenderTarget.LoweredXml` directly. It reads the source XML, evaluates template-data
expressions in text and attributes, runs the registered `ITransformer` implementations, applies style nodes, returns the
materialized XML tree as a render result and stops there.

The in-memory diagnostics path is:

```csharp
using System.Globalization;
using System.Xml;
using X39.Solutions.Papercraft;

session.TemplateData.SetVariable("CustomerName", customer.Name);

using var reader = XmlReader.Create(xmlTemplateStream);

var loweredXml = await session.ReadLoweredXmlAsync(reader, CultureInfo.CurrentUICulture);
```

The same output can be written to a stream:

```csharp
using var reader = XmlReader.Create(xmlTemplateStream);
await using var output = File.Create("template.lowered.xml");

await session.GenerateLoweredXmlAsync(output, reader, CultureInfo.CurrentUICulture);
```

`RenderTarget.FromMediaType(PapercraftMediaTypes.ApplicationPapercraftLoweredXml)` resolves to the same target.
The output media type is `application/vnd.papercraft.lowered+xml`.

Lowered XML output bypasses `IPapercraftRenderBackend` selection. `BackendId`, backend capability validation,
backend-owned `ITextService` instances, layout, display-list generation and control rendering do not apply.
`ValidateAsync(..., RenderTarget.LoweredXml, ...)` only verifies that the template can be read and lowered.

Use `PapercraftGenerator.ReadLoweredXmlAsync(...)` when application code needs the in-memory
`XmlNodeInformation` tree instead of serialized XML.

Lowered XML reads consume the `XmlReader`, just like validation and rendering. Create a new `XmlReader` if you render
the same template after inspection.

## Trace Renderer Activity

Papercraft emits phase-level renderer activities through `PapercraftInstrumentation.ActivitySource`.
Applications that use OpenTelemetry can install `X39.Solutions.Papercraft.OpenTelemetry` and register the source during host setup:

```csharp
using X39.Solutions.Papercraft.OpenTelemetry;

builder.AddPapercraftOpenTelemetry(
    (tracing) =>
    {
        // Add exporters or processors here.
    });
```

The tracing package only wires Papercraft into OpenTelemetry. Exporters, sampling and resource configuration remain application-owned.

## Add A Function

Use a function when a template needs a reusable value or calculation such as a formatted total, lookup result or application-specific label.

```csharp
using System.Globalization;
using X39.Solutions.Papercraft.Abstraction;

public sealed class CustomerLabelFunction : IFunction
{
    public string Name => "customerLabel";
    public int Arguments => 1;
    public bool IsVariadic => false;

    public ValueTask<object?> ExecuteAsync(
        CultureInfo cultureInfo,
        object?[] arguments,
        CancellationToken cancellationToken = default)
    {
        var customerId = Convert.ToString(arguments[0], cultureInfo);
        return ValueTask.FromResult<object?>($"Customer {customerId}");
    }
}
```

Register the function:

```csharp
services.AddPapercraft(
    (builder) => builder.AddFunction<CustomerLabelFunction>());
```

A template author can call it after the application exposes it:

```xml
<text>@customerLabel(CustomerId)</text>
```

## Add A Custom Control

Use a custom control when the built-in XML controls cannot render a required visual element.
Most custom controls should derive from `Control` or an existing base control instead of implementing `IControl` directly.

```csharp
using System.Globalization;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Attributes;
using X39.Solutions.Papercraft.Controls.Base;
using X39.Solutions.Papercraft.Data;

[Control(Constants.ControlsNamespace, "approvalStamp")]
public sealed class ApprovalStampControl : Control
{
    protected override Size DoMeasure(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => Size.Zero;

    protected override Size DoArrange(
        float dpi,
        in Size fullPageSize,
        in Size framedPageSize,
        in Size remainingSize,
        CultureInfo cultureInfo)
        => Size.Zero;

    protected override Size DoRender(
        IDeferredCanvas canvas,
        float dpi,
        in Size parentSize,
        CultureInfo cultureInfo)
        => Size.Zero;
}
```

Register the control:

```csharp
services.AddPapercraft(
    (builder) => builder.AddControl<ApprovalStampControl>());
```

Use the registered element name in the template:

```xml
<template>
    <body>
        <approvalStamp/>
    </body>
</template>
```

Elements without an XML namespace are treated as if they are in the runtime's built-in control namespace.
Registering a custom control with `Constants.ControlsNamespace` lets template authors use it beside built-in controls
without adding `xmlns`, as shown above.

If a template sets a custom default XML namespace, unprefixed elements move into that namespace and built-in controls
are not found unless the application registers matching controls there. Current templates should not use prefixed
element names such as `default:text`; the reader validates the XML element name itself and rejects prefixed control names.

Existing compatibility templates may still contain the legacy XML namespace
`X39.Solutions.PdfTemplate.Controls`. Keep that namespace unchanged during a package migration unless the
application team has verified the runtime namespace registrations for the release being used.

## Add A Transformer

Use a transformer only when XML nodes need to be included, removed, repeated or rewritten before controls are created.
For simple calculated values, prefer a function.

```csharp
using System.Globalization;
using System.Runtime.CompilerServices;
using X39.Solutions.Papercraft.Abstraction;
using XmlNode = X39.Solutions.Papercraft.Xml.XmlNode;

public sealed class KeepChildrenTransformer : ITransformer
{
    public string Name => "keep";

    public async IAsyncEnumerable<XmlNode> TransformAsync(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<XmlNode> nodes,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        foreach (var node in nodes)
        {
            yield return node.DeepCopy();
        }
    }
}
```

Register the transformer:

```csharp
services.AddPapercraft(
    (builder) => builder.AddTransformer<KeepChildrenTransformer>());
```

The transformer is then available by name:

```xml
@keep {
    <text>This node is returned by the custom transformer.</text>
}
```

## Add An Image Resolver

The default image resolver accepts base64 image data and `data:image/...;base64,...` sources.
It does not read the file system or the internet.
Register a custom `IResourceResolver` after `AddPapercraft` when templates need images from application storage:

```csharp
using X39.Solutions.Papercraft.Services.ResourceResolver;

services.AddPapercraft();
services.AddScoped<IResourceResolver, ApplicationImageResolver>();
```

```csharp
public sealed class ApplicationImageResolver : IResourceResolver
{
    public async ValueTask<byte[]> ResolveImageAsync(
        string source,
        object? context,
        CancellationToken cancellationToken = default)
    {
        var request = context as PrintRequestContext;
        return await LoadImageBytesAsync(source, request, cancellationToken);
    }
}
```

`PapercraftRenderOptions.DocumentOptions.Context` is passed unchanged to
`IResourceResolver.ResolveImageAsync` and to controls that implement `IInitializeControlAsync`.

## Extension Point Map

Choose the smallest extension point that solves the template author's request.
Most requests fit one of these rows:

| Interface or type | Use when | How it is wired |
|-------------------|----------|-----------------|
| `Papercraft` | The application needs to render or validate a template. | Resolve it from dependency injection, then create a disposable `PapercraftSession` for each isolated render workflow. |
| `PapercraftSession` | A render workflow needs template data, validation or output. | Set `TemplateData`, call `ValidateAsync`, `RenderAsync` or `RenderRasterPagesAsync`, then dispose the session. |
| `PapercraftGenerator` | Advanced code needs a backend-neutral `PapercraftDocument` before rendering. | Resolve it from dependency injection when a backend-neutral document is needed directly. |
| `ITemplateData` | The application supplies variables, registers functions or evaluates expressions inside custom extensions. | Use `session.TemplateData` before rendering; custom transformers and functions receive it through their APIs. |
| `IFunction` | The template needs a reusable calculated value. | Implement `Name`, argument metadata and `ExecuteAsync`, then register with `AddFunction<TFunction>()`. |
| `IControl` | The application must render a new XML element. | Add `[Control(...)]`, implement measure/arrange/render behavior and register with `AddControl<TControl>()`. |
| `IContentControl` | A custom control must contain child controls. | Implement `CanAdd` and child storage, or derive from an existing content-control base. |
| `IInitializeControlAsync` | A control needs async setup or request context before rendering. | Implement it on the control; `DocumentOptions.Context` is passed to `InitializeControlAsync`. |
| `ITransformer` | XML nodes must be conditionally rewritten, removed or repeated before controls are created. | Implement `Name` and `TransformAsync`, then register with `AddTransformer<TTransformer>()`. |
| `RenderTarget.LoweredXml` | The application needs lowered XML before controls, layout or backend rendering. | Use `session.ReadLoweredXmlAsync(...)`, `session.GenerateLoweredXmlAsync(...)`, or `session.RenderAsync(..., RenderTarget.LoweredXml, ...)` and `PapercraftRenderResult.ReadText()`. |
| `PapercraftGenerator.ReadLoweredXmlAsync(...)` and `XmlNodeInformation` | The application needs the in-memory lowered template tree before controls are created. | Resolve a `PapercraftGenerator`, set its `TemplateData` and call `ReadLoweredXmlAsync(...)`. |
| `IResourceResolver` | Image sources must be resolved from application-specific storage. | Register an `IResourceResolver` implementation after `AddPapercraft`. |
| `IParameterConverter<T>` | A custom control attribute needs custom parsing. | Set the converter on the control property with `ParameterAttribute.Converter`. |
| `IPapercraftRenderBackend` | The application needs a custom output backend. | Expose backend capabilities, `ITextService`, and render methods, then register the backend with DI. |
| `IControlFactory` | Advanced activation behavior is needed. | Replace the DI service only for unusual activation needs; most applications should use `AddControl<TControl>()`. |

Source-checked registration notes:

- `Papercraft` is registered as a singleton-style session factory.
- `PapercraftRenderer` remains registered as a transient obsolete compatibility adapter.
- `PapercraftGenerator` is registered as transient.
- `ITemplateData`, `IResourceResolver` and `IControlFactory` are registered as scoped services.
- `ControlRegistry`, `ControlActivationCache`, `IPropertyAccessCache`, the default unkeyed `ITextService`, `SkPaintCache` and `SkiaSharpDisplayListRenderer` are registered as singleton services.
- Built-in transformer registrations are transient; custom transformers added with `AddTransformer<TTransformer>()` use the same path.
- Functions added with `AddFunction<TFunction>()` are scoped by default, and the builder accepts another `ServiceLifetime`.
- Control registration stores control metadata; a fresh control instance is created for template nodes through the control factory.
- `DocumentOptions.Context` is opaque to the library and is passed unchanged to resource resolvers and async control initialization.

## Advanced Infrastructure Interfaces

These interfaces are public because controls, renderers and extension points use them.
Most applications should not replace them directly.

| Interface | Use |
|-----------|-----|
| `IDrawableCanvas` | Low-level drawing surface used by controls. Custom controls may call it through the deferred canvas. |
| `IDeferredCanvas` | Canvas passed to `IControl.Render`; it records drawing work until page-specific values are available. |
| `IImmediateCanvas` | Canvas exposed inside deferred drawing for page-specific information such as the current page and total pages. |
| `ITextService` | Text measurement and text display-list service used by text-based controls. `PapercraftSession` uses the selected backend's `ITextService`; the unkeyed DI service is a direct-control-activation fallback. |
| `IPropertyAccessCache` | Expression-evaluation infrastructure that caches property access. It is not a normal application extension point. |

## Compatibility Package And Legacy API

Existing applications can keep the compatibility package during the additive migration:

```shell
dotnet add package X39.Solutions.PdfTemplate
```

The compatibility package keeps the legacy service registration and generator entry point available:

```csharp
using System.Globalization;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate;

services.AddPdfTemplateService();

await using var scope = serviceProvider.CreateAsyncScope();
using var generator = scope.ServiceProvider.GetRequiredService<Generator>();

using var reader = XmlReader.Create(xmlTemplateStream);
await using var output = File.Create("document.pdf");

await generator.GeneratePdfAsync(
    output,
    reader,
    CultureInfo.CurrentUICulture);
```

`Generator.GenerateLoweredXmlAsync(...)` writes the same backend-free lowered XML diagnostic output as
`PapercraftRenderer.GenerateLoweredXmlAsync(...)`.
`Generator.GenerateBitmapsAsync(...)` remains the legacy SkiaSharp bitmap API.
For new raster workflows, prefer `PapercraftSession.RenderRasterPagesAsync(...)`.

## When Template Authors Need Developer Help

Ask for application work when a manual page or template needs:

- a variable that is not already present,
- a function that is not already registered,
- an image source that the current resolver cannot load,
- a control that does not exist in the built-in controls,
- a transformer beyond the built-in template language,
- document options that the application does not expose.

Keep those decisions in application code.
The user-facing template should stay focused on XML structure, layout, data placeholders and small task examples.

Previous: [Troubleshooting](troubleshooting.md) | [Manual home](index.md) | Next: [Renderer backends](render-backends.md)
