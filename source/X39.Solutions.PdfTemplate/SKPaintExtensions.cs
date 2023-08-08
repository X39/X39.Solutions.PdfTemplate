using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate;

public static class SKPaintExtensions
{
    public static unsafe Rectangle MeasureText(this SkiaSharp.SKPaint paint, string text, float maxWidth)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return new Rectangle();
        }

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
            {
                // Increase startIndex by one due to i increasing after this
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
            {
                // Increase startIndex by one due to i increasing after this
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
        var width = 0.0F;
        var height = 0.0F;
        var localWidth = 0F;
        for (var i = 0; i < wordCount; i++)
        {
            var tuple = spans[i];
            var tupleWidth = tuple.Measure + spaceWidth * tuple.LeadingSpaces;
            var tmp = localWidth + tupleWidth;
            if (tmp > maxWidth)
            {
                if (localWidth == 0)
                {
                    width      = maxWidth;
                    localWidth = maxWidth;
                }
                else
                {
                    i--;
                    width      =  Math.Max(width, localWidth);
                    height     += paint.FontSpacing;
                    localWidth =  0;
                }
            }
            else
            {
                localWidth = tmp;
            }
        }

        width  =  Math.Max(width, localWidth);
        height += paint.FontSpacing;
        return new Rectangle(0, 0, width, height);
    }
}