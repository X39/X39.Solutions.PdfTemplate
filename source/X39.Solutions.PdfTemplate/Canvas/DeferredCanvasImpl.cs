using SkiaSharp;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Canvas;

internal sealed class DeferredCanvasImpl : IDeferredCanvas
{
    private readonly List<Action<IImmediateCanvas>> _drawActions = new();

    // We want to allow ~20 levels of nesting without a reallocation.
    private const    int          DefaultStackCapacity = 20 + 1;
    private readonly Stack<Point> _stateStack          = new(DefaultStackCapacity);
    public           Point        Translation                            => _stateStack.Count is not 0 ? _stateStack.Peek() : new Point();
    public Size ActualPageSize { get; set; }
    public Size PageSize { get; set; }

    public void Defer(Action<IImmediateCanvas> action)
    {
        _drawActions.Add(action);
    }

    internal void Render(IImmediateCanvas canvas)
    {
        foreach (var action in _drawActions)
        {
            action(canvas);
        }
    }

    public void PushState()
    {
        _stateStack.Push(Translation);
        _drawActions.Add((canvas) => canvas.PushState());
    }

    public void Clip(Rectangle rectangle)
    {
        _drawActions.Add((canvas) => canvas.Clip(rectangle));
    }

    public void DrawRect(Rectangle rectangle, Color color)
    {
        if (color.Alpha is 0)
            return;
        _drawActions.Add((canvas) => canvas.DrawRect(rectangle, color));
    }

    public void Translate(Point point)
    {
        if (point is { X: 0, Y: 0 })
            return;
        var translation = Translation + point;
        _stateStack.Pop();
        _stateStack.Push(translation);
        _drawActions.Add((canvas) => canvas.Translate(point));
    }

    public void PopState()
    {
        _stateStack.Pop();
        _drawActions.Add((canvas) => canvas.PopState());
    }

    public void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
    {
        if (color.Alpha is 0)
            return;
        _drawActions.Add((canvas) =>canvas.DrawLine(color, thickness, startX, startY, endX, endY));
    }

    public void DrawText(TextStyle textStyle, float dpi, string text, float x, float y)
    {
        if (text.IsNullOrWhiteSpace())
            return;
        _drawActions.Add((canvas) => canvas.DrawText(textStyle, dpi, text, x, y));
    }

    public void DrawBitmap(byte[] bytes, Rectangle rectangle)
    {
        _drawActions.Add((canvas) => canvas.DrawBitmap(bytes, rectangle));
    }

    public void DrawBitmap(SKBitmap bitmap, Rectangle rectangle)
    {
        _drawActions.Add((canvas) => canvas.DrawBitmap(bitmap, rectangle));
    }
    
}