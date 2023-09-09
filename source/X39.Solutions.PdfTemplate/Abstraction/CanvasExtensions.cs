using X39.Solutions.PdfTemplate.Data;

namespace X39.Solutions.PdfTemplate.Abstraction;

/// <summary>
/// Contains extension methods for <see cref="ICanvas"/>.
/// </summary>
public static class CanvasExtensions
{
    /// <inheritdoc cref="ICanvas.Translate"/>
    /// <param name="self">The <see cref="ICanvas"/> to operate on.</param>
    /// <param name="x">The distance to translate along the x-axis.</param>
    /// <param name="y">The distance to translate along the y-axis.</param>
    public static void Translate(this ICanvas self, float x, float y)
    {
        self.Translate(new Point(x, y));
    }
    
    /// <inheritdoc cref="ICanvas.Clip"/>
    /// <param name="self">The <see cref="ICanvas"/> to operate on.</param>
    /// <param name="x">The x-coordinate of the rectangle.</param>
    /// <param name="y">The y-coordinate of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public static void Clip(this ICanvas self, float x, float y, float width, float height)
    {
        self.Clip(new Rectangle(x, y, width, height));
    }
}