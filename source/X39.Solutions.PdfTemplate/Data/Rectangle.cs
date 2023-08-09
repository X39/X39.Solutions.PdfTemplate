using System.Numerics;
using SkiaSharp;

namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// A rectangle with a position and a size
/// </summary>
/// <param name="Left">The left position of the rectangle</param>
/// <param name="Top">The top position of the rectangle</param>
/// <param name="Width">The width of the rectangle</param>
/// <param name="Height">The height of the rectangle</param>
public readonly record struct Rectangle(float Left, float Top, float Width, float Height)
    : IAdditionOperators<Rectangle, Rectangle, Rectangle>,
        IAdditiveIdentity<Rectangle, Rectangle>,
        IDecrementOperators<Rectangle>,
        IDivisionOperators<Rectangle, Rectangle, Rectangle>,
        IEqualityOperators<Rectangle, Rectangle, bool>,
        IIncrementOperators<Rectangle>,
        IMultiplicativeIdentity<Rectangle, Rectangle>,
        IMultiplyOperators<Rectangle, Rectangle, Rectangle>,
        ISubtractionOperators<Rectangle, Rectangle, Rectangle>,
        IUnaryPlusOperators<Rectangle, Rectangle>,
        IUnaryNegationOperators<Rectangle, Rectangle>,
        IMinMaxValue<Rectangle>
{
    /// <summary>
    /// The bottom position of the rectangle
    /// </summary>
    public float Bottom => Top + Height;

    /// <summary>
    /// The right position of the rectangle
    /// </summary>
    public float Right => Left + Width;

    /// <inheritdoc />
    public static Rectangle operator +(Rectangle left, Rectangle right)
        => new(left.Left + right.Left, left.Top + right.Top, left.Width + right.Width, left.Height + right.Height);

    /// <inheritdoc />
    public static Rectangle AdditiveIdentity => new(0, 0, 0, 0);

    /// <inheritdoc />
    public static Rectangle operator --(Rectangle value)
        => new(value.Left - 1, value.Top - 1, value.Width - 1, value.Height - 1);

    /// <inheritdoc />
    public static Rectangle operator /(Rectangle left, Rectangle right)
        => new(left.Left / right.Left, left.Top / right.Top, left.Width / right.Width, left.Height / right.Height);

    /// <inheritdoc />
    public static Rectangle operator ++(Rectangle value)
        => new(value.Left + 1, value.Top + 1, value.Width + 1, value.Height + 1);

    /// <inheritdoc />
    public static Rectangle MultiplicativeIdentity => new(1, 1, 1, 1);

    /// <inheritdoc />
    public static Rectangle operator *(Rectangle left, Rectangle right)
        => new(left.Left * right.Left, left.Top * right.Top, left.Width * right.Width, left.Height * right.Height);

    /// <inheritdoc />
    public static Rectangle operator -(Rectangle left, Rectangle right)
        => new(left.Left - right.Left, left.Top - right.Top, left.Width - right.Width, left.Height - right.Height);

    /// <inheritdoc />
    public static Rectangle operator +(Rectangle value)
        => new(+value.Left, +value.Top, +value.Width, +value.Height);

    /// <inheritdoc />
    public static Rectangle operator -(Rectangle value)
        => new(-value.Left, -value.Top, -value.Width, -value.Height);

    /// <inheritdoc />
    public static Rectangle MaxValue => new(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);
    /// <inheritdoc />
    public static Rectangle MinValue => new(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
    
    /// <summary>
    /// Implicitly convert a <see cref="Rectangle"/> to a <see cref="SKRect"/>
    /// </summary>
    /// <param name="rectangle">The rectangle to convert</param>
    /// <returns>The converted rectangle</returns>
    public static implicit operator SKRect(Rectangle rectangle)
        => new(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
    
    /// <summary>
    /// Implicitly convert a <see cref="SKRect"/> to a <see cref="Rectangle"/>
    /// </summary>
    /// <param name="rectangle">The rectangle to convert</param>
    /// <returns>The converted rectangle</returns>
    public static implicit operator Rectangle(SKRect rectangle)
        => new(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
    
    /// <summary>
    /// Implicitly convert a <see cref="Rectangle"/> to a <see cref="Size"/>
    /// </summary>
    /// <param name="rectangle">The rectangle to convert</param>
    /// <returns>The converted size</returns>
    public static implicit operator Size(Rectangle rectangle)
        => new(rectangle.Width, rectangle.Height);
    
    /// <summary>
    /// Implicitly convert a <see cref="Rectangle"/> to a <see cref="Point"/>
    /// </summary>
    /// <param name="rectangle">The rectangle to convert</param>
    /// <returns>The converted point</returns>
    public static implicit operator Point(Rectangle rectangle)
        => new(rectangle.Left, rectangle.Top);
}