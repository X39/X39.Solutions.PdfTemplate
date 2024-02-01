using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate;

internal sealed class Template : IAsyncDisposable
{
    private Template(
        IReadOnlyCollection<IControl> headerControls,
        IReadOnlyCollection<IControl> bodyControls,
        IReadOnlyCollection<IControl> footerControls)
    {
        HeaderControls = headerControls;
        BodyControls   = bodyControls;
        FooterControls = footerControls;
    }

    public IReadOnlyCollection<IControl> HeaderControls { get; }
    public IReadOnlyCollection<IControl> BodyControls { get; }
    public IReadOnlyCollection<IControl> FooterControls { get; }


    public static async Task<Template> CreateAsync(
        XmlNodeInformation rootNode,
        ControlStorage cache,
        CultureInfo cultureInfo,
        CancellationToken cancellationToken)
    {
        var headerControls = new List<IControl>();
        var bodyControls = new List<IControl>();
        var footerControls = new List<IControl>();
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

        return new Template(headerControls.AsReadOnly(), bodyControls.AsReadOnly(), footerControls.AsReadOnly());
    }

    private static async Task<IControl> CreateControlAsync(
        XmlNodeInformation node,
        ControlStorage storage,
        CultureInfo cultureInfo,
        CancellationToken cancellationToken)
    {
        IControl? control = null;
        try
        {
            control = storage.Create(
                node.NodeNamespace,
                node.NodeName,
                node.Attributes,
                node.TextContent,
                cultureInfo);
            if (control is not IContentControl contentControl)
            {
                if (node.Children.Count != 0)
                    throw new InvalidOperationException(
                        $"The control {node.NodeNamespace}:{node.NodeName} does not support child controls.");
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
                            throw new ContentControlDoesNotSupportChild(
                                child.Line,
                                child.Column,
                                childType,
                                $"The parent control does not support the child control at L{child.Line}:C{child.Column}.");
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