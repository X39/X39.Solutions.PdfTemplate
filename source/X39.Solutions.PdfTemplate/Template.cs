using System.Diagnostics.CodeAnalysis;
using System.Xml;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Exceptions;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate;

internal sealed class Template : IAsyncDisposable
{
    internal record struct Area(
        IReadOnlyCollection<IControl> Controls,
        Length? Left,
        Length? Top,
        Length? Right,
        Length? Bottom,
        Length? Width,
        Length? Height
    )
    {
        public (float width, float height, float left, float top, Size size) CalculateClippingAndTranslationData(
            float dpi,
            Size pageSize
        )
        {
            var leftValue = Left?.ToPixels(pageSize.Width, dpi) ?? 0F;
            var rightValue = pageSize.Width - Right?.ToPixels(pageSize.Width, dpi) ?? 0F;
            var topValue = Top?.ToPixels(pageSize.Height, dpi) ?? 0F;
            var bottomValue = pageSize.Height - Bottom?.ToPixels(pageSize.Height, dpi) ?? 0F;
            float width, height, left, top;
            if (Left is not null && Right is not null)
                width = rightValue - leftValue;
            else
                width = Width?.ToPixels(pageSize.Width, dpi) ?? 0F;
            if (Top is not null && Bottom is not null)
                height = bottomValue - topValue;
            else
                height = Height?.ToPixels(pageSize.Height, dpi) ?? 0F;
            if (Left is not null)
                left = leftValue;
            else if (Right is not null)
                left = rightValue - width;
            else
                left = 0F;
            if (Top is not null)
                top = topValue;
            else if (Bottom is not null)
                top = bottomValue - height;
            else
                top = 0F;
            var size = new Size(width, height);
            return (width, height, left, top, size);
        }
    }

    private Template(
        IReadOnlyCollection<IControl> backgroundControls,
        IReadOnlyCollection<IControl> headerControls,
        IReadOnlyCollection<IControl> bodyControls,
        IReadOnlyCollection<IControl> footerControls,
        IReadOnlyCollection<IControl> foregroundControls,
        IReadOnlyCollection<Area> areaControls
    )
    {
        BackgroundControls = backgroundControls;
        HeaderControls     = headerControls;
        BodyControls       = bodyControls;
        FooterControls     = footerControls;
        ForegroundControls = foregroundControls;
        AreaControls       = areaControls;
    }

    public IReadOnlyCollection<IControl> BackgroundControls { get; }
    public IReadOnlyCollection<IControl> HeaderControls { get; }
    public IReadOnlyCollection<IControl> BodyControls { get; }
    public IReadOnlyCollection<IControl> FooterControls { get; }
    public IReadOnlyCollection<IControl> ForegroundControls { get; }

    public IReadOnlyCollection<Area> AreaControls { get; }


    [SuppressMessage("ReSharper", "InvertIf")]
    public static async Task<Template> CreateAsync(
        XmlNodeInformation rootNode,
        ControlStorage cache,
        CultureInfo cultureInfo,
        CancellationToken cancellationToken
    )
    {
        var headerControls = new List<IControl>();
        var bodyControls = new List<IControl>();
        var footerControls = new List<IControl>();
        var backgroundControls = new List<IControl>();
        var foregroundControls = new List<IControl>();
        var areaControls = new List<Area>();
        if (rootNode["header", rootNode.NodeNamespace] is { } headerNode)
        {
            foreach (var node in headerNode.Children)
            {
                var control = await CreateControlAsync(node, cache, cultureInfo, cancellationToken)
                    .ConfigureAwait(false);
                headerControls.Add(control);
            }
        }

        if (rootNode["body", rootNode.NodeNamespace] is { } bodyNode)
        {
            foreach (var node in bodyNode.Children)
            {
                var control = await CreateControlAsync(node, cache, cultureInfo, cancellationToken)
                    .ConfigureAwait(false);
                bodyControls.Add(control);
            }
        }

        if (rootNode["footer", rootNode.NodeNamespace] is { } footerNode)
        {
            foreach (var node in footerNode.Children)
            {
                var control = await CreateControlAsync(node, cache, cultureInfo, cancellationToken)
                    .ConfigureAwait(false);
                footerControls.Add(control);
            }
        }

        if (rootNode["background", rootNode.NodeNamespace] is { } backgroundNode)
        {
            foreach (var node in backgroundNode.Children)
            {
                var control = await CreateControlAsync(node, cache, cultureInfo, cancellationToken)
                    .ConfigureAwait(false);
                backgroundControls.Add(control);
            }
        }

        if (rootNode["foreground", rootNode.NodeNamespace] is { } foregroundNode)
        {
            foreach (var node in foregroundNode.Children)
            {
                var control = await CreateControlAsync(node, cache, cultureInfo, cancellationToken)
                    .ConfigureAwait(false);
                foregroundControls.Add(control);
            }
        }

        if (rootNode["areas", rootNode.NodeNamespace] is { } areaNode)
        {
            foreach (var areaNodeChild in areaNode.Children)
            {
                if (!areaNodeChild.NodeName.Equals("area", StringComparison.OrdinalIgnoreCase))
                    throw new AreaIncompleteException(
                            areaNodeChild.Line,
                            areaNodeChild.Column,
                            $"Areas section may only contain area nodes."
                        );
                static Length? Value(CultureInfo cultureInfo, XmlNodeInformation root, string name)
                {
                    var value = root[name];
                    if (value is null)
                        return null;
                    if (value.IsNullOrEmpty())
                        throw new AreaIncompleteException(
                            root.Line,
                            root.Column,
                            $"Area cannot have empty {value}."
                        );
                    if (Length.TryParse(value, cultureInfo, out var length))
                        return length;
                    throw new AreaIncompleteException(
                        root.Line,
                        root.Column,
                        $"Area property {value} has an invalid format."
                    );
                }

                var left = Value(cultureInfo, areaNodeChild, "left");
                var right = Value(cultureInfo, areaNodeChild, "right");
                var top = Value(cultureInfo, areaNodeChild, "top");
                var bottom = Value(cultureInfo, areaNodeChild, "bottom");
                var width = Value(cultureInfo, areaNodeChild, "width");
                var height = Value(cultureInfo, areaNodeChild, "height");
                var controls = new List<IControl>();
                foreach (var node in areaNodeChild.Children)
                {
                    var control = await CreateControlAsync(node, cache, cultureInfo, cancellationToken)
                        .ConfigureAwait(false);
                    controls.Add(control);
                }

                areaControls.Add(new Area(controls.AsReadOnly(), left, top, right, bottom, width, height));
            }
        }

        return new Template(
            backgroundControls.AsReadOnly(),
            headerControls.AsReadOnly(),
            bodyControls.AsReadOnly(),
            footerControls.AsReadOnly(),
            foregroundControls.AsReadOnly(),
            areaControls.AsReadOnly()
        );
    }

    private static async Task<IControl> CreateControlAsync(
        XmlNodeInformation node,
        ControlStorage storage,
        CultureInfo cultureInfo,
        CancellationToken cancellationToken
    )
    {
        IControl? control = null;
        try
        {
            control = storage.Create(node.NodeNamespace, node.NodeName, node.Attributes, node.TextContent, cultureInfo);
            if (control is not IContentControl contentControl)
            {
                if (node.Children.Count != 0)
                    throw new ContentControlDoesNotSupportChildrenException(
                        node.Children.First()
                            .Line,
                        node.Children.First()
                            .Column,
                        $"The control {node.NodeNamespace}:{node.NodeName} does not support child controls."
                    );
            }
            else
            {
                foreach (var child in node.Children)
                {
                    IControl? childControl = null;
                    try
                    {
                        childControl = await CreateControlAsync(child, storage, cultureInfo, cancellationToken)
                            .ConfigureAwait(false);
                        var childType = childControl.GetType();
                        if (!contentControl.CanAdd(childType))
                            throw new ContentControlDoesNotSupportTheProvidedChildException(
                                child.Line,
                                child.Column,
                                childType,
                                $"The parent control does not support the child control at L{child.Line}:C{child.Column}."
                            );
                        contentControl.Add(childControl);
                    }
                    catch when (childControl is not null)
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        if (childControl is IAsyncDisposable asyncDisposable)
                            await asyncDisposable.DisposeAsync()
                                .ConfigureAwait(false);
                        // ReSharper disable once SuspiciousTypeConversion.Global
                        else if (childControl is IDisposable disposable)
                            disposable.Dispose();
                        throw;
                    }
                }
            }

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (control is IInitializeAsync initializeAsync)
                await initializeAsync.InitializeAsync(cancellationToken)
                    .ConfigureAwait(false);

            return control;
        }
        catch (Exception ex)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (control is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync()
                    .ConfigureAwait(false);
            // ReSharper disable once SuspiciousTypeConversion.Global
            else if (control is IDisposable disposable)
                disposable.Dispose();
            throw new FailedToCreateControlException(ex, node);
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var headerControl in HeaderControls)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (headerControl is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync()
                    .ConfigureAwait(false);
            // ReSharper disable once SuspiciousTypeConversion.Global
            else if (headerControl is IDisposable disposable)
                disposable.Dispose();
        }

        foreach (var bodyControl in BodyControls)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (bodyControl is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync()
                    .ConfigureAwait(false);
            // ReSharper disable once SuspiciousTypeConversion.Global
            else if (bodyControl is IDisposable disposable)
                disposable.Dispose();
        }

        foreach (var footerControl in FooterControls)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (footerControl is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync()
                    .ConfigureAwait(false);
            // ReSharper disable once SuspiciousTypeConversion.Global
            else if (footerControl is IDisposable disposable)
                disposable.Dispose();
        }
    }
}