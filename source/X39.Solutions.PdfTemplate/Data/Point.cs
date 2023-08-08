using System.Numerics;
using SkiaSharp;

namespace X39.Solutions.PdfTemplate.Data;
/// <summary>
/// A point representing a position
/// </summary>
/// <param name="X">The X position of the point</param>
/// <param name="Y">The Y position of the point</param>
public readonly record struct Point(float X, float Y)
    : IAdditionOperators<Point, Point, Point>,
        IAdditiveIdentity<Point, Point>,
        IDecrementOperators<Point>,
        IDivisionOperators<Point, Point, Point>,
        IEqualityOperators<Point, Point, bool>,
        IIncrementOperators<Point>,
        IMultiplicativeIdentity<Point, Point>,
        IMultiplyOperators<Point, Point, Point>,
        ISubtractionOperators<Point, Point, Point>,
        IUnaryPlusOperators<Point, Point>,
        IUnaryNegationOperators<Point, Point>,
        IMinMaxValue<Point>
{
    /// <inheritdoc />
    public static Point operator +(Point left, Point right)
        => new(left.X + right.X, left.Y + right.Y);

    /// <inheritdoc />
    public static Point AdditiveIdentity => new(0, 0);

    /// <inheritdoc />
    public static Point operator --(Point value)
        => new(value.X - 1, value.Y - 1);

    /// <inheritdoc />
    public static Point operator /(Point left, Point right)
        => new(left.X / right.X, left.Y / right.Y);

    /// <inheritdoc />
    public static Point operator ++(Point value)
        => new(value.X + 1, value.Y + 1);

    /// <inheritdoc />
    public static Point MultiplicativeIdentity => new(1, 1);

    /// <inheritdoc />
    public static Point operator *(Point left, Point right)
        => new(left.X * right.X, left.Y * right.Y);

    /// <inheritdoc />
    public static Point operator -(Point left, Point right)
        => new(left.X - right.X, left.Y - right.Y);

    /// <inheritdoc />
    public static Point operator +(Point value)
        => new(+value.X, +value.Y);

    /// <inheritdoc />
    public static Point operator -(Point value)
        => new(-value.X, -value.Y);

    /// <inheritdoc />
    public static Point MaxValue => new(float.MaxValue, float.MaxValue);

    /// <inheritdoc />
    public static Point MinValue => new(float.MinValue, float.MinValue);
    
    /// <summary>
    /// Implicitly converts a <see cref="Point"/> to a <see cref="SKPoint"/>
    /// </summary>
    /// <param name="point">The point to convert</param>
    /// <returns>The converted point</returns>
    public static implicit operator SKPoint(Point point)
        => new(point.X, point.Y);
}