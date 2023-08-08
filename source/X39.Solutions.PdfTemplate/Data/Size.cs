using System.Numerics;
using SkiaSharp;

namespace X39.Solutions.PdfTemplate.Data;

/// <summary>
/// A size with a position and a size
/// </summary>
/// <param name="Width">The width of the size</param>
/// <param name="Height">The height of the size</param>
public readonly record struct Size(float Width, float Height)
    : IAdditionOperators<Size, Size, Size>,
        IAdditiveIdentity<Size, Size>,
        IDecrementOperators<Size>,
        IDivisionOperators<Size, Size, Size>,
        IEqualityOperators<Size, Size, bool>,
        IIncrementOperators<Size>,
        IMultiplicativeIdentity<Size, Size>,
        IMultiplyOperators<Size, Size, Size>,
        ISubtractionOperators<Size, Size, Size>,
        IUnaryPlusOperators<Size, Size>,
        IUnaryNegationOperators<Size, Size>,
        IMinMaxValue<Size>
{
    /// <inheritdoc />
    public static Size operator +(Size left, Size right)
        => new(left.Width + right.Width, left.Height + right.Height);

    /// <inheritdoc />
    public static Size AdditiveIdentity => new(0, 0);

    /// <inheritdoc />
    public static Size operator --(Size value)
        => new(value.Width - 1, value.Height - 1);

    /// <inheritdoc />
    public static Size operator /(Size left, Size right)
        => new(left.Width / right.Width, left.Height / right.Height);

    /// <inheritdoc />
    public static Size operator ++(Size value)
        => new( value.Width + 1, value.Height + 1);

    /// <inheritdoc />
    public static Size MultiplicativeIdentity => new(1, 1);

    /// <inheritdoc />
    public static Size operator *(Size left, Size right)
        => new( left.Width * right.Width, left.Height * right.Height);

    /// <inheritdoc />
    public static Size operator -(Size left, Size right)
        => new( left.Width - right.Width, left.Height - right.Height);

    /// <inheritdoc />
    public static Size operator +(Size value)
        => new( +value.Width, +value.Height);

    /// <inheritdoc />
    public static Size operator -(Size value)
        => new( -value.Width, -value.Height);

    /// <inheritdoc />
    public static Size MaxValue => new(float.MaxValue, float.MaxValue);
    /// <inheritdoc />
    public static Size MinValue => new(float.MinValue, float.MinValue);
}