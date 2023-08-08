using SkiaSharp;
using X39.Solutions.PdfTemplate.Data;
using X39.Util.Threading;

namespace X39.Solutions.PdfTemplate.Services;

/// <summary>
/// Cache for SkPaint.
/// </summary>
public sealed class SkPaintCache : IDisposable
{
    private readonly record struct Key(Color Color);
    private readonly Dictionary<Key, SKPaint> _paints = new();
    private readonly ReaderWriterLockSlim       _lock   = new();

    /// <inheritdoc />
    public void Dispose()
    {
        _lock.WriteLocked(
            () =>
            {
                foreach(var color in _paints.Values)
                    color.Dispose();
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
    public SKPaint GetStrokePaint(Color color, float thickness)
    {
        var key = new Key(color);
        return _lock.UpgradeableReadLocked(
            () =>
            {
                if (_paints.TryGetValue(key, out var paint))
                    return paint;
                return _lock.WriteLocked(
                    () =>
                    {
                        if (_paints.TryGetValue(key, out var paint2))
                            return paint2;
                        return _paints[key] = new SKPaint
                        {
                            Color = color,
                            StrokeWidth = thickness,
                            StrokeCap = SKStrokeCap.Round,
                        };
                    });
            });
    }
}