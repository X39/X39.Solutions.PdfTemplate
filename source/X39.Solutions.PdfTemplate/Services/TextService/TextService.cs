using SkiaSharp;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Services.TextService;

internal class TextService : ITextService
{
    private readonly SkPaintCache _paintCache;
    public TextService(SkPaintCache paintCache)
    {
        _paintCache = paintCache;
    }
    private ref struct ReadOnlySpanPair<T>
    {
        public ReadOnlySpan<T> Left;
        public ReadOnlySpan<T> Right;

        public ReadOnlySpanPair(ReadOnlySpan<T> left, ReadOnlySpan<T> right)
        {
            Left  = left;
            Right = right;
        }

        public void Deconstruct(out ReadOnlySpan<T> left, out ReadOnlySpan<T> right)
        {
            left  = Left;
            right = Right;
        }
    }

    public Size Measure(TextStyle textStyle, ReadOnlySpan<char> text, float maxWidth)
    {
        var skPaint = _paintCache.Get(textStyle);
        var lines = 0;
        var resultWidth = 0F;
        var right = text;
        var left = text;
        while (!right.IsEmpty && !left.IsEmpty)
        {
            var (fullLine, remainder) = NextLine(right);
            var trimmedFullLine                  = fullLine.TrimStart();
            if (fullLine.IsEmpty && !remainder.IsEmpty)
            { // new line character
                right = remainder;
                continue;
            }
            var (divided, _) = DivideAndConquer(trimmedFullLine, skPaint, maxWidth, out var width);
            left             = divided;
            right            = right[(left.Length + fullLine.Length - trimmedFullLine.Length)..];
            lines++;
            resultWidth = Math.Max(resultWidth, width);
        }
        var height = skPaint.FontMetrics.Bottom + -skPaint.FontMetrics.Top;
        return new Size(resultWidth, height + (lines - 1) * (height * textStyle.LineHeight));
    }
    private static ReadOnlySpanPair<char> NextLine(ReadOnlySpan<char> text)
    {
        var index = text.IndexOf('\n');
        return index == -1
            ? new ReadOnlySpanPair<char>(text, ReadOnlySpan<char>.Empty)
            : new ReadOnlySpanPair<char>(text[..index], text[(index + 1)..]);
    }

    private static ReadOnlySpanPair<char> DivideAndConquer(ReadOnlySpan<char> text, SKPaint skPaint, float maxWidth, out float leftWidth)
    {
        const int start = 0;
        var end = text.Length;
        leftWidth = skPaint.MeasureText(text);
        while (leftWidth > maxWidth)
        {
            for (end--; end > start; end--)
            {
                if (!text[end].IsWhiteSpace())
                    continue;
                break;
            }

            leftWidth = skPaint.MeasureText(text[..end]);
        }
        return new ReadOnlySpanPair<char>(text[..end], text[end..]);
    }

    public void Draw(ICanvas canvas, TextStyle textStyle, ReadOnlySpan<char> text, float maxWidth)
    {
        var skPaint = _paintCache.Get(textStyle);
        var height = skPaint.FontMetrics.Bottom + -skPaint.FontMetrics.Top;
        var right = text;
        var left = text;
        var y = 0F;
        while (!right.IsEmpty && !left.IsEmpty)
        {
            var (fullLine, remainder) = NextLine(right);
            var trimmedFullLine                  = fullLine.TrimStart();
            if (fullLine.IsEmpty && !remainder.IsEmpty)
            { // new line character
                right = remainder;
                continue;
            }
            var (divided, _) = DivideAndConquer(trimmedFullLine, skPaint, maxWidth, out _);
            left             = divided;
            right            = right[(left.Length + fullLine.Length - trimmedFullLine.Length)..];
            canvas.DrawText(textStyle, left.ToString(), 0, y - skPaint.FontMetrics.Ascent);
            y += height * textStyle.LineHeight;
        }
    }
}