using System.Xml;
using System.Diagnostics;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Canvas;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Functions;
using X39.Solutions.Papercraft.Services.TextService;
using X39.Solutions.Papercraft.Xml;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Backend-neutral Papercraft template generator.
/// </summary>
/// <remarks>
/// This class is not thread safe. Resolve a fresh instance for concurrent document generation.
/// </remarks>
public sealed class PapercraftGenerator : IDisposable, IAsyncDisposable
{
    private readonly IControlFactory _controlFactory;
    private readonly Dictionary<string, object> _data = new();
    private readonly IReadOnlyCollection<ITransformer> _transformers;

    /// <summary>
    /// Creates a new backend-neutral generator.
    /// </summary>
    public PapercraftGenerator(
        IControlFactory controlFactory,
        IEnumerable<IFunction> functions,
        IEnumerable<ITransformer> transformers)
    {
        ArgumentNullException.ThrowIfNull(controlFactory);
        ArgumentNullException.ThrowIfNull(functions);
        ArgumentNullException.ThrowIfNull(transformers);

        TemplateData = new TemplateData();
        TemplateData.RegisterFunction(new AllTemplateDataFunctions(TemplateData));
        TemplateData.RegisterFunction(new AllTemplateDataVariables(TemplateData));
        foreach (var function in functions)
        {
            TemplateData.RegisterFunction(function);
        }

        var transformerArray = transformers.ToArray();
        var duplicateTransformers = transformerArray
            .GroupBy((q) => q.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault((q) => q.Count() > 1);
        if (duplicateTransformers is not null)
            throw new InvalidOperationException(
                $"The transformer {duplicateTransformers.Key} is registered more than once.");

        _controlFactory = controlFactory;
        _transformers = transformerArray;
    }

    /// <summary>
    /// Template data available to this generator.
    /// </summary>
    public ITemplateData TemplateData { get; }

    /// <summary>
    /// Adds data to the generator and exposes it as a template variable.
    /// </summary>
    public void AddData(string key, object data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(data);
        _data.Add(key, data);
        TemplateData.SetVariable(key, data);
    }

    /// <summary>
    /// Generates a backend-neutral document from the supplied template reader.
    /// </summary>
    public async ValueTask<PapercraftDocument> GenerateAsync(
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions? documentOptions = default,
        CancellationToken cancellationToken = default)
        => await GenerateAsync(
                reader,
                cultureInfo,
                documentOptions,
                _controlFactory,
                cancellationToken)
            .ConfigureAwait(false);

    internal async ValueTask<PapercraftDocument> GenerateAsync(
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions? documentOptions,
        IControlFactory controlFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(cultureInfo);
        ArgumentNullException.ThrowIfNull(controlFactory);
        cancellationToken.ThrowIfCancellationRequested();

        using var activity = PapercraftActivity.Start(PapercraftActivityNames.GeneratorGenerate);
        try
        {
            var options = documentOptions ?? DocumentOptions.Default;
            var rootNode = await ReadLoweredXmlAsync(reader, cultureInfo, options, cancellationToken)
                .ConfigureAwait(false);

            await using var template = await Template.CreateAsync(
                    rootNode,
                    controlFactory,
                    cultureInfo,
                    options.Context,
                    cancellationToken)
                .ConfigureAwait(false);

            var layout = MeasureAndArrange(template, options, cultureInfo);
            var pages = ComposePages(template, options, layout, cultureInfo, cancellationToken);
            var document = new PapercraftDocument(pages, cultureInfo, options);
            PapercraftActivity.SetDocument(activity, document);
            return document;
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    internal IControlFactory? CreateTextServiceScopedControlFactory(ITextService textService)
    {
        ArgumentNullException.ThrowIfNull(textService);
        return _controlFactory is ControlFactory controlFactory
            ? controlFactory.WithTextService(textService)
            : null;
    }

    /// <summary>
    /// Reads the supplied template through the same lowering step used before control creation.
    /// </summary>
    /// <returns>The materialized XML node tree after expression evaluation, transformer expansion and style application.</returns>
    public async ValueTask<XmlNodeInformation> ReadLoweredXmlAsync(
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions? documentOptions = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(cultureInfo);
        cancellationToken.ThrowIfCancellationRequested();

        var options = documentOptions ?? DocumentOptions.Default;
        using var templateDataScope = TemplateData.Scope("Document");
        using var templateReader = new XmlTemplateReader(options, cultureInfo, TemplateData, _transformers);
        return await templateReader.ReadAsync(reader, cancellationToken)
            .ConfigureAwait(false);
    }

    private static RenderLayout MeasureAndArrange(
        Template template,
        DocumentOptions options,
        CultureInfo cultureInfo)
    {
        using var activity = PapercraftActivity.Start(PapercraftActivityNames.GeneratorMeasureArrange);
        try
        {
        var originalPageSize = GetOriginalPageSize(options);
        var marginLeft = options.Margin.Left.ToPixels(originalPageSize.Width, options.DotsPerInch);
        var marginTop = options.Margin.Top.ToPixels(originalPageSize.Height, options.DotsPerInch);
        var pageSize = new Size(
            originalPageSize.Width
            - marginLeft
            - options.Margin.Right.ToPixels(originalPageSize.Width, options.DotsPerInch),
            originalPageSize.Height
            - marginTop
            - options.Margin.Bottom.ToPixels(originalPageSize.Height, options.DotsPerInch));

        foreach (var control in Enumerable.Empty<IControl>()
                     .Concat(template.BackgroundControls)
                     .Concat(template.HeaderControls)
                     .Concat(template.BodyControls)
                     .Concat(template.FooterControls)
                     .Concat(template.ForegroundControls))
        {
            control.Measure(options.DotsPerInch, pageSize, pageSize, pageSize, cultureInfo);
        }

        foreach (var area in template.AreaControls)
        {
            var tuple = area.CalculateClippingAndTranslationData(options.DotsPerInch, originalPageSize);
            foreach (var areaControl in area.Controls)
            {
                areaControl.Measure(options.DotsPerInch, tuple.size, tuple.size, tuple.size, cultureInfo);
            }
        }

        var backgroundPageSize = originalPageSize with { Height = originalPageSize.Height };
        var backgroundSizes = ArrangeControls(
            template.BackgroundControls,
            options.DotsPerInch,
            originalPageSize,
            backgroundPageSize,
            backgroundPageSize,
            cultureInfo);

        var headerPageSize = pageSize with { Height = pageSize.Height * 0.25F };
        var headerSizes = ArrangeControls(
            template.HeaderControls,
            options.DotsPerInch,
            pageSize,
            headerPageSize,
            headerPageSize,
            cultureInfo);
        headerPageSize = headerPageSize with { Height = Math.Min(headerSizes.Sum((q) => q.Height), pageSize.Height * 0.25F) };

        var footerPageSize = pageSize with { Height = pageSize.Height * 0.25F };
        var footerSizes = ArrangeControls(
            template.FooterControls,
            options.DotsPerInch,
            pageSize,
            footerPageSize,
            footerPageSize,
            cultureInfo);
        footerPageSize = footerPageSize with { Height = Math.Min(footerSizes.Sum((q) => q.Height), pageSize.Height * 0.25F) };

        var bodyPageSize = pageSize with { Height = pageSize.Height - headerPageSize.Height - footerPageSize.Height };
        var bodySizes = ArrangeControls(
            template.BodyControls,
            options.DotsPerInch,
            pageSize,
            bodyPageSize,
            bodyPageSize,
            cultureInfo);

        var foregroundPageSize = originalPageSize with { Height = originalPageSize.Height };
        var foregroundSizes = ArrangeControls(
            template.ForegroundControls,
            options.DotsPerInch,
            originalPageSize,
            foregroundPageSize,
            foregroundPageSize,
            cultureInfo);

        var areaSizes = new List<(int areaIndex, Size size)>();
        for (var areaIndex = 0; areaIndex < template.AreaControls.Count; areaIndex++)
        {
            var area = template.AreaControls.ElementAt(areaIndex);
            var tuple = area.CalculateClippingAndTranslationData(options.DotsPerInch, originalPageSize);
            foreach (var control in area.Controls)
            {
                var size = control.Arrange(options.DotsPerInch, tuple.size, tuple.size, tuple.size, cultureInfo);
                areaSizes.Add((areaIndex, size));
            }
        }

        return new RenderLayout(
            originalPageSize,
            pageSize,
            new Point(marginLeft, marginTop),
            backgroundPageSize,
            backgroundSizes,
            headerPageSize,
            headerSizes,
            bodyPageSize,
            bodySizes,
            footerPageSize,
            footerSizes,
            foregroundPageSize,
            foregroundSizes,
            areaSizes);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private static IReadOnlyList<PapercraftPage> ComposePages(
        Template template,
        DocumentOptions options,
        RenderLayout layout,
        CultureInfo cultureInfo,
        CancellationToken cancellationToken)
    {
        using var activity = PapercraftActivity.Start(PapercraftActivityNames.GeneratorComposePages);
        try
        {
        var backgroundCanvas = CreateLayerCanvas(layout.OriginalPageSize, layout.OriginalPageSize);
        using (StartLayerActivity("background"))
        {
        RenderSequentialControls(
            backgroundCanvas,
            template.BackgroundControls,
            layout.BackgroundSizes,
            options.DotsPerInch,
            layout.BackgroundPageSize,
            cultureInfo);
        }

        var headerCanvas = CreateLayerCanvas(layout.OriginalPageSize, layout.PageSize);
        using (StartLayerActivity("header"))
        {
        RenderSequentialControls(
            headerCanvas,
            template.HeaderControls,
            layout.HeaderSizes,
            options.DotsPerInch,
            layout.HeaderPageSize,
            cultureInfo);
        }

        var bodyCanvas = CreateLayerCanvas(layout.OriginalPageSize, layout.BodyPageSize);
        var desiredBodyHeight = layout.BodySizes.Sum((q) => q.Height);
        using (StartLayerActivity("body"))
        {
        using (bodyCanvas.CreateState())
        {
            foreach (var (control, size) in template.BodyControls.Zip(layout.BodySizes))
            {
                var (_, additionalHeight) = control.Render(
                    bodyCanvas,
                    options.DotsPerInch,
                    layout.BodyPageSize,
                    cultureInfo);
                desiredBodyHeight += additionalHeight;
                bodyCanvas.Translate(0F, size.Height + additionalHeight);
            }
        }
        }

        var footerCanvas = CreateLayerCanvas(layout.OriginalPageSize, layout.PageSize);
        using (StartLayerActivity("footer"))
        {
        RenderSequentialControls(
            footerCanvas,
            template.FooterControls,
            layout.FooterSizes,
            options.DotsPerInch,
            layout.FooterPageSize,
            cultureInfo);
        }

        var foregroundCanvas = CreateLayerCanvas(layout.OriginalPageSize, layout.OriginalPageSize);
        using (StartLayerActivity("foreground"))
        {
        RenderSequentialControls(
            foregroundCanvas,
            template.ForegroundControls,
            layout.ForegroundSizes,
            options.DotsPerInch,
            layout.ForegroundPageSize,
            cultureInfo);
        }

        var bodyPageCount = Math.Max((ushort)Math.Ceiling(desiredBodyHeight / layout.BodyPageSize.Height), (ushort)1);
        var pageCount = GetPageCountIncludingAreas(template, options, layout.OriginalPageSize, bodyPageCount);
        var pages = new List<PapercraftPage>(pageCount);
        var currentHeight = 0F;
        for (var i = 0; i < pageCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var displayCanvas = new DisplayCanvasImpl
            {
                PageNumber = (ushort)(i + 1),
                TotalPages = pageCount,
            };

            using (displayCanvas.CreateState())
            {
                using (displayCanvas.CreateState())
                {
                    displayCanvas.Clip(0, 0, layout.BackgroundPageSize.Width, layout.BackgroundPageSize.Height);
                    backgroundCanvas.Render(displayCanvas);
                }

                using (displayCanvas.CreateState())
                {
                    displayCanvas.Translate(layout.Margin.X, layout.Margin.Y);
                    using (displayCanvas.CreateState())
                    {
                        displayCanvas.Clip(0, 0, layout.HeaderPageSize.Width, layout.HeaderPageSize.Height);
                        headerCanvas.Render(displayCanvas);
                    }

                    using (displayCanvas.CreateState())
                    {
                        displayCanvas.Translate(0, layout.HeaderPageSize.Height);
                        displayCanvas.Clip(0, 0, layout.BodyPageSize.Width, layout.BodyPageSize.Height);
                        displayCanvas.Translate(0, -currentHeight);
                        bodyCanvas.Render(displayCanvas);
                    }

                    using (displayCanvas.CreateState())
                    {
                        displayCanvas.Translate(0, layout.HeaderPageSize.Height);
                        displayCanvas.Translate(0, layout.BodyPageSize.Height);
                        displayCanvas.Clip(0, 0, layout.FooterPageSize.Width, layout.FooterPageSize.Height);
                        footerCanvas.Render(displayCanvas);
                    }
                }

                using (displayCanvas.CreateState())
                {
                    var areaCanvas = CreateLayerCanvas(layout.OriginalPageSize, layout.OriginalPageSize);
                    RenderAreasForPage(
                        areaCanvas,
                        template,
                        layout.AreaSizes,
                        options,
                        layout.OriginalPageSize,
                        i,
                        cultureInfo);
                    areaCanvas.Render(displayCanvas);
                }

                using (displayCanvas.CreateState())
                {
                    displayCanvas.Clip(0, 0, layout.ForegroundPageSize.Width, layout.ForegroundPageSize.Height);
                    foregroundCanvas.Render(displayCanvas);
                }
            }

            pages.Add(
                new PapercraftPage(
                    i,
                    i + 1,
                    pageCount,
                    layout.OriginalPageSize,
                    options.DotsPerMillimeter,
                    displayCanvas.DisplayList));
            currentHeight += layout.BodyPageSize.Height;
        }

        activity?.SetTag(PapercraftActivity.DocumentPageCountTag, pages.Count);
        return pages;
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private static DeferredCanvasImpl CreateLayerCanvas(Size actualPageSize, Size pageSize)
        => new() { ActualPageSize = actualPageSize, PageSize = pageSize };

    private static ushort GetPageCountIncludingAreas(
        Template template,
        DocumentOptions options,
        Size pageSize,
        ushort minimumPageCount)
    {
        var pageCount = (int)minimumPageCount;
        if (pageSize.Height <= 0)
            return minimumPageCount;

        foreach (var area in template.AreaControls)
        {
            var tuple = area.CalculateClippingAndTranslationData(options.DotsPerInch, pageSize);
            var areaPageIndex = GetAreaPageIndex(tuple.top, pageSize.Height);
            if (areaPageIndex >= 0)
                pageCount = Math.Max(pageCount, areaPageIndex + 1);
        }

        if (pageCount > ushort.MaxValue)
            throw new InvalidOperationException("The generated document exceeds the maximum supported page count.");
        return (ushort)pageCount;
    }

    private static void RenderAreasForPage(
        IDeferredCanvas areaCanvas,
        Template template,
        IReadOnlyList<(int areaIndex, Size size)> areaSizes,
        DocumentOptions options,
        Size pageSize,
        int pageIndex,
        CultureInfo cultureInfo)
    {
        using (StartLayerActivity("area"))
        {
            using (areaCanvas.CreateState())
            {
                var absolutePageTop = pageIndex * pageSize.Height;
                for (var areaIndex = 0; areaIndex < template.AreaControls.Count; areaIndex++)
                {
                    var area = template.AreaControls.ElementAt(areaIndex);
                    var tuple = area.CalculateClippingAndTranslationData(options.DotsPerInch, pageSize);
                    if (GetAreaPageIndex(tuple.top, pageSize.Height) != pageIndex)
                        continue;

                    using (areaCanvas.CreateState())
                    {
                        areaCanvas.Translate(tuple.left, tuple.top - absolutePageTop);
                        areaCanvas.Clip(0, 0, tuple.width, tuple.height);
                        var localAreaCanvas = new RelativeDeferredCanvas(areaCanvas, areaCanvas.Translation, tuple.size);
                        foreach (var (control, (_, size)) in area.Controls.Zip(areaSizes.Where((q) => q.areaIndex == areaIndex)))
                        {
                            _ = control.Render(localAreaCanvas, options.DotsPerInch, tuple.size, cultureInfo);
                            localAreaCanvas.Translate(0F, size.Height);
                        }
                    }
                }
            }
        }
    }

    private static int GetAreaPageIndex(float absoluteTop, float pageHeight)
    {
        if (pageHeight <= 0 || float.IsNaN(absoluteTop) || float.IsInfinity(absoluteTop))
            return -1;
        return (int)Math.Floor(absoluteTop / pageHeight);
    }

    private static IReadOnlyList<Size> ArrangeControls(
        IEnumerable<IControl> controls,
        float dpi,
        Size fullPageSize,
        Size framedPageSize,
        Size remainingSize,
        CultureInfo cultureInfo)
        => controls
            .Select((q) => q.Arrange(dpi, fullPageSize, framedPageSize, remainingSize, cultureInfo))
            .ToArray();

    private static void RenderSequentialControls(
        IDeferredCanvas canvas,
        IEnumerable<IControl> controls,
        IReadOnlyList<Size> sizes,
        float dpi,
        Size pageSize,
        CultureInfo cultureInfo)
    {
        using (canvas.CreateState())
        {
            foreach (var (control, size) in controls.Zip(sizes))
            {
                _ = control.Render(canvas, dpi, pageSize, cultureInfo);
                canvas.Translate(0F, size.Height);
            }
        }
    }

    private static Activity? StartLayerActivity(string layer)
    {
        var activity = PapercraftActivity.Start(PapercraftActivityNames.GeneratorComposeLayer);
        activity?.SetTag(PapercraftActivity.LayerTag, layer);
        return activity;
    }

    private static Size GetOriginalPageSize(DocumentOptions options)
        => new(
            options.DotsPerMillimeter * options.PageWidthInMillimeters,
            options.DotsPerMillimeter * options.PageHeightInMillimeters);

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var (_, value) in _data)
        {
            if (value is IDisposable disposable)
                disposable.Dispose();
        }

        _data.Clear();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (var (_, value) in _data)
        {
            switch (value)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync()
                        .ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        _data.Clear();
    }

    private sealed record RenderLayout(
        Size OriginalPageSize,
        Size PageSize,
        Point Margin,
        Size BackgroundPageSize,
        IReadOnlyList<Size> BackgroundSizes,
        Size HeaderPageSize,
        IReadOnlyList<Size> HeaderSizes,
        Size BodyPageSize,
        IReadOnlyList<Size> BodySizes,
        Size FooterPageSize,
        IReadOnlyList<Size> FooterSizes,
        Size ForegroundPageSize,
        IReadOnlyList<Size> ForegroundSizes,
        IReadOnlyList<(int areaIndex, Size size)> AreaSizes);
}
