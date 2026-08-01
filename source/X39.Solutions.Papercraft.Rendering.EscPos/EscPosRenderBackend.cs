using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Rendering.EscPos;

/// <summary>
/// Papercraft render backend that emits first-pass ESC/POS printer command bytes.
/// </summary>
public sealed class EscPosRenderBackend : IPapercraftRenderBackend
{
    /// <summary>
    /// The ESC/POS media type supported by this backend.
    /// </summary>
    public const string MediaType = "application/vnd.papercraft.escpos";

    /// <summary>
    /// The explicit printer-command render target for this backend.
    /// </summary>
    public static RenderTarget Target { get; } = new(MediaType, RendererOutputKind.PrinterCommands);

    private const string RectangleFeature = "drawing.rectangle";
    private const string NonHorizontalLineFeature = "drawing.line.non-horizontal";
    private const string BackendDrawFeature = "drawing.backend-command";
    private const string TextRotationFeature = "text.rotation";
    private static readonly byte[] InitializeCommand = { 0x1B, 0x40 };
    private static readonly byte[] ResetSizeCommand = { 0x1D, 0x21, 0x00 };
    private static readonly byte[] DoubleSizeCommand = { 0x1D, 0x21, 0x11 };
    private static readonly byte[] BoldOnCommand = { 0x1B, 0x45, 0x01 };
    private static readonly byte[] BoldOffCommand = { 0x1B, 0x45, 0x00 };
    private static readonly byte[] UnderlineOffCommand = { 0x1B, 0x2D, 0x00 };
    private static readonly byte[] UnderlineOnCommand = { 0x1B, 0x2D, 0x01 };
    private static readonly byte[] DoubleUnderlineOnCommand = { 0x1B, 0x2D, 0x02 };
    private static readonly byte[] FullCutCommand = { 0x1D, 0x56, 0x00 };
    private static readonly byte[] LineFeedCommand = { 0x0A };

    private static readonly RendererCapabilities StaticCapabilities = new(
        "escpos",
        "ESC/POS",
        RendererOutputKind.PrinterCommands,
        new[] { MediaType },
        new Dictionary<string, RendererSupportLevel>(StringComparer.OrdinalIgnoreCase)
        {
            [RendererFeatures.PrinterCommandOutput] = RendererSupportLevel.Supported,
            [RendererFeatures.Multipage] = RendererSupportLevel.Degraded,
            [RendererFeatures.TextMeasurement] = RendererSupportLevel.Degraded,
            [RendererFeatures.TextDrawing] = RendererSupportLevel.Supported,
            [RendererFeatures.Images] = RendererSupportLevel.Unsupported,
            [RendererFeatures.Clipping] = RendererSupportLevel.Unsupported,
            [RendererFeatures.Transparency] = RendererSupportLevel.Unsupported,
            [RendererFeatures.Fonts] = RendererSupportLevel.Degraded,
            [RendererFeatures.Color] = RendererSupportLevel.Degraded,
            [RendererFeatures.AbsolutePositioning] = RendererSupportLevel.Degraded,
            [RendererFeatures.LinkAnnotations] = RendererSupportLevel.Unsupported,
            [RendererFeatures.EmbeddedFiles] = RendererSupportLevel.Unsupported,
        },
        "First-pass text-oriented ESC/POS command output. Text, basic emphasis, and horizontal rules are emitted directly; no printer transport is performed.");

    private readonly EscPosRenderOptions _options;

    /// <summary>
    /// Creates an ESC/POS render backend with default options.
    /// </summary>
    public EscPosRenderBackend()
        : this(EscPosRenderOptions.Default)
    {
    }

    /// <summary>
    /// Creates an ESC/POS render backend.
    /// </summary>
    /// <param name="options">The ESC/POS render options.</param>
    public EscPosRenderBackend(EscPosRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.TextEncoding);
        ArgumentOutOfRangeException.ThrowIfNegative(options.PageFeedLines);
        ArgumentOutOfRangeException.ThrowIfNegative(options.DocumentEndFeedLines);
        ArgumentOutOfRangeException.ThrowIfLessThan(options.CharactersPerLine, 1);
        _options = options;
    }

    /// <inheritdoc />
    public RendererCapabilities Capabilities => StaticCapabilities;

    /// <inheritdoc />
    public ITextService TextService { get; } = new EscPosTextService();

    /// <inheritdoc />
    public ValueTask<RenderValidationResult> ValidateAsync(
        PapercraftDocument document,
        RenderTarget target,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(target);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = RenderValidationResult.Combine(
            Capabilities.ValidateTarget(target),
            Capabilities.ValidateDocument(document),
            ValidateEscPosCommandShape(document));
        return ValueTask.FromResult(validation);
    }

    /// <inheritdoc />
    public async ValueTask RenderAsync(
        PapercraftDocument document,
        RenderOutput output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);

        var validation = await ValidateAsync(document, output.Target, cancellationToken)
            .ConfigureAwait(false);
        validation.ThrowIfUnsupported();

        await WriteDocumentAsync(document, output.Stream, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask RenderRasterPagesAsync(
        PapercraftDocument document,
        RasterPageRenderOutput output,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(output);
        cancellationToken.ThrowIfCancellationRequested();

        var validation = Capabilities.ValidateTarget(output.Target);
        validation.ThrowIfUnsupported();

        throw new RenderValidationException(
            new RenderValidationResult(
                new[]
                {
                    new RenderDiagnostic(
                        RenderDiagnosticCodes.UnsupportedOutputKind,
                        RendererSupportLevel.Unsupported,
                        RendererFeatures.RasterImageOutput,
                        $"Backend '{Capabilities.DisplayName}' only produces ESC/POS printer command output.",
                        "Use a raster-capable backend for page-by-page raster rendering."),
                }));
    }

    private static RenderValidationResult ValidateEscPosCommandShape(PapercraftDocument document)
    {
        var diagnostics = new List<RenderDiagnostic>();
        var seenFeatures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var page in document.Pages)
        {
            foreach (var command in page.DisplayList.Commands)
            {
                switch (command)
                {
                    case DrawRectangleCommand when seenFeatures.Add(RectangleFeature):
                        diagnostics.Add(
                            new RenderDiagnostic(
                                RenderDiagnosticCodes.UnsupportedFeature,
                                RendererSupportLevel.Unsupported,
                                RectangleFeature,
                                "Filled rectangles are not supported by the first-pass ESC/POS backend.",
                                "The ESC/POS backend currently emits text and horizontal text-rule approximations only."));
                        break;
                    case DrawLineCommand drawLine when !IsHorizontal(drawLine)
                                                     && seenFeatures.Add(NonHorizontalLineFeature):
                        diagnostics.Add(
                            new RenderDiagnostic(
                                RenderDiagnosticCodes.UnsupportedFeature,
                                RendererSupportLevel.Unsupported,
                                NonHorizontalLineFeature,
                                "Only horizontal line commands can be approximated by the first-pass ESC/POS backend.",
                                "Non-horizontal line geometry cannot be represented as simple printer text commands."));
                        break;
                    case BackendDrawCommand when seenFeatures.Add(BackendDrawFeature):
                        diagnostics.Add(
                            new RenderDiagnostic(
                                RenderDiagnosticCodes.UnsupportedFeature,
                                RendererSupportLevel.Unsupported,
                                BackendDrawFeature,
                                "Backend-specific drawing callbacks are not supported by the ESC/POS backend.",
                                "The ESC/POS backend only consumes renderer-neutral display commands."));
                        break;
                    case DrawTextCommand drawText when Math.Abs(drawText.TextStyle.Rotation) > 0.001F
                                                    && seenFeatures.Add(TextRotationFeature):
                        diagnostics.Add(
                            new RenderDiagnostic(
                                RenderDiagnosticCodes.DegradedFeature,
                                RendererSupportLevel.Degraded,
                                TextRotationFeature,
                                "Rotated text is emitted without rotation by the first-pass ESC/POS backend.",
                                "ESC/POS text commands do not preserve arbitrary Papercraft text rotation."));
                        break;
                }
            }
        }

        return diagnostics.Count is 0
            ? RenderValidationResult.Supported
            : new RenderValidationResult(diagnostics);
    }

    private async ValueTask WriteDocumentAsync(
        PapercraftDocument document,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        if (_options.InitializePrinter)
        {
            await WriteBytesAsync(outputStream, InitializeCommand, cancellationToken)
                .ConfigureAwait(false);
        }

        for (var i = 0; i < document.Pages.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await WritePageAsync(document.Pages[i], outputStream, cancellationToken)
                .ConfigureAwait(false);

            if (i < document.Pages.Count - 1)
            {
                await FeedLinesAsync(outputStream, _options.PageFeedLines, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        await FeedLinesAsync(outputStream, _options.DocumentEndFeedLines, cancellationToken)
            .ConfigureAwait(false);

        if (_options.CutPaper)
        {
            await WriteBytesAsync(outputStream, FullCutCommand, cancellationToken)
                .ConfigureAwait(false);
        }

        await outputStream.FlushAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask WritePageAsync(
        PapercraftPage page,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        foreach (var command in page.DisplayList.Commands)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (command)
            {
                case DrawTextCommand drawText:
                    await WriteTextAsync(drawText, outputStream, cancellationToken)
                        .ConfigureAwait(false);
                    break;
                case DrawLineCommand drawLine when IsHorizontal(drawLine):
                    await WriteHorizontalRuleAsync(drawLine, page, outputStream, cancellationToken)
                        .ConfigureAwait(false);
                    break;
            }
        }
    }

    private async ValueTask WriteTextAsync(
        DrawTextCommand command,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        var bold = IsBold(command.TextStyle);
        var doubleSize = IsDoubleSize(command.TextStyle);
        var underlineCommand = GetUnderlineCommand(command.TextStyle);

        if (bold)
        {
            await WriteBytesAsync(outputStream, BoldOnCommand, cancellationToken)
                .ConfigureAwait(false);
        }

        if (doubleSize)
        {
            await WriteBytesAsync(outputStream, DoubleSizeCommand, cancellationToken)
                .ConfigureAwait(false);
        }

        if (underlineCommand is not null)
        {
            await WriteBytesAsync(outputStream, underlineCommand, cancellationToken)
                .ConfigureAwait(false);
        }

        await WriteEncodedTextAsync(outputStream, command.Text, cancellationToken)
            .ConfigureAwait(false);
        await WriteBytesAsync(outputStream, LineFeedCommand, cancellationToken)
            .ConfigureAwait(false);

        if (underlineCommand is not null)
        {
            await WriteBytesAsync(outputStream, UnderlineOffCommand, cancellationToken)
                .ConfigureAwait(false);
        }

        if (doubleSize)
        {
            await WriteBytesAsync(outputStream, ResetSizeCommand, cancellationToken)
                .ConfigureAwait(false);
        }

        if (bold)
        {
            await WriteBytesAsync(outputStream, BoldOffCommand, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async ValueTask WriteHorizontalRuleAsync(
        DrawLineCommand command,
        PapercraftPage page,
        Stream outputStream,
        CancellationToken cancellationToken)
    {
        var width = Math.Abs(command.EndX - command.StartX);
        var columns = page.PageSize.Width <= 0
            ? _options.CharactersPerLine
            : (int)MathF.Round(width / page.PageSize.Width * _options.CharactersPerLine);
        columns = Math.Clamp(columns, 1, _options.CharactersPerLine);

        await WriteEncodedTextAsync(outputStream, new string('-', columns), cancellationToken)
            .ConfigureAwait(false);
        await WriteBytesAsync(outputStream, LineFeedCommand, cancellationToken)
            .ConfigureAwait(false);
    }

    private async ValueTask WriteEncodedTextAsync(
        Stream outputStream,
        string text,
        CancellationToken cancellationToken)
    {
        var bytes = _options.TextEncoding.GetBytes(text);
        await outputStream.WriteAsync(bytes, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async ValueTask FeedLinesAsync(
        Stream outputStream,
        int lineCount,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < lineCount; i++)
        {
            await WriteBytesAsync(outputStream, LineFeedCommand, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private static async ValueTask WriteBytesAsync(
        Stream outputStream,
        byte[] bytes,
        CancellationToken cancellationToken)
        => await outputStream.WriteAsync(bytes, cancellationToken)
            .ConfigureAwait(false);

    private static bool IsHorizontal(DrawLineCommand command)
        => Math.Abs(command.StartY - command.EndY) <= Math.Max(0.5F, command.Thickness / 2F);

    private static bool IsBold(DisplayTextStyle style)
        => style.FontFamily.Weight >= 700;

    private static bool IsDoubleSize(DisplayTextStyle style)
        => style.FontSize * Math.Max(0F, style.Scale) >= 18F;

    private static byte[]? GetUnderlineCommand(DisplayTextStyle style)
    {
        if (style.Decoration.HasFlag(TextDecoration.DoubleUnderline))
            return DoubleUnderlineOnCommand;
        return style.Decoration.HasFlag(TextDecoration.Underline)
            ? UnderlineOnCommand
            : null;
    }

    private sealed class EscPosTextService : ITextService
    {
        public Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
        {
            var fontSize = GetFontSize(textStyle, dpi);
            var lineHeight = GetLineHeight(textStyle, fontSize);
            var maxLineWidth = 0F;
            var lineCount = 0;
            foreach (var line in LayoutLines(textStyle, dpi, text, maxWidth))
            {
                maxLineWidth = Math.Max(maxLineWidth, MeasureLine(textStyle, dpi, line));
                lineCount++;
            }

            return lineCount is 0
                ? Size.Zero
                : new Size(maxLineWidth, lineCount * lineHeight);
        }

        public void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
        {
            ArgumentNullException.ThrowIfNull(canvas);
            var fontSize = GetFontSize(textStyle, dpi);
            var lineHeight = GetLineHeight(textStyle, fontSize);
            var y = fontSize;
            foreach (var line in LayoutLines(textStyle, dpi, text, maxWidth))
            {
                canvas.DrawText(textStyle, dpi, line, 0F, y);
                y += lineHeight;
            }
        }

        private static IReadOnlyList<string> LayoutLines(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
        {
            var lines = new List<string>();
            foreach (var paragraph in text.ToString().Split('\n'))
            {
                lines.AddRange(WrapLine(textStyle, dpi, paragraph.Trim(), maxWidth));
            }

            return lines;
        }

        private static IReadOnlyList<string> WrapLine(TextStyle textStyle, float dpi, string text, float maxWidth)
        {
            var lines = new List<string>();
            if (string.IsNullOrWhiteSpace(text))
                return lines;

            if (!float.IsFinite(maxWidth) || maxWidth <= 0F)
            {
                lines.Add(text);
                return lines;
            }

            var remaining = text;
            while (!string.IsNullOrWhiteSpace(remaining))
            {
                var end = remaining.Length;
                while (end > 1 && MeasureLine(textStyle, dpi, remaining.AsSpan(0, end)) > maxWidth)
                {
                    var whitespace = LastWhitespaceBefore(remaining.AsSpan(), end);
                    end = whitespace > 0
                        ? whitespace
                        : Math.Max(1, end - 1);
                    if (whitespace <= 0)
                        break;
                }

                lines.Add(remaining[..end].Trim());
                remaining = remaining[end..].TrimStart();
            }

            return lines;
        }

        private static int LastWhitespaceBefore(ReadOnlySpan<char> text, int endExclusive)
        {
            for (var i = Math.Min(endExclusive, text.Length) - 1; i > 0; i--)
            {
                if (char.IsWhiteSpace(text[i]))
                    return i;
            }

            return -1;
        }

        private static float MeasureLine(TextStyle textStyle, float dpi, ReadOnlySpan<char> line)
            => line.Length * GetFontSize(textStyle, dpi) * GetAverageGlyphWidth(textStyle);

        private static float GetFontSize(TextStyle textStyle, float dpi)
            => Math.Max(1F, textStyle.FontSize * dpi / 72.272F * Math.Max(0.01F, textStyle.Scale));

        private static float GetLineHeight(TextStyle textStyle, float fontSize)
            => fontSize * Math.Max(0.1F, textStyle.LineHeight);

        private static float GetAverageGlyphWidth(TextStyle textStyle)
            => textStyle.FontFamily.Family.Contains("mono", StringComparison.OrdinalIgnoreCase)
                ? 0.6F
                : 0.55F;
    }
}
