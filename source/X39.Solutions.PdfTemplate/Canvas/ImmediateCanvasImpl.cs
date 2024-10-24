using SkiaSharp;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services;

namespace X39.Solutions.PdfTemplate.Canvas;

internal sealed class ImmediateCanvasImpl : IImmediateCanvas
{
    private readonly SkPaintCache _paintCache;
    private readonly SKCanvas     _canvas;

    public ImmediateCanvasImpl(SKCanvas canvas, SkPaintCache paintCache)
    {
        _canvas     = canvas;
        _paintCache = paintCache;
    }

    public void PushState()               => _canvas.Save();
    public void Clip(Rectangle rectangle) => _canvas.ClipRect(rectangle);
    public void PopState()                => _canvas.Restore();

    public void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
        => _canvas.DrawLine(startX, startY, endX, endY, _paintCache.Get(color, thickness));

    public void Translate(Point point) => _canvas.Translate(point.X, point.Y);

    public void DrawText(TextStyle textStyle, float dpi, string text, float x, float y)
        => _canvas.DrawText(text, x, y, _paintCache.Get(textStyle, dpi));

    public void DrawRect(Rectangle rectangle, Color color) => _canvas.DrawRect(rectangle, _paintCache.Get(color));

    public void DrawBitmap(byte[] bytes, Rectangle rectangle)
    {
        using var stream = new MemoryStream(bytes);
        using var bitmap = SKBitmap.Decode(stream);
        if (bitmap is null)
            return;
        _canvas.DrawBitmap(bitmap, rectangle);
    }

    public void   DrawBitmap(SKBitmap bitmap, Rectangle arrangementInner) => _canvas.DrawBitmap(bitmap, arrangementInner);
    public ushort PageNumber { get; internal set; }
    public ushort TotalPages { get; internal set; }
}
