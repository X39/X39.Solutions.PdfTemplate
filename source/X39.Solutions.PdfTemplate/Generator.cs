using System.Xml;
using SkiaSharp;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Functions;
using X39.Solutions.PdfTemplate.Services;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Generator for PDF files
/// </summary>
/// <remarks>
/// This class is not thread safe. Make sure to implement locking if you want to use it in a multi-threaded environment.
/// </remarks>
[PublicAPI]
public sealed class Generator : IDisposable, IAsyncDisposable, IAddControls, IAddTransformers
{
    /// <summary>
    /// The data available to the templates processed by this generator.
    /// </summary>
    public ITemplateData TemplateData { get; }

    private readonly SkPaintCache               _skPaintCache;
    private readonly ControlStorage             _controlStorage;
    private readonly Dictionary<string, object> _data         = new();
    private readonly List<ITransformer>         _transformers = new();

    /// <summary>
    /// Creates a new instance of the <see cref="Generator"/> class.
    /// </summary>
    /// <param name="skPaintCache">The paint cache to use.</param>
    /// <param name="controlExpressionCache">The control expression cache to use.</param>
    /// <param name="functions">The functions to register.</param>
    public Generator(
        SkPaintCache skPaintCache,
        ControlExpressionCache controlExpressionCache,
        IEnumerable<IFunction> functions)
    {
        TemplateData = new TemplateData();
        TemplateData.RegisterFunction(new AllTemplateDataFunctions(TemplateData));
        TemplateData.RegisterFunction(new AllTemplateDataVariables(TemplateData));
        foreach (var function in functions)
        {
            TemplateData.RegisterFunction(function);
        }

        _skPaintCache   = skPaintCache;
        _controlStorage = new(controlExpressionCache);
    }


    /// <summary>
    /// Adds a control to the generator, making it available for use in templates.
    /// </summary>
    /// <typeparam name="TControl">The type of the control to add.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown when the type does not have a <see cref="ControlAttribute"/>.</exception>
    public void AddControl<
        [MeansImplicitUse(
            ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign)]
        TControl>()
        where TControl : IControl => _controlStorage.AddControl<TControl>();

    /// <summary>
    /// Adds a transformer to the generator, making it available for use in templates.
    /// </summary>
    /// <param name="transformer"></param>
    public void AddTransformer(ITransformer transformer)
    {
        _transformers.Add(transformer);
    }

    /// <summary>
    /// Adds data to the generator, making it available for use in templates.
    /// </summary>
    /// <param name="key">The key to store the data under.</param>
    /// <param name="data">The data to store.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is <see langword="null"/>.</exception>
    public void AddData(string key, object data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _data.Add(key, data);
    }

    /// <summary>
    /// Generates a PDF document from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="outputStream">The stream to write the PDF document to.</param>
    /// <param name="reader">The reader to read the template from.</param>
    /// <param name="cultureInfo">The culture to use for the generation.</param>
    /// <param name="documentOptions">The options for the document.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}"/> that will complete when the generation has finished,
    ///     returning the <see cref="SKBitmap"/>'s.
    /// </returns>
    public async Task GeneratePdfAsync(
        Stream outputStream,
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions? documentOptions = default,
        CancellationToken cancellationToken = default)
    {
        var options = documentOptions ?? DocumentOptions.Default;
        var pageSize = new Size(
            options.DotsPerMillimeter * options.PageWidthInMillimeters,
            options.DotsPerMillimeter * options.PageHeightInMillimeters);
        using var skDocument = SKDocument.CreatePdf(
            outputStream,
            new SKDocumentPdfMetadata
            {
                RasterDpi = options.DotsPerInch,
                Producer  = options.Producer,
                Modified  = options.Modified,
                PdfA      = true,
            });
        var hasOpenPage = false;
        await GenerateAsync(
                () =>
                {
                    hasOpenPage = true;
                    return skDocument.BeginPage(pageSize.Width, pageSize.Height);
                },
                reader,
                cultureInfo,
                options,
                cancellationToken)
            .ConfigureAwait(false);
        if (!hasOpenPage) skDocument.BeginPage(pageSize.Width, pageSize.Height).Dispose();
        skDocument.EndPage();
        skDocument.Close();
    }


    /// <summary>
    /// Generates <see cref="SKBitmap"/>'s from the given <paramref name="reader"/>.
    /// </summary>
    /// <remarks>
    /// The caller is responsible for disposing the returned <see cref="SKBitmap"/>'s.
    /// </remarks>
    /// <param name="reader">The reader to read the template from.</param>
    /// <param name="cultureInfo">The culture to use for the generation.</param>
    /// <param name="documentOptions">The options for the document.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
    /// <returns>
    ///     A <see cref="Task{TResult}"/> that will complete when the generation has finished,
    ///     returning the <see cref="SKBitmap"/>'s.
    /// </returns>
    public async Task<IReadOnlyCollection<SKBitmap>> GenerateBitmapsAsync(
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions? documentOptions = default,
        CancellationToken cancellationToken = default)
    {
        var options = documentOptions ?? DocumentOptions.Default;
        var pageSize = new Size(
            options.DotsPerMillimeter * options.PageWidthInMillimeters,
            options.DotsPerMillimeter * options.PageHeightInMillimeters);
        var bitmaps = new List<SKBitmap>();
        try
        {
            SKCanvas? canvas = null;
            await GenerateAsync(
                    () =>
                    {
                        var bitmap = new SKBitmap(
                            (int) Math.Ceiling(pageSize.Width),
                            (int) Math.Ceiling(pageSize.Height));
                        bitmaps.Add(bitmap);
                        canvas = new SKCanvas(bitmap);
                        canvas.Clear(SKColors.White);
                        return canvas;
                    },
                    reader,
                    cultureInfo,
                    options,
                    cancellationToken)
                .ConfigureAwait(false);
            canvas?.Dispose();
            return bitmaps.AsReadOnly();
        }
        catch
        {
            bitmaps.ForEach((q) => q.Dispose());
            throw;
        }
    }

    private async Task GenerateAsync(
        [InstantHandle] Func<SKCanvas> nextCanvas,
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions? documentOptions = default,
        CancellationToken cancellationToken = default)
    {
        using var templateDataScope = TemplateData.Scope("Document");
        var options = documentOptions ?? DocumentOptions.Default;
        XmlNodeInformation rootNode;
        using (var templateReader = new XmlTemplateReader(cultureInfo, TemplateData, _transformers))
            rootNode = await templateReader.ReadAsync(reader, cancellationToken)
                .ConfigureAwait(false);

        await using var template = await Template.CreateAsync(rootNode, _controlStorage, cultureInfo, cancellationToken)
            .ConfigureAwait(false);
        var originalPageSize = new Size(
            options.DotsPerMillimeter * options.PageWidthInMillimeters,
            options.DotsPerMillimeter * options.PageHeightInMillimeters);
        var marginLeft = options.Margin.Left.ToPixels(originalPageSize.Width, options.DotsPerInch);
        var marginTop = options.Margin.Top.ToPixels(originalPageSize.Height, options.DotsPerInch);
        var pageSize = new Size(
            originalPageSize.Width
            - marginLeft
            - options.Margin.Right.ToPixels(originalPageSize.Width, options.DotsPerInch),
            originalPageSize.Height
            - marginTop
            - options.Margin.Bottom.ToPixels(originalPageSize.Height, options.DotsPerInch)
        );

        #region Measure

        foreach (var control in Enumerable.Empty<IControl>()
                                          .Concat(template.BackgroundControls)
                                          .Concat(template.HeaderControls)
                                          .Concat(template.BodyControls)
                                          .Concat(template.FooterControls))
        {
            control.Measure(options.DotsPerInch, pageSize, pageSize, pageSize, cultureInfo);
        }

        #endregion

        #region Arrange

        var backgroundSizes = new List<Size>();
        var backgroundPageSize = originalPageSize with {Height = originalPageSize.Height * 0.25F};
        foreach (var control in template.BackgroundControls)
        {
            var size = control.Arrange(options.DotsPerInch, originalPageSize, backgroundPageSize, backgroundPageSize, cultureInfo);
            backgroundSizes.Add(size);
        }

        backgroundPageSize = backgroundPageSize with {Height = backgroundSizes.Sum((q) => q.Height)};
        if (backgroundPageSize.Height > originalPageSize.Height)
            backgroundPageSize = backgroundPageSize with {Height = originalPageSize.Height};

        var headerSizes = new List<Size>();
        var headerPageSize = pageSize with {Height = pageSize.Height * 0.25F};
        foreach (var control in template.HeaderControls)
        {
            var size = control.Arrange(options.DotsPerInch, pageSize, headerPageSize, headerPageSize, cultureInfo);
            headerSizes.Add(size);
        }

        headerPageSize = headerPageSize with {Height = headerSizes.Sum((q) => q.Height)};
        if (headerPageSize.Height > pageSize.Height * 0.25F)
            headerPageSize = headerPageSize with {Height = pageSize.Height * 0.25F};

        var footerSizes = new List<Size>();
        var footerPageSize = pageSize with {Height = pageSize.Height * 0.25F};
        foreach (var control in template.FooterControls)
        {
            var size = control.Arrange(options.DotsPerInch, pageSize, footerPageSize, footerPageSize, cultureInfo);
            footerSizes.Add(size);
        }

        footerPageSize = footerPageSize with {Height = footerSizes.Sum((q) => q.Height)};
        if (footerPageSize.Height > pageSize.Height * 0.25F)
            footerPageSize = footerPageSize with {Height = pageSize.Height * 0.25F};


        var bodySizes = new List<Size>();
        var bodyPageSize = pageSize with {Height = pageSize.Height - headerPageSize.Height - footerPageSize.Height};
        foreach (var control in template.BodyControls)
        {
            var size = control.Arrange(options.DotsPerInch, pageSize, bodyPageSize, bodyPageSize, cultureInfo);
            bodySizes.Add(size);
        }

        #endregion

        #region Render

        var backgroundCanvasAbstraction = new DeferredCanvasImpl();
        using (backgroundCanvasAbstraction.CreateState())
        {
            foreach (var (control, size) in template.BackgroundControls.Zip(backgroundSizes))
            {
                _ = control.Render(backgroundCanvasAbstraction, options.DotsPerInch, backgroundPageSize, cultureInfo);
                backgroundCanvasAbstraction.Translate(0F, size.Height);
            }
        }

        var headerCanvasAbstraction = new DeferredCanvasImpl();
        using (headerCanvasAbstraction.CreateState())
        {
            foreach (var (control, size) in template.HeaderControls.Zip(headerSizes))
            {
                _ = control.Render(headerCanvasAbstraction, options.DotsPerInch, headerPageSize, cultureInfo);
                headerCanvasAbstraction.Translate(0F, size.Height);
            }
        }

        var bodyCanvasAbstraction = new DeferredCanvasImpl();
        var desiredBodyHeight = bodySizes.Sum((q) => q.Height);
        using (bodyCanvasAbstraction.CreateState())
        {
            foreach (var (control, size) in template.BodyControls.Zip(bodySizes))
            {
                var (_, additionalHeight) = control.Render(bodyCanvasAbstraction, options.DotsPerInch, bodyPageSize, cultureInfo);
                desiredBodyHeight += additionalHeight;
                bodyCanvasAbstraction.Translate(0F, size.Height + additionalHeight);
            }
        }
        var pageCount = (ushort) Math.Ceiling(desiredBodyHeight / bodyPageSize.Height);

        var footerCanvasAbstraction = new DeferredCanvasImpl();
        using (footerCanvasAbstraction.CreateState())
        {
            foreach (var (control, size) in template.FooterControls.Zip(footerSizes))
            {
                _ = control.Render(footerCanvasAbstraction, options.DotsPerInch, footerPageSize, cultureInfo);
                footerCanvasAbstraction.Translate(0F, size.Height);
            }
        }

        var currentHeight = 0F;

        for (var i = 0; i < pageCount; i++)
        {
            using var canvas = nextCanvas();
            var immediateCanvas = new ImmediateCanvasImpl(canvas, _skPaintCache)
            {
                PageNumber = (ushort) (i + 1), TotalPages = pageCount,
            };
            canvas.Save();
            canvas.Save();
            canvas.ClipRect(
                new SKRect { Left = 0, Right = backgroundPageSize.Width, Top = 0, Bottom = backgroundPageSize.Height }
            );
            backgroundCanvasAbstraction.Render(immediateCanvas);
            canvas.Restore();


            canvas.Translate(marginLeft, marginTop);

            canvas.Save();
            canvas.ClipRect(
                new SKRect { Left = 0, Right = headerPageSize.Width, Top = 0, Bottom = headerPageSize.Height }
            );
            headerCanvasAbstraction.Render(immediateCanvas);
            canvas.Restore();

            canvas.Save();
            canvas.Translate(0, headerPageSize.Height);
            canvas.ClipRect(new SKRect { Left = 0, Right = bodyPageSize.Width, Top = 0, Bottom = bodyPageSize.Height });
            canvas.Translate(0, -currentHeight);
            bodyCanvasAbstraction.Render(immediateCanvas);
            canvas.Restore();

            canvas.Save();
            canvas.Translate(0, headerPageSize.Height);
            canvas.Translate(0, bodyPageSize.Height);
            canvas.ClipRect(
                new SKRect { Left = 0, Right = footerPageSize.Width, Top = 0, Bottom = footerPageSize.Height }
            );
            footerCanvasAbstraction.Render(immediateCanvas);
            canvas.Restore();
            currentHeight += bodyPageSize.Height;

            canvas.Restore();
        }

        #endregion
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var (_, value) in _data)
        {
            switch (value)
            {
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        _data.Clear();
        _controlStorage.Clear();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (var (_, value) in _data)
        {
            switch (value)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync();
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        _data.Clear();
        _controlStorage.Clear();
    }
}