using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Services.TextService;

/// <summary>
/// Service abstraction for measuring text.
/// </summary>
public interface ITextService
{
    /// <summary>
    /// Measures the given <paramref name="text"/> with the given <paramref name="textStyle"/>.
    /// </summary>
    /// <param name="textStyle">The text style to use.</param>
    /// <param name="text">The text to measure.</param>
    /// <param name="maxWidth">
    /// The maximum width of a single line.
    /// If the width of the widest character is larger than this value,
    /// the returned <see cref="Size"/> will have both elements being <see cref="float.NaN"/>!
    /// </param>
    /// <returns>
    /// The corresponding <see cref="Size"/> or
    /// a <see cref="Size"/> with both elements being <see cref="float.NaN"/>
    /// if <paramref name="maxWidth"/> is smaller than the width of the widest character.
    /// </returns>
    Size Measure(TextStyle textStyle, ReadOnlySpan<char> text, float maxWidth);

    /// <summary>
    /// Renders the given <paramref name="text"/> with the given <paramref name="textStyle"/>.
    /// </summary>
    /// <param name="canvas">The canvas to render on.</param>
    /// <param name="textStyle">The text style to use.</param>
    /// <param name="text">The text to render.</param>
    /// <param name="maxWidth">The maximum width of a single line.</param>
    void Draw(ICanvas canvas, TextStyle textStyle, ReadOnlySpan<char> text, float maxWidth);
}