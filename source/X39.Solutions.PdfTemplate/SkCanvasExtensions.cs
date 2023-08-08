namespace X39.Solutions.PdfTemplate;

public static class SkCanvasExtensions
{
    public static unsafe void DrawText(this SkiaSharp.SKCanvas canvas, SkiaSharp.SKPaint paint, string text, float x, float y, float maxWidth, float maxHeight)
    {
        if (string.IsNullOrWhiteSpace(text)) { return; }
        var totalSpan = text.AsSpan();
        var startIndex = 0;
        var wordCount = 0;
        // Count all words
        for (var i = 0; i < totalSpan.Length; i++)
        {
            var c = totalSpan[i];
            if (!c.IsWhiteSpace())
                continue;
            if (i != startIndex)
            {
                wordCount++;
                startIndex = i + 1;
            }
            else
            { // Increase startIndex by one due to i increasing after this
                startIndex++;
            }
        }
        if (startIndex != totalSpan.Length)
        {
            wordCount++;
        }
        // Measure all words
        var spans = stackalloc (int StartIndex, int EndIndex, float Measure, int LeadingSpaces)[wordCount];
        startIndex = 0;
        var spaceCount = 0;
        var spansIndex = 0;
        var spaceWidth = paint.MeasureText(" ");
        for (var i = 0; i < totalSpan.Length; i++)
        {
            var c = totalSpan[i];
            if (!c.IsWhiteSpace())
                continue;
            if (i != startIndex)
            {
                var span = totalSpan[startIndex..i];
                spans[spansIndex++] = (startIndex, i, paint.MeasureText(span), spaceCount);
                startIndex          = i + 1;
                spaceCount          = 1;
            }
            else
            { // Increase startIndex by one due to i increasing after this
                spaceCount++;
                startIndex++;
            }
        }
        if (startIndex != totalSpan.Length)
        {
            var span = totalSpan[startIndex..totalSpan.Length];
            spans[spansIndex++] = (startIndex, totalSpan.Length, paint.MeasureText(span), spaceCount);
        }

        // Count until maxWidth is satisfied for each line
        var locX = x;
        var locY = y;
        var lineHeight = paint.FontSpacing;
        var baselineOffset = -paint.FontMetrics.Ascent;
        for (var i = 0; i < wordCount && (locY - y) < maxHeight; i++)
        {
            var tuple = spans[i];
            var tupleWidth = tuple.Measure + spaceWidth * tuple.LeadingSpaces;
            var tmp = (locX - x) + tupleWidth;
            if (tmp > maxWidth)
            {
                if (Math.Abs(locX - x) < float.Epsilon)
                {
                    canvas.Save();
                    canvas.ClipRect(new SkiaSharp.SKRect(locX, locY, locX + maxWidth, locY + baselineOffset + lineHeight));
                    canvas.DrawText(text[tuple.StartIndex..tuple.EndIndex], locX, locY + baselineOffset, paint);
                    canvas.Restore();
                    locX =  x;
                    locY += lineHeight;
                }
                else
                {
                    i--;
                    locX =  x;
                    locY += lineHeight;
                }
            }
            else
            {
                canvas.DrawText(text[tuple.StartIndex..tuple.EndIndex], locX + spaceWidth * tuple.LeadingSpaces, locY + baselineOffset, paint);
                locX += tupleWidth;
            }
        }
    }
}