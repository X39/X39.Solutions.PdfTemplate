using System.Diagnostics;
using System.Runtime.CompilerServices;
using SkiaSharp;
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


    private Point Translation => _stateStack.Any() ? _stateStack.Peek().Translation : new Point();

    private readonly Stack<State>       _stateStack    = new(new State().MakeEnumerable());
    private readonly List<DrawLineCall> _drawLineCalls = new();
    private readonly List<DrawTextCall> _drawTextCalls = new();
    private readonly List<Rectangle>    _clipCalls     = new();

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
        rectangle += Translation;
        _clipCalls.Add(rectangle);
        _stateStack.Peek().Clip = rectangle;
    }

    public void PopState()
    {
        _stateStack.Pop();
    }

    public void DrawLine(Color color, float thickness, float startX, float startY, float endX, float endY)
    {
        _drawLineCalls.Add(
            new DrawLineCall(
                color,
                thickness,
                startX + Translation.X,
                startY + Translation.Y,
                endX + Translation.X,
                endY + Translation.Y));
    }

    public void Translate(Point point)
    {
        _stateStack.Peek().Translation += point;
    }

    public void DrawText(TextStyle textStyle, float dpi, string text, float x, float y)
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

    public void DrawBitmap(byte[] bitmap, Rectangle rectangle)
    {
    }
    public void DrawBitmap(SKBitmap bitmap, Rectangle rectangle)
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

    [StackTraceHidden]
    public void AssertClip(Rectangle rectangle)
    {
        Assert.NotEmpty(_clipCalls);
        var actual = _clipCalls.First();
        var expected = rectangle;
        Assert.Equal(expected, actual);
    }

    [StackTraceHidden]
    public void AssertClip(int index, Rectangle rectangle, bool withTranslation = true)
    {
        Assert.NotEmpty(_clipCalls);
        Assert.True(_clipCalls.Count > index, $"The assertion failed because the amount of clip calls is less than {index}.");
        var actual = _clipCalls.ElementAt(index);
        var expected = rectangle;
        if (withTranslation)
            Assert.Equal(expected, actual);
        else
            Assert.Equal<Size>(expected, actual);
    }

    [StackTraceHidden]
    public void AssertClip(params Rectangle[] clipCalls)
    {
        Assert.Equal(clipCalls.Length, _clipCalls.Count);
        var zipped = _clipCalls.Zip(clipCalls);
        foreach (var (actual, expected) in zipped)
        {
            Assert.Equal(expected, actual);
        }
    }

    [StackTraceHidden]
    public void AssertAllClip(
        Func<Rectangle, bool> predicate,
        [CallerArgumentExpression(nameof(predicate))] string expression = "")
    {
        Assert.NotEmpty(_clipCalls);
        Assert.True(_clipCalls.All(predicate), $"The predicate {expression} was not true for all clip calls.");
    }
}