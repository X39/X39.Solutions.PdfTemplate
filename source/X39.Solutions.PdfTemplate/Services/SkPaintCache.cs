using System.ComponentModel;
using SkiaSharp;
using X39.Solutions.PdfTemplate.Data;
using X39.Util.Threading;

namespace X39.Solutions.PdfTemplate.Services;

/// <summary>
/// Cache for SkPaint.
/// </summary>
public sealed class SkPaintCache : IDisposable
{
    // ReSharper disable NotAccessedPositionalProperty.Local -- Disabled as this is a key-only record
    private readonly record struct StrokePaintKey(Color Color, float Thickness);
    // ReSharper restore NotAccessedPositionalProperty.Local

    private readonly Dictionary<StrokePaintKey, SKPaint> _strokePaints     = new();
    private readonly ReaderWriterLockSlim                _strokePaintsLock = new();
    private readonly Dictionary<TextStyle, SKPaint>      _textPaints       = new();
    private readonly ReaderWriterLockSlim                _textPaintsLock   = new();
    private readonly Dictionary<Color, SKPaint>      _fillPaintKey     = new();
    private readonly ReaderWriterLockSlim                _fillPaintKeyLock = new();

    /// <inheritdoc />
    public void Dispose()
    {
        _strokePaintsLock.WriteLocked(
            () =>
            {
                foreach (var skPaint in _strokePaints.Values)
                    skPaint.Dispose();
            });
        _textPaintsLock.WriteLocked(
            () =>
            {
                foreach (var skPaint in _textPaints.Values)
                    skPaint.Dispose();
            });
    }

    /// <summary>
    /// Method to receive the <see cref="SKPaint"/> for a stroke.
    /// </summary>
    /// <remarks>
    /// Gets a <see cref="SKPaint"/> from the cache or adds it if it does not exist.
    /// </remarks>
    /// <param name="color">The color of the paint.</param>
    /// <param name="thickness">The thickness of the color</param>
    /// <returns>A <see cref="SKPaint"/> with the given parameters.</returns>
    public SKPaint Get(Color color, float thickness)
    {
        var key = new StrokePaintKey(color, thickness);
        return _strokePaintsLock.UpgradeableReadLocked(
            () =>
            {
                if (_strokePaints.TryGetValue(key, out var skPaint1))
                    return skPaint1;
                return _strokePaintsLock.WriteLocked(
                    () =>
                    {
                        if (_strokePaints.TryGetValue(key, out var skPaint2))
                            return skPaint2;
                        return _strokePaints[key] = new SKPaint
                        {
                            Color       = color,
                            StrokeWidth = thickness,
                            IsStroke = true,
                        };
                    });
            });
    }

    /// <summary>
    /// Method to receive the <see cref="SKPaint"/> for a filled color.
    /// </summary>
    /// <remarks>
    /// Gets a <see cref="SKPaint"/> from the cache or adds it if it does not exist.
    /// </remarks>
    /// <param name="color">The color of the paint.</param>
    /// <returns>A <see cref="SKPaint"/> with the given parameters.</returns>
    public SKPaint Get(Color color)
    {
        var key = color;
        return _fillPaintKeyLock.UpgradeableReadLocked(
            () =>
            {
                if (_fillPaintKey.TryGetValue(key, out var skPaint1))
                    return skPaint1;
                return _fillPaintKeyLock.WriteLocked(
                    () =>
                    {
                        if (_fillPaintKey.TryGetValue(key, out var skPaint2))
                            return skPaint2;
                        return _fillPaintKey[key] = new SKPaint
                        {
                            Color       = color,
                            IsStroke = false,
                        };
                    });
            });
    }

    /// <summary>
    /// Returns a <see cref="SKPaint"/> for the given <see cref="TextStyle"/>.
    /// </summary>
    /// <param name="textStyle">The <see cref="TextStyle"/> to get the <see cref="SKPaint"/> for.</param>
    /// <param name="dpi">The DPI to use.</param>
    /// <returns>A <see cref="SKPaint"/> for the given <see cref="TextStyle"/>.</returns>
    /// <exception cref="InvalidEnumArgumentException">Thrown when the <see cref="EFontStyle"/> is not supported, indicating a programming error, not a user one.</exception>
    public SKPaint Get(TextStyle textStyle, float dpi)
    {
        return _textPaintsLock.UpgradeableReadLocked(
            () =>
            {
                if (_textPaints.TryGetValue(textStyle, out var paint))
                    return paint;
                return _textPaintsLock.WriteLocked(
                    () =>
                    {
                        if (_textPaints.TryGetValue(textStyle, out var paint2))
                            return paint2;
                        return _textPaints[textStyle] = new SKPaint
                        {
                            Color       = textStyle.Foreground,
                            StrokeWidth = textStyle.StrokeThickness,
                            StrokeCap   = SKStrokeCap.Round,
                            Typeface = SKTypeface.FromFamilyName(
                                textStyle.FontFamily.Family,
                                textStyle.FontFamily.Weight,
                                textStyle.FontFamily.LetterSpacing,
                                textStyle.FontFamily.Style switch
                                {
                                    EFontStyle.Upright => SKFontStyleSlant.Upright,
                                    EFontStyle.Italic  => SKFontStyleSlant.Italic,
                                    EFontStyle.Oblique => SKFontStyleSlant.Oblique,
                                    _ => throw new InvalidEnumArgumentException(
                                        nameof(textStyle.FontFamily.Style),
                                        (int) textStyle.FontFamily.Style,
                                        typeof(EFontStyle)),
                                }),
                            TextScaleX = textStyle.Scale,
                            TextSkewX  = textStyle.Rotation,
                            TextSize   = textStyle.FontSize * dpi / 72.272F,
                        };
                    });
            });
    }
}