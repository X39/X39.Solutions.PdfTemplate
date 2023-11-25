using SkiaSharp;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services;

namespace X39.Solutions.PdfTemplate.Abstraction;

internal sealed class CanvasImpl : ICanvas
{
    private readonly SkPaintCache           _paintCache;
    private readonly List<Action<SKCanvas>> _drawActions = new();

    public CanvasImpl(SkPaintCache paintCache)
    {
        _paintCache = paintCache;
    }
    
    internal void Render(SKCanvas canvas)
    {
        foreach (var action in _drawActions)
        {
            action(canvas);
        }
    }

    public void PushState()
    {
        _drawActions.Add((canvas) => canvas.Save());
    }

    public void Clip(Rectangle rectangle)
    {
        _drawActions.Add((canvas) => canvas.ClipRect(rectangle));
    }

    public void DrawRect(Rectangle rectangle, Color color)
    {
        _drawActions.Add((canvas) =>
        {
            var paint = _paintCache.Get(color);
            canvas.DrawRect(rectangle, paint);
        });
    }

    public void Translate(Point point)
    {
        _drawActions.Add((canvas) => canvas.Translate(point));
    }

    public void PopState()
    {
        _drawActions.Add((canvas) => canvas.Restore());
    }

    public void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
    {
        _drawActions.Add((canvas) =>
        {
            var paint = _paintCache.Get(color, thickness);
            canvas.DrawLine(startX, startY, endX, endY, paint);
        });
    }

    public void DrawText(TextStyle textStyle, float dpi, string text, float x, float y)
    {
        _drawActions.Add((canvas) =>
        {
            var paint = _paintCache.Get(textStyle, dpi);
            canvas.DrawText(text, x, y, paint);
        });
    }

    public void DrawBitmap(byte[] bytes, Rectangle rectangle)
    {
        _drawActions.Add((canvas) =>
        {
            using var stream = new MemoryStream(bytes);
            using var bitmap = SKBitmap.Decode(stream);
            if (bitmap is null)
                return;
            canvas.DrawBitmap(bitmap, rectangle);
        });
    }
}