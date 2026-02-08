using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using SkiaSharp;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Data;
using X39.Util;

namespace X39.Solutions.PdfTemplate.Test.Mock;

public partial class DeferredCanvasMock : IDeferredCanvas, IImmediateCanvas
{
    [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local", Justification = "This is a preparation for future use. Having unused properties is expected hence.")]
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

    private record struct DrawRectCall(
        Rectangle Rectangle,
        Color Color)
    {
        public override string ToString()
        {
            return
                $"{nameof(DrawRectCall)} {{ {Rectangle}, {Color} }}";
        }
    }

    private class State
    {
        public Point Translation { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Rectangle Clip { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Rectangle Unclip { get; set; }
    }


    public Size ActualPageSize { get; set; }
    public Size PageSize { get; set; }
    public Point Translation                            => _stateStack.Any() ? _stateStack.Peek().Translation : new Point();
    public void  Defer(Action<IImmediateCanvas> action) { action(this); }

    public int DrawLineCount => _drawLineCalls.Count;
    public int DrawTextCount => _drawTextCalls.Count;
    public int DrawRectCount => _drawRectCalls.Count;
    public int ClipCount     => _clipCalls.Count;

    private readonly Stack<State>       _stateStack    = new(new State().MakeEnumerable());
    private readonly List<DrawLineCall> _drawLineCalls = new();
    private readonly List<DrawTextCall> _drawTextCalls = new();
    private readonly List<DrawRectCall> _drawRectCalls = new();
    private readonly List<Rectangle>    _clipCalls     = new();
    private readonly List<Rectangle>    _unclipCalls     = new();

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

    public void Unclip(Rectangle rectangle)
    {
        rectangle += Translation;
        _unclipCalls.Add(rectangle);
        _stateStack.Peek().Unclip = rectangle;
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
                x + Translation.X,
                y + Translation.Y));
    }

    public void DrawRect(Rectangle rectangle, Color color)
    {
        rectangle += Translation;
        _drawRectCalls.Add(new DrawRectCall(rectangle, color));
    }

    public void DrawBitmap(byte[] bitmap, Rectangle rectangle)
    {
    }
    public void DrawBitmap(SKBitmap bitmap, Rectangle rectangle)
    {
    }

    public void   AddBreakPageHeight(float additionalPageHeight) {  }
    public ushort EstimatedPageCount                             { get; set; }
    public ushort PageNumber                                     { get; set; }
    public ushort TotalPages                                     { get; set; }
}

public partial class DeferredCanvasMock
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

    /// <summary>
    /// Asserts all DrawLine calls have coordinates within the given bounds (with tolerance).
    /// </summary>
    [StackTraceHidden]
    public void AssertAllDrawLinesWithin(Rectangle bounds, float tolerance = 1f)
    {
        var minX = bounds.Left - tolerance;
        var minY = bounds.Top - tolerance;
        var maxX = bounds.Right + tolerance;
        var maxY = bounds.Bottom + tolerance;
        for (var i = 0; i < _drawLineCalls.Count; i++)
        {
            var call = _drawLineCalls[i];
            Assert.True(
                call.StartX >= minX && call.StartX <= maxX &&
                call.StartY >= minY && call.StartY <= maxY &&
                call.EndX >= minX && call.EndX <= maxX &&
                call.EndY >= minY && call.EndY <= maxY,
                $"DrawLine call #{i} is out of bounds {bounds}: {call}");
        }
    }

    /// <summary>
    /// Asserts all DrawText calls have positions within the given bounds (with tolerance).
    /// </summary>
    [StackTraceHidden]
    public void AssertAllDrawTextWithin(Rectangle bounds, float tolerance = 1f)
    {
        var minX = bounds.Left - tolerance;
        var minY = bounds.Top - tolerance;
        var maxX = bounds.Right + tolerance;
        var maxY = bounds.Bottom + tolerance;
        for (var i = 0; i < _drawTextCalls.Count; i++)
        {
            var call = _drawTextCalls[i];
            Assert.True(
                call.X >= minX && call.X <= maxX &&
                call.Y >= minY && call.Y <= maxY,
                $"DrawText call #{i} ('{call.Text}') position ({call.X}, {call.Y}) is out of bounds {bounds}");
        }
    }

    /// <summary>
    /// Asserts all DrawRect calls are within the given bounds (with tolerance).
    /// </summary>
    [StackTraceHidden]
    public void AssertAllDrawRectsWithin(Rectangle bounds, float tolerance = 1f)
    {
        var minX = bounds.Left - tolerance;
        var minY = bounds.Top - tolerance;
        var maxX = bounds.Right + tolerance;
        var maxY = bounds.Bottom + tolerance;
        for (var i = 0; i < _drawRectCalls.Count; i++)
        {
            var call = _drawRectCalls[i];
            Assert.True(
                call.Rectangle.Left >= minX && call.Rectangle.Right <= maxX &&
                call.Rectangle.Top >= minY && call.Rectangle.Bottom <= maxY,
                $"DrawRect call #{i} {call.Rectangle} is out of bounds {bounds}");
        }
    }

    /// <summary>
    /// Asserts that at least the given number of DrawLine calls were made.
    /// </summary>
    [StackTraceHidden]
    public void AssertDrawLineCountAtLeast(int count)
    {
        Assert.True(_drawLineCalls.Count >= count,
            $"Expected at least {count} DrawLine calls, but got {_drawLineCalls.Count}.");
    }

    /// <summary>
    /// Asserts that at least the given number of DrawText calls were made.
    /// </summary>
    [StackTraceHidden]
    public void AssertDrawTextCountAtLeast(int count)
    {
        Assert.True(_drawTextCalls.Count >= count,
            $"Expected at least {count} DrawText calls, but got {_drawTextCalls.Count}.");
    }

    /// <summary>
    /// Asserts that at least the given number of DrawRect calls were made.
    /// </summary>
    [StackTraceHidden]
    public void AssertDrawRectCountAtLeast(int count)
    {
        Assert.True(_drawRectCalls.Count >= count,
            $"Expected at least {count} DrawRect calls, but got {_drawRectCalls.Count}.");
    }

    /// <summary>
    /// Asserts any DrawText call contains the specified text substring.
    /// </summary>
    [StackTraceHidden]
    public void AssertAnyDrawTextContains(string text)
    {
        Assert.True(
            _drawTextCalls.Any(c => c.Text.Contains(text, StringComparison.Ordinal)),
            $"No DrawText call contains '{text}'. Calls: [{string.Join(", ", _drawTextCalls.Select(c => $"'{c.Text}'"))}]");
    }

    /// <summary>
    /// Asserts no DrawText call contains the specified text substring.
    /// </summary>
    [StackTraceHidden]
    public void AssertNoDrawTextContains(string text)
    {
        Assert.True(
            !_drawTextCalls.Any(c => c.Text.Contains(text, StringComparison.Ordinal)),
            $"DrawText call unexpectedly contains '{text}'.");
    }

    /// <summary>
    /// Asserts any DrawLine call uses the specified color.
    /// </summary>
    [StackTraceHidden]
    public void AssertAnyDrawLineWithColor(Color color)
    {
        Assert.True(
            _drawLineCalls.Any(c => c.Color == color),
            $"No DrawLine call uses color {color}.");
    }

    /// <summary>
    /// Asserts any DrawRect call uses the specified color.
    /// </summary>
    [StackTraceHidden]
    public void AssertAnyDrawRectWithColor(Color color)
    {
        Assert.True(
            _drawRectCalls.Any(c => c.Color == color),
            $"No DrawRect call uses color {color}.");
    }
}