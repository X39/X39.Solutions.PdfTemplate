using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Data;
using X39.Util;

namespace X39.Solutions.PdfTemplate.Test.Mock;

public partial class CanvasMock : ICanvas
{
    private record struct DrawTextCall(
        TextStyle TextStyle,
        string Text,
        float X,
        float Y);

    private record struct DrawLineCall(
        Color Color,
        float Thickness,
        float StartX,
        float StartY,
        float EndX,
        float EndY)
    {
        public override string ToString()
        {
            return
                $"{nameof(DrawLineCall)} {{ {Color}, {nameof(Thickness)} = {Thickness}, {nameof(StartX)} = {StartX}, {nameof(StartY)} = {StartY}, {nameof(EndX)} = {EndX}, {nameof(EndY)} = {EndY} }}";
        }
    }

    private class State
    {
        public Point Translation { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Rectangle Clip { get; set; }
    }

    private readonly Stack<State>       _stateStack    = new(new State().MakeEnumerable());
    private readonly List<DrawLineCall> _drawLineCalls = new();
    private readonly List<DrawTextCall> _drawTextCalls = new();

    public void PushState()
    {
        var previousTranslation = _stateStack.Any() ? _stateStack.Peek().Translation : new Point();
        _stateStack.Push(
            new State
            {
                Clip        = Rectangle.MaxValue,
                Translation = previousTranslation,
            });
    }

    public void Clip(Rectangle rectangle)
    {
        _stateStack.Peek().Clip = rectangle;
    }

    public void PopState()
    {
        _stateStack.Pop();
    }

    public void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
    {
        var translation = _stateStack.Any() ? _stateStack.Peek().Translation : new Point();
        _drawLineCalls.Add(
            new DrawLineCall(
                color,
                thickness,
                startX + translation.X,
                startY + translation.Y,
                endX + translation.X,
                endY + translation.Y));
    }

    public void Translate(Point point)
    {
        _stateStack.Peek().Translation += point;
    }

    public void DrawText(TextStyle textStyle, string text, float x, float y)
    {
        _drawTextCalls.Add(
            new DrawTextCall(
                textStyle,
                text,
                x,
                y));
    }

    public void DrawRect(Rectangle rectangle, Color color)
    {
    }
}

public partial class CanvasMock
{
    [StackTraceHidden]
    public void AssertState()
    {
        Assert.Single(_stateStack);
    }

    [StackTraceHidden]
    public void AssertDrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
    {
        var actual = _drawLineCalls.FirstOrDefault();
        var expected = new DrawLineCall(color, thickness, startX, startY, endX, endY);
        Assert.Equal(expected, actual);
    }

    [StackTraceHidden]
    public void AssertDrawLine(
        params (Color color, float thickness, float startX, float startY, float endX, float endY)[] drawLineCalls)
    {
        Assert.Equal(drawLineCalls.Length, _drawLineCalls.Count);
        var zipped = _drawLineCalls.Zip(drawLineCalls);
        foreach (var (actual, callExpected) in zipped)
        {
            var expected = new DrawLineCall(
                callExpected.color,
                callExpected.thickness,
                callExpected.startX,
                callExpected.startY,
                callExpected.endX,
                callExpected.endY);
            Assert.Equal(expected, actual);
        }
    }

    [StackTraceHidden]
    public void AssertDrawText(TextStyle textStyle, string text, float x, float y)
    {
        Assert.NotEmpty(_drawTextCalls);
        var actual = _drawTextCalls.FirstOrDefault();
        var expected = new DrawTextCall(textStyle, text, x, y);
        Assert.Equal(expected, actual);
    }

    [StackTraceHidden]
    public void AssertDrawText(params (TextStyle textStyle, string text, float x, float y)[] drawTextCalls)
    {
        Assert.Equal(drawTextCalls.Length, _drawTextCalls.Count);
        var zipped = _drawTextCalls.Zip(drawTextCalls);
        foreach (var (actual, callExpected) in zipped)
        {
            var expected = new DrawTextCall(callExpected.textStyle, callExpected.text, callExpected.x, callExpected.y);
            Assert.Equal(expected, actual);
        }
    }
}