using System.Globalization;
using System.Reflection;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Functions;
using X39.Solutions.PdfTemplate.Services;
using X39.Solutions.PdfTemplate.Transformers;
using X39.Solutions.PdfTemplate.Xml;
using X39.Util.Collections;

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

    public void AddTransfromer(ITransformer transformer)
    {
        _transformers.Add(transformer);
    }

    public void AddData(string key, object data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        _data.Add(key, data);
    }

    public void Generate(
        Stream outputStream,
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
        using var skDocument = SkiaSharp.SKDocument.CreatePdf(
            outputStream,
            new SkiaSharp.SKDocumentPdfMetadata
            {
                RasterDpi = options.DotsPerInch,
                Producer  = options.Producer,
                Modified  = options.Modified,
                PdfA      = true,
            });
        // ToDo: Measure elements to estimate the number of pages
        var pageSize = new Size(
            options.DotsPerMillimeter * options.PageWidthInMillimeters,
            options.DotsPerMillimeter * options.PageHeightInMillimeters);

        #region Measure


        foreach (var control in template.HeaderControls.Concat(template.BodyControls).Concat(template.FooterControls))
        {
            var size = control.Measure(pageSize, pageSize, pageSize, cultureInfo);
        }

        #endregion

        #region Arrange

        var headerSizes = new List<Size>();
        var headerPageSize = pageSize with { Height = pageSize.Height * 0.25F };
        foreach (var control in template.HeaderControls)
        {
            var size = control.Arrange(pageSize, headerPageSize, headerPageSize, cultureInfo);
            headerSizes.Add(size);
        }
        headerPageSize = headerPageSize with { Height = headerSizes.Sum((q)=>q.Height) };
        if (headerPageSize.Height > pageSize.Height * 0.25F)
            headerPageSize = headerPageSize with { Height = pageSize.Height * 0.25F };

        var footerSizes = new List<Size>();
        var footerPageSize = pageSize with { Height = pageSize.Height * 0.25F };
        foreach (var control in template.FooterControls)
        {
            var size = control.Arrange(pageSize, footerPageSize, footerPageSize, cultureInfo);
            footerSizes.Add(size);
        }
        footerPageSize = footerPageSize with { Height = footerSizes.Sum((q)=>q.Height) };
        if (footerPageSize.Height > pageSize.Height * 0.25F)
            footerPageSize = footerPageSize with { Height = pageSize.Height * 0.25F };
        

        var bodySizes = new List<Size>();
        var bodyPageSize = pageSize with { Height = pageSize.Height - headerPageSize.Height - footerPageSize.Height };
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

        var desiredHeight = headerSizes.Sum((q)=>q.Height) + bodySizes.Sum((q)=>q.Height) + footerSizes.Sum((q)=>q.Height);
        var pageCount = (int) Math.Ceiling(desiredHeight / pageSize.Height);

        var currentHeight = 0F;

        for (var i = 0; i < pageCount; i++)
        {
            using var canvas = skDocument.BeginPage(pageSize.Width, pageSize.Height);
            
            canvas.Save();
            canvas.ClipRect(new SKRect{ Left = 0, Right = headerPageSize.Width, Top = 0, Bottom = headerPageSize.Height });
            headerCanvasAbstraction.Render(canvas);
            canvas.Restore();
            
            canvas.Save();
            canvas.Translate(0, headerPageSize.Height);
            canvas.ClipRect(new SKRect{ Left = 0, Right = bodyPageSize.Width, Top = 0, Bottom = bodyPageSize.Height });
            canvas.Translate(0, -currentHeight);
            bodyCanvasAbstraction.Render(canvas);
            canvas.Restore();
            
            canvas.Save();
            canvas.Translate(0, headerPageSize.Height);
            canvas.Translate(0, bodyPageSize.Height);
            canvas.ClipRect(new SKRect{ Left = 0, Right = footerPageSize.Width, Top = 0, Bottom = footerPageSize.Height });
            footerCanvasAbstraction.Render(canvas);
            canvas.Restore();
            
            skDocument.EndPage();
            currentHeight += bodyPageSize.Height;
        }

        skDocument.Close();

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