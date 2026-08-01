using System.Xml;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Services;
using X39.Solutions.Papercraft.Services.TextService;
using static System.String;

namespace X39.Solutions.Papercraft;

internal sealed class PapercraftRenderPipeline
{
    private readonly PapercraftGenerator _generator;
    private readonly IPapercraftRenderBackend[] _backends;
    private readonly Func<IPapercraftRenderBackend, IControlFactory?>? _createControlFactory;

    public PapercraftRenderPipeline(
        PapercraftGenerator generator,
        IEnumerable<IPapercraftRenderBackend> backends)
    {
        ArgumentNullException.ThrowIfNull(generator);
        ArgumentNullException.ThrowIfNull(backends);
        _generator = generator;
        _backends = backends.ToArray();
    }

    public PapercraftRenderPipeline(
        PapercraftGenerator generator,
        IEnumerable<IPapercraftRenderBackend> backends,
        IServiceProvider serviceProvider,
        ControlActivationCache controlActivationCache,
        ControlRegistry controlRegistry)
        : this(generator, backends)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(controlActivationCache);
        ArgumentNullException.ThrowIfNull(controlRegistry);

        _createControlFactory = (backend) => new ControlFactory(
            new BackendTextServiceProvider(serviceProvider, backend.TextService),
            controlActivationCache,
            controlRegistry);
    }

    public ITemplateData TemplateData => _generator.TemplateData;

    public IReadOnlyCollection<IPapercraftRenderBackend> Backends => _backends;

    public async ValueTask<RenderValidationResult> ValidateAsync(
        XmlReader reader,
        RenderTarget target,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererValidate);
        PapercraftActivity.SetRenderTarget(activity, target);
        try
        {
            using var diagnosticScope = RenderDiagnosticScope.Begin(null);
            var renderOptions = options ?? PapercraftRenderOptions.Default;
            if (IsLoweredXmlTarget(target))
            {
                _ = await _generator.ReadLoweredXmlAsync(
                        reader,
                        cultureInfo,
                        renderOptions.DocumentOptions,
                        cancellationToken)
                    .ConfigureAwait(false);
                var loweredXmlValidation = ValidateLoweredXmlEmbeddedFiles(renderOptions.DocumentOptions);
                PapercraftActivity.SetValidation(activity, loweredXmlValidation);
                return loweredXmlValidation;
            }

            var backend = SelectBackend(target, renderOptions);
            PapercraftActivity.SetBackend(activity, backend);
            var document = await GenerateDocumentAsync(
                    reader,
                    cultureInfo,
                    renderOptions.DocumentOptions,
                    backend,
                    cancellationToken)
                .ConfigureAwait(false);
            PapercraftActivity.SetDocument(activity, document);
            var validation = await ValidateBackendAsync(backend, document, target, cancellationToken)
                .ConfigureAwait(false);
            validation = CombineWithScopedDiagnostics(validation);
            PapercraftActivity.SetValidation(activity, validation);
            return validation;
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    public async ValueTask RenderAsync(
        XmlReader reader,
        RenderOutput output,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererRender);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        try
        {
            using var diagnosticScope = RenderDiagnosticScope.Begin(output.DiagnosticSink);
            var renderOptions = options ?? PapercraftRenderOptions.Default;
            if (IsLoweredXmlTarget(output.Target))
            {
                var validation = ValidateLoweredXmlEmbeddedFiles(renderOptions.DocumentOptions);
                RenderDiagnosticScope.Report(validation.Diagnostics);
                validation.ThrowIfUnsupported();
                var lowered = await _generator.ReadLoweredXmlAsync(
                        reader,
                        cultureInfo,
                        renderOptions.DocumentOptions,
                        cancellationToken)
                    .ConfigureAwait(false);
                await LoweredXmlWriter.WriteAsync(lowered, output.Stream, cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            var backend = SelectBackend(output.Target, renderOptions);
            PapercraftActivity.SetBackend(activity, backend);
            var document = await GenerateDocumentAsync(
                    reader,
                    cultureInfo,
                    renderOptions.DocumentOptions,
                    backend,
                    cancellationToken)
                .ConfigureAwait(false);
            PapercraftActivity.SetDocument(activity, document);
            await RenderAsync(document, output, renderOptions, backend, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    public async ValueTask RenderAsync(
        PapercraftDocument document,
        RenderOutput output,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);

        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererRender);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        PapercraftActivity.SetDocument(activity, document);
        try
        {
            using var diagnosticScope = RenderDiagnosticScope.Begin(output.DiagnosticSink);
            if (IsLoweredXmlTarget(output.Target))
                throw new InvalidOperationException("Lowered XML output requires the template-based RenderAsync overload.");

            var renderOptions = options ?? PapercraftRenderOptions.Default;
            var backend = SelectBackend(output.Target, renderOptions);
            PapercraftActivity.SetBackend(activity, backend);
            await RenderAsync(document, output, renderOptions, backend, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    public async ValueTask RenderRasterPagesAsync(
        XmlReader reader,
        RasterPageRenderOutput output,
        CultureInfo cultureInfo,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(cultureInfo);

        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererRenderRasterPages);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        try
        {
            var renderOptions = options ?? PapercraftRenderOptions.Default;
            var backend = SelectBackend(output.Target, renderOptions);
            PapercraftActivity.SetBackend(activity, backend);
            var document = await GenerateDocumentAsync(
                    reader,
                    cultureInfo,
                    renderOptions.DocumentOptions,
                    backend,
                    cancellationToken)
                .ConfigureAwait(false);
            PapercraftActivity.SetDocument(activity, document);
            await RenderRasterPagesAsync(document, output, renderOptions, backend, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    public async ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        PapercraftRenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);

        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererRenderRasterPages);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        PapercraftActivity.SetDocument(activity, document);
        try
        {
            var renderOptions = options ?? PapercraftRenderOptions.Default;
            var backend = SelectBackend(output.Target, renderOptions);
            PapercraftActivity.SetBackend(activity, backend);
            await RenderRasterPagesAsync(document, output, renderOptions, backend, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private async ValueTask RenderAsync(
        PapercraftDocument document,
        RenderOutput output,
        PapercraftRenderOptions options,
        IPapercraftRenderBackend backend,
        CancellationToken cancellationToken)
    {
        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererBackendRender);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        PapercraftActivity.SetDocument(activity, document);
        PapercraftActivity.SetBackend(activity, backend);
        try
        {
            var validation = await ValidateBackendAsync(backend, document, output.Target, cancellationToken)
                .ConfigureAwait(false);
            RenderDiagnosticScope.Report(validation.Diagnostics);
            validation = CombineWithScopedDiagnostics(validation);
            PapercraftActivity.SetValidation(activity, validation);
            validation.ThrowIfUnsupportedOrStrictDegraded(options.TreatDegradedAsUnsupported);
            await backend.RenderAsync(document, output, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private async ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        PapercraftRenderOptions options,
        IPapercraftRenderBackend backend,
        CancellationToken cancellationToken)
    {
        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererBackendRenderRasterPages);
        PapercraftActivity.SetRenderTarget(activity, output.Target);
        PapercraftActivity.SetDocument(activity, document);
        PapercraftActivity.SetBackend(activity, backend);
        try
        {
            var validation = await ValidateBackendAsync(backend, document, output.Target, cancellationToken)
                .ConfigureAwait(false);
            PapercraftActivity.SetValidation(activity, validation);
            validation.ThrowIfUnsupportedOrStrictDegraded(options.TreatDegradedAsUnsupported);
            await backend.RenderRasterPagesAsync(document, output, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private IPapercraftRenderBackend SelectBackend(RenderTarget target, PapercraftRenderOptions options)
    {
        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererSelectBackend);
        PapercraftActivity.SetRenderTarget(activity, target);
        if (!IsNullOrWhiteSpace(options.BackendId))
            activity?.SetTag(PapercraftActivity.BackendIdTag, options.BackendId);
        try
        {
            if (_backends.Length is 0)
                throw new InvalidOperationException("No Papercraft render backend is registered.");

            if (!IsNullOrWhiteSpace(options.BackendId))
            {
                var backend = _backends.FirstOrDefault(
                    (q) => string.Equals(
                        q.Capabilities.RendererId,
                        options.BackendId,
                        StringComparison.OrdinalIgnoreCase));
                if (backend is null)
                    throw new InvalidOperationException($"No Papercraft render backend with id '{options.BackendId}' is registered.");
                PapercraftActivity.SetBackend(activity, backend);
                return backend;
            }

            var selected = _backends.FirstOrDefault((q) => q.Capabilities.Supports(target))
                           ?? _backends.First();
            PapercraftActivity.SetBackend(activity, selected);
            return selected;
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private async ValueTask<PapercraftDocument> GenerateDocumentAsync(
        XmlReader reader,
        CultureInfo cultureInfo,
        DocumentOptions documentOptions,
        IPapercraftRenderBackend backend,
        CancellationToken cancellationToken)
    {
        var controlFactory = _createControlFactory?.Invoke(backend)
                             ?? _generator.CreateTextServiceScopedControlFactory(backend.TextService);
        return controlFactory is null
            ? await _generator.GenerateAsync(reader, cultureInfo, documentOptions, cancellationToken)
                .ConfigureAwait(false)
            : await _generator.GenerateAsync(reader, cultureInfo, documentOptions, controlFactory, cancellationToken)
                .ConfigureAwait(false);
    }

    private static async ValueTask<RenderValidationResult> ValidateBackendAsync(
        IPapercraftRenderBackend backend,
        PapercraftDocument document,
        RenderTarget target,
        CancellationToken cancellationToken)
    {
        using var activity = PapercraftActivity.Start(PapercraftActivityNames.RendererValidateBackend);
        PapercraftActivity.SetRenderTarget(activity, target);
        PapercraftActivity.SetDocument(activity, document);
        PapercraftActivity.SetBackend(activity, backend);
        try
        {
            var backendValidation = await backend.ValidateAsync(document, target, cancellationToken)
                .ConfigureAwait(false);
            var validation = RenderValidationResult.Combine(
                backendValidation,
                backend.Capabilities.ValidateDocument(document));
            PapercraftActivity.SetValidation(activity, validation);
            return validation;
        }
        catch (Exception ex)
        {
            PapercraftActivity.SetError(activity, ex);
            throw;
        }
    }

    private static bool IsLoweredXmlTarget(RenderTarget target)
        => target.OutputKind is RendererOutputKind.LoweredXml
           || string.Equals(target.MediaType, PapercraftMediaTypes.ApplicationPapercraftLoweredXml, StringComparison.OrdinalIgnoreCase);

    private static RenderValidationResult ValidateLoweredXmlEmbeddedFiles(DocumentOptions options)
    {
        if (options.EmbeddedFiles.Count is 0)
            return RenderValidationResult.Supported;

        return new RenderValidationResult(
            new[]
            {
                new RenderDiagnostic(
                    RenderDiagnosticCodes.UnsupportedFeature,
                    RendererSupportLevel.Unsupported,
                    RendererFeatures.EmbeddedFiles,
                    "Lowered XML output cannot carry embedded files.",
                    "Render the prepared document to PDF to preserve document attachments."),
            });
    }

    private static RenderValidationResult CombineWithScopedDiagnostics(RenderValidationResult validation)
    {
        var scopedDiagnostics = RenderDiagnosticScope.CurrentDiagnostics;
        if (scopedDiagnostics.Count is 0)
            return validation;

        var diagnostics = validation.Diagnostics
            .Concat(scopedDiagnostics)
            .Distinct()
            .ToArray();
        return diagnostics.Length == validation.Diagnostics.Count
               && diagnostics.SequenceEqual(validation.Diagnostics)
            ? validation
            : new RenderValidationResult(diagnostics);
    }

    private sealed class BackendTextServiceProvider : IServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITextService _textService;

        public BackendTextServiceProvider(IServiceProvider serviceProvider, ITextService textService)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(textService);
            _serviceProvider = serviceProvider;
            _textService = textService;
        }

        public object? GetService(Type serviceType)
        {
            ArgumentNullException.ThrowIfNull(serviceType);
            if (serviceType == typeof(ITextService))
                return _textService;
            if (serviceType == typeof(IServiceProvider))
                return this;
            return _serviceProvider.GetService(serviceType);
        }
    }
}
