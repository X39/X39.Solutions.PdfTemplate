using PdfSharp.Drawing;
using X39.Solutions.Papercraft.Abstraction;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Services.TextService;

namespace X39.Solutions.Papercraft.Rendering.PdfSharp.Services;

internal sealed class PdfSharpTextService : ITextLayoutService
{
    public Size Measure(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
    {
        var layout = Layout(textStyle, dpi, text, maxWidth);
        return layout.Count is 0
            ? Size.Zero
            : new Size(
                layout.Max((q) => q.Width),
                layout[^1].Top + layout[^1].Height);
    }

    public void Draw(IDrawableCanvas canvas, TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
    {
        ArgumentNullException.ThrowIfNull(canvas);
        foreach (var line in Layout(textStyle, dpi, text, maxWidth))
        {
            canvas.DrawText(textStyle, dpi, line.Text, line.X, line.BaselineY);
        }
    }

    public IReadOnlyList<TextLineLayout> Layout(TextStyle textStyle, float dpi, ReadOnlySpan<char> text, float maxWidth)
    {
        using var graphics = PdfSharpFontHelper.CreateMeasureContext();
        var font = PdfSharpFontHelper.CreateFont(textStyle, dpi);
        var fontHeight = (float) font.GetHeight();
        var baselineOffset = font.CellSpace > 0
            ? fontHeight * font.CellAscent / font.CellSpace
            : (float) PdfSharpFontHelper.GetFontSize(textStyle.FontSize, dpi);
        var lineAdvance = fontHeight * Math.Max(0.1F, textStyle.LineHeight);
        var lines = LayoutLines(graphics, font, textStyle, text, maxWidth);
        var result = new TextLineLayout[lines.Count];
        for (var i = 0; i < lines.Count; i++)
        {
            var lineTop = i * lineAdvance;
            result[i] = new TextLineLayout(
                lines[i],
                0F,
                lineTop + baselineOffset,
                lineTop,
                fontHeight,
                MeasureLine(graphics, font, textStyle, lines[i]));
        }

        return result;
    }

    private static IReadOnlyList<string> LayoutLines(
        XGraphics graphics,
        XFont font,
        TextStyle textStyle,
        ReadOnlySpan<char> text,
        float maxWidth)
    {
        var lines = new List<string>();
        foreach (var paragraph in text.ToString().Split('\n'))
        {
            lines.AddRange(WrapLine(graphics, font, textStyle, paragraph.Trim(), maxWidth));
        }

        return lines;
    }

    private static IReadOnlyList<string> WrapLine(
        XGraphics graphics,
        XFont font,
        TextStyle textStyle,
        string text,
        float maxWidth)
    {
        var lines = new List<string>();
        if (string.IsNullOrWhiteSpace(text))
            return lines;

        if (!float.IsFinite(maxWidth) || maxWidth <= 0F)
        {
            lines.Add(text);
            return lines;
        }

        var remaining = text;
        while (!string.IsNullOrWhiteSpace(remaining))
        {
            if (MeasureLine(graphics, font, textStyle, remaining) <= maxWidth)
            {
                lines.Add(remaining.Trim());
                break;
            }

            var end = FindWhitespaceBreak(graphics, font, textStyle, remaining, maxWidth);
            if (end <= 0)
                end = FindLargestPrefix(graphics, font, textStyle, remaining, maxWidth);

            lines.Add(remaining[..end].Trim());
            remaining = remaining[end..].TrimStart();
        }

        return lines;
    }

    private static int FindWhitespaceBreak(
        XGraphics graphics,
        XFont font,
        TextStyle textStyle,
        string text,
        float maxWidth)
    {
        for (var i = text.Length - 1; i > 0; i--)
        {
            if (!char.IsWhiteSpace(text[i]))
                continue;

            var candidate = text[..i].TrimEnd();
            if (candidate.Length > 0
                && MeasureLine(graphics, font, textStyle, candidate) <= maxWidth)
            {
                return i;
            }
        }

        return -1;
    }

    private static int FindLargestPrefix(
        XGraphics graphics,
        XFont font,
        TextStyle textStyle,
        string text,
        float maxWidth)
    {
        if (text.Length <= 1
            || MeasureLine(graphics, font, textStyle, text.AsSpan(0, 1)) > maxWidth)
        {
            return 1;
        }

        var low = 1;
        var high = text.Length;
        while (low < high)
        {
            var mid = (low + high + 1) / 2;
            if (MeasureLine(graphics, font, textStyle, text.AsSpan(0, mid)) <= maxWidth)
                low = mid;
            else
                high = mid - 1;
        }

        return Math.Max(1, low);
    }

    private static float MeasureLine(
        XGraphics graphics,
        XFont font,
        TextStyle textStyle,
        ReadOnlySpan<char> line)
    {
        if (line.IsEmpty)
            return 0F;

        var size = graphics.MeasureString(line.ToString(), font);
        return (float) (size.Width * PdfSharpFontHelper.GetHorizontalScale(textStyle.Scale));
    }
}
