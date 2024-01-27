using SkiaSharp;
using X39.Solutions.PdfTemplate.Data;
using X39.Solutions.PdfTemplate.Services;

namespace X39.Solutions.PdfTemplate.Abstraction;

internal sealed class CanvasImpl : ICanvas
{
    private readonly SkPaintCache           _paintCache;
    private readonly List<Action<SKCanvas>> _drawActions = new();

    // We want to allow ~20 levels of nesting without a reallocation.
    private const    int          DefaultStackCapacity = 20 + 1;
    private readonly Stack<Point> _stateStack          = new(DefaultStackCapacity);
    public           Point        Translation => _stateStack.Count is not 0 ? _stateStack.Peek() : new Point();

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
        _stateStack.Push(Translation);
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
        var translation = Translation + point;
        _stateStack.Pop();
        _stateStack.Push(translation);
        _drawActions.Add((canvas) => canvas.Translate(point));
    }

    public void PopState()
    {
        _stateStack.Pop();
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

    public void DrawBitmap(SKBitmap bitmap, Rectangle rectangle)
    {
        _drawActions.Add((canvas) =>
        {
            canvas.DrawBitmap(bitmap, rectangle);
        });
    }
}