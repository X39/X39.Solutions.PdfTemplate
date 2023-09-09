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

    public Size Measure(TextStyle textStyle, ReadOnlySpan<char> text, float maxWidth)
    {
        var paint = _paintCache.Get(textStyle);
        return maxWidth < paint.FontMetrics.MaxCharacterWidth
            ? new Size(float.NaN, float.NaN)
            : SplatterAndMeasureLines(textStyle, paint, text, maxWidth);
    }

    private static Size SplatterAndMeasureLines(
        TextStyle textStyle,
        SKPaint skPaint,
        ReadOnlySpan<char> text,
        float maxWidth)
    {
        Size size;
        var start = 0;
        var rOffset = 0;
        var maxLineLength = 0F;
        var lines = 1;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            switch (c)
            {
                case '\r':
                    rOffset = -1;
                    break;
                case '\n':
                    var end = i + rOffset;
                    size = Measure(skPaint, text[start..end]);
                    // Backtrack if the line is too long
                    while (size.Width > maxWidth)
                    {
                        for (i--; i > start; i--)
                        {
                            if (!text[i].IsWhiteSpace())
                                continue;
                            end = i;
                            break;
                        }

                        size = Measure(skPaint, text[start..end]);
                    }

                    rOffset       = 0;
                    maxLineLength = Math.Max(maxLineLength, size.Width);
                    start         = i + 1;
                    lines++;
                    break;
            }
        }

        size          = Measure(skPaint, text[start..]);
        maxLineLength = Math.Max(maxLineLength, size.Width);
        return new Size(maxLineLength, size.Height + (lines - 1) * (size.Height * textStyle.LineHeight));
    }

    private static Size Measure(SKPaint skPaint, ReadOnlySpan<char> readOnlySpan)
    {
        var width = skPaint.MeasureText(readOnlySpan);
        return new Size(width, skPaint.FontMetrics.Bottom + -skPaint.FontMetrics.Top);
    }


    public void Draw(ICanvas canvas, TextStyle textStyle, ReadOnlySpan<char> text, float maxWidth)
    {
        var paint = _paintCache.Get(textStyle);
        if (maxWidth < paint.FontMetrics.MaxCharacterWidth)
            return;
        RenderLines(canvas, textStyle, paint, text, maxWidth);
    }
    
    private static void RenderLines(
        ICanvas canvas,
        TextStyle textStyle,
        SKPaint skPaint,
        ReadOnlySpan<char> text,
        float maxWidth)
    {
        var start = 0;
        var rOffset = 0;
        var y = 0F;
        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];
            switch (c)
            {
                case '\r':
                    rOffset = -1;
                    break;
                case '\n':
                    var end = i + rOffset;
                    var size = Measure(skPaint, text[start..end]);
                    // Backtrack if the line is too long
                    while (size.Width > maxWidth)
                    {
                        for (i--; i > start; i--)
                        {
                            if (!text[i].IsWhiteSpace())
                                continue;
                            end = i;
                            break;
                        }

                        size = Measure(skPaint, text[start..end]);
                    }
                    canvas.DrawText(textStyle, text[start..end].ToString(), 0, y);
                    
                    rOffset = 0;
                    start   = i + 1;
                    y += size.Height * textStyle.LineHeight;
                    break;
            }
        }
    }
}