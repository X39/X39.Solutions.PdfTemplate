﻿using System.Globalization;
using System.Reflection;
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
public sealed class Generator : IDisposable, IAsyncDisposable
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
        where TControl : IControl
    {
        var type = typeof(TControl);

        if (type.IsGenericType)
            throw new InvalidOperationException(
                $"The type {type.FullName} is a generic type and cannot be used as a control.");
        var attribute = typeof(TControl).GetCustomAttribute<ControlAttribute>();
        if (attribute is null)
            throw new InvalidOperationException(
                $"The type {typeof(TControl).FullName} does not have a {nameof(ControlAttribute)}.");
        var name = attribute.Name;
        if (name.IsNullOrEmpty())
        {
            const string controlSuffix = "Control";
            name = typeof(TControl).Name();

            if (name.EndsWith(controlSuffix, StringComparison.Ordinal))
                name = name[..^controlSuffix.Length];
        }

        _controlStorage.Add(
            attribute.Namespace,
            name,
            typeof(TControl));
    }

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
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        _data.Add(key, data);
    }

    /// <summary>
    /// Generates a PDF document from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="outputStream">The stream to write the PDF document to.</param>
    /// <param name="reader">The reader to read the template from.</param>
    /// <param name="cultureInfo">The culture to use for the generation.</param>
    /// <param name="documentOptions">The options for the document.</param>
    public void GeneratePdf(
        Stream outputStream,
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions? documentOptions = default)
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
        Generate(
            () =>
            {
                hasOpenPage = true;
                return skDocument.BeginPage(pageSize.Width, pageSize.Height);
            },
            reader,
            cultureInfo,
            options);
        if (!hasOpenPage) skDocument.BeginPage(pageSize.Width, pageSize.Height).Dispose();
        skDocument.EndPage();
        skDocument.Close();
    }


    /// <summary>
    /// Generates <see cref="SKBitmap"/>'s from the given <paramref name="reader"/>.
    /// </summary>
    /// <param name="reader">The reader to read the template from.</param>
    /// <param name="cultureInfo">The culture to use for the generation.</param>
    /// <param name="documentOptions">The options for the document.</param>
    /// <returns>The generated <see cref="SKBitmap"/>'s.</returns>
    public IReadOnlyCollection<SKBitmap> GenerateBitmaps(
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions? documentOptions = default)
    {
        var options = documentOptions ?? DocumentOptions.Default;
        var pageSize = new Size(
            options.DotsPerMillimeter * options.PageWidthInMillimeters,
            options.DotsPerMillimeter * options.PageHeightInMillimeters);
        var bitmaps = new List<SKBitmap>();
        try
        {
            SKCanvas? canvas = null;
            Generate(
                () =>
                {
                    var bitmap = new SKBitmap((int) Math.Ceiling(pageSize.Width), (int) Math.Ceiling(pageSize.Height));
                    bitmaps.Add(bitmap);
                    canvas = new SKCanvas(bitmap);
                    return canvas;
                },
                reader,
                cultureInfo,
                options);
            canvas?.Dispose();
            return bitmaps;
        }
        catch
        {
            bitmaps.ForEach((q) => q.Dispose());
            throw;
        }
    }

    private void Generate(
        [InstantHandle] Func<SKCanvas> nextCanvas,
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions? documentOptions = default)
    {
        using var templateDataScope = TemplateData.Scope("Document");
        var options = documentOptions ?? DocumentOptions.Default;
        XmlNodeInformation rootNode;
        using (var templateReader = new XmlTemplateReader(TemplateData, _transformers))
            rootNode = templateReader.Read(reader);

        var template = Template.Create(rootNode, _controlStorage, cultureInfo);
        var pageSize = new Size(
            options.DotsPerMillimeter * options.PageWidthInMillimeters,
            options.DotsPerMillimeter * options.PageHeightInMillimeters);

        #region Measure

        foreach (var control in template.HeaderControls.Concat(template.BodyControls).Concat(template.FooterControls))
        {
            control.Measure(pageSize, pageSize, pageSize, cultureInfo);
        }

        #endregion

        #region Arrange

        var headerSizes = new List<Size>();
        var headerPageSize = pageSize with {Height = pageSize.Height * 0.25F};
        foreach (var control in template.HeaderControls)
        {
            var size = control.Arrange(pageSize, headerPageSize, headerPageSize, cultureInfo);
            headerSizes.Add(size);
        }

        headerPageSize = headerPageSize with {Height = headerSizes.Sum((q) => q.Height)};
        if (headerPageSize.Height > pageSize.Height * 0.25F)
            headerPageSize = headerPageSize with {Height = pageSize.Height * 0.25F};

        var footerSizes = new List<Size>();
        var footerPageSize = pageSize with {Height = pageSize.Height * 0.25F};
        foreach (var control in template.FooterControls)
        {
            var size = control.Arrange(pageSize, footerPageSize, footerPageSize, cultureInfo);
            footerSizes.Add(size);
        }

        footerPageSize = footerPageSize with {Height = footerSizes.Sum((q) => q.Height)};
        if (footerPageSize.Height > pageSize.Height * 0.25F)
            footerPageSize = footerPageSize with {Height = pageSize.Height * 0.25F};


        var bodySizes = new List<Size>();
        var bodyPageSize = pageSize with {Height = pageSize.Height - headerPageSize.Height - footerPageSize.Height};
        foreach (var control in template.BodyControls)
        {
            var size = control.Arrange(pageSize, bodyPageSize, bodyPageSize, cultureInfo);
            bodySizes.Add(size);
        }

        #endregion

        #region Render

        var headerCanvasAbstraction = new CanvasImpl(_skPaintCache);
        headerCanvasAbstraction.PushState();
        foreach (var (control, size) in template.HeaderControls.Zip(headerSizes))
        {
            control.Render(headerCanvasAbstraction, headerPageSize, cultureInfo);
            headerCanvasAbstraction.Translate(0F, size.Height);
        }

        headerCanvasAbstraction.PopState();

        var bodyCanvasAbstraction = new CanvasImpl(_skPaintCache);
        bodyCanvasAbstraction.PushState();
        foreach (var (control, size) in template.BodyControls.Zip(bodySizes))
        {
            control.Render(bodyCanvasAbstraction, bodyPageSize, cultureInfo);
            bodyCanvasAbstraction.Translate(0F, size.Height);
        }

        bodyCanvasAbstraction.PopState();

        var footerCanvasAbstraction = new CanvasImpl(_skPaintCache);
        footerCanvasAbstraction.PushState();
        foreach (var (control, size) in template.FooterControls.Zip(footerSizes))
        {
            control.Render(footerCanvasAbstraction, footerPageSize, cultureInfo);
            footerCanvasAbstraction.Translate(0F, size.Height);
        }

        footerCanvasAbstraction.PopState();

        var desiredHeight = headerSizes.Sum((q) => q.Height) + bodySizes.Sum((q) => q.Height) +
                            footerSizes.Sum((q) => q.Height);
        var pageCount = (int) Math.Ceiling(desiredHeight / pageSize.Height);

        var currentHeight = 0F;

        for (var i = 0; i < pageCount; i++)
        {
            using var canvas = nextCanvas();

            canvas.Save();
            canvas.ClipRect(
                new SKRect {Left = 0, Right = headerPageSize.Width, Top = 0, Bottom = headerPageSize.Height});
            headerCanvasAbstraction.Render(canvas);
            canvas.Restore();

            canvas.Save();
            canvas.Translate(0, headerPageSize.Height);
            canvas.ClipRect(new SKRect {Left = 0, Right = bodyPageSize.Width, Top = 0, Bottom = bodyPageSize.Height});
            canvas.Translate(0, -currentHeight);
            bodyCanvasAbstraction.Render(canvas);
            canvas.Restore();

            canvas.Save();
            canvas.Translate(0, headerPageSize.Height);
            canvas.Translate(0, bodyPageSize.Height);
            canvas.ClipRect(
                new SKRect {Left = 0, Right = footerPageSize.Width, Top = 0, Bottom = footerPageSize.Height});
            footerCanvasAbstraction.Render(canvas);
            canvas.Restore();
            currentHeight += bodyPageSize.Height;
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