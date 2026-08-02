using PdfSharp.Drawing;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;

namespace X39.Solutions.Papercraft.Rendering.PdfSharp.Services;

internal static class PdfSharpFontHelper
{
    public static XFont CreateFont(TextStyle textStyle, float dpi)
    {
        PdfSharpFontResolverRegistration.EnsureConfigured();
        return new XFont(
            NormalizeFontFamily(textStyle.FontFamily.Family),
            GetFontSize(textStyle.FontSize, dpi),
            ToFontStyle(
                textStyle.FontFamily.Weight,
                textStyle.FontFamily.Style is EFontStyle.Italic or EFontStyle.Oblique,
                textStyle.Decoration));
    }

    public static XFont CreateFont(DisplayTextStyle textStyle, float dpi)
    {
        PdfSharpFontResolverRegistration.EnsureConfigured();
        return new XFont(
            NormalizeFontFamily(textStyle.FontFamily.Family),
            GetFontSize(textStyle.FontSize, dpi),
            ToFontStyle(
                textStyle.FontFamily.Weight,
                textStyle.FontFamily.Style is DisplayFontStyle.Italic or DisplayFontStyle.Oblique,
                textStyle.Decoration));
    }

    public static double GetFontSize(float fontSize, float dpi)
    {
        var normalized = dpi > 0F
            ? fontSize * dpi / 72.272F
            : fontSize;
        return Math.Max(1D, normalized);
    }

    public static double GetHorizontalScale(float scale)
        => Math.Abs(scale);

    public static XGraphics CreateMeasureContext()
        => XGraphics.CreateMeasureContext(
            new XSize(1D, 1D),
            XGraphicsUnit.Point,
            XPageDirection.Downwards);

    private static XFontStyleEx ToFontStyle(ushort weight, bool italic, TextDecoration decoration)
    {
        var style = XFontStyleEx.Regular;
        if (weight >= 600)
            style |= XFontStyleEx.Bold;
        if (italic)
            style |= XFontStyleEx.Italic;
        if (decoration.HasFlag(TextDecoration.Underline)
            || decoration.HasFlag(TextDecoration.DoubleUnderline))
            style |= XFontStyleEx.Underline;
        if (decoration.HasFlag(TextDecoration.StrikeThrough))
            style |= XFontStyleEx.Strikeout;
        return style;
    }

    private static string NormalizeFontFamily(string family)
        => string.IsNullOrWhiteSpace(family)
            ? PdfSharpSystemFontResolver.GenericSansSerifFamily
            : family;
}
