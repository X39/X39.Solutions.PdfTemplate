using System.Globalization;
using System.Reflection;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services;
using X39.Solutions.PdfTemplate.Xml;
using X39.Util.Collections;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Generator for PDF files
/// </summary>
/// <remarks>
/// This class is not thread safe. Make sure to implement locking if you want to use it in a multi-threaded environment.
/// </remarks>
public sealed class Generator : IDisposable, IAsyncDisposable
{
    private readonly SkPaintCache   _skPaintCache;
    private readonly ControlStorage _controlStorage;

    public Generator(SkPaintCache skPaintCache, ControlExpressionCache controlExpressionCache)
    {
        _skPaintCache = skPaintCache;
        _controlStorage    = new(controlExpressionCache);
    }

    private readonly Dictionary<string, object> _data = new();


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
        var options = documentOptions?? DocumentOptions.Default;
        var templateReader = new XmlTemplateReader();
        var rootNode = templateReader.Read(reader);
        var template = Template.Create(rootNode, _controlStorage, cultureInfo);
        using var skDocument = SkiaSharp.SKDocument.CreatePdf(outputStream, options.DotsPerInch);
        // ToDo: Measure elements to estimate the number of pages
        var pageSize = new Size(
            options.DotsPerMillimeter * options.PageWidthInMillimeters,
            options.DotsPerMillimeter * options.PageHeightInMillimeters);
        var desiredHeight = 0F;
        foreach (var control in template.HeaderControls.Concat(template.BodyControls).Concat(template.FooterControls))
        {
            var size = control.Measure(pageSize, cultureInfo);
            desiredHeight += size.Height;
        }
        
        // ToDo: Implement page splitting
        var pageCount = (int) Math.Ceiling(desiredHeight / pageSize.Height);
        foreach (var control in template.HeaderControls.Concat(template.BodyControls).Concat(template.FooterControls))
        {
            var size = control.Arrange(pageSize, cultureInfo);
        }

        // ToDo: Generate pages
        using var canvas = skDocument.BeginPage(pageSize.Width, pageSize.Height);
        var canvasAbstraction = new CanvasImpl(_skPaintCache, canvas);
        foreach (var control in template.HeaderControls.Concat(template.BodyControls).Concat(template.FooterControls))
        {
            control.Render(canvasAbstraction, pageSize, cultureInfo);
        }
        canvas.Flush();
        skDocument.EndPage();
        skDocument.Close();
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