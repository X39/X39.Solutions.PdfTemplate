using SkiaSharp;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services;

namespace X39.Solutions.PdfTemplate.Abstraction;

internal sealed class CanvasImpl : ICanvas
{
    private readonly SkPaintCache _paintCache;
    private readonly SKCanvas     _canvas;

    public CanvasImpl(SkPaintCache paintCache, SKCanvas canvas)
    {
        _paintCache = paintCache;
        _canvas     = canvas;
    }

    public void PushState()
    {
        _canvas.Save();
    }

    public void ClipRect(Rectangle rectangle)
    {
        _canvas.ClipRect(rectangle);
    }

    public void Translate(Point point)
    {
        _canvas.Translate(point);
    }

    public void PopState()
    {
        _canvas.Restore();
    }

    public void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
    {
        _canvas.DrawLine(startX, startY, endX, endY, _paintCache.GetStrokePaint(color, thickness));
    }
}