using X39.Solutions.Papercraft.Data;

namespace X39.Solutions.Papercraft.Abstraction;

/// <summary>
/// A canvas for drawing on.
/// </summary>
/// <remarks>
/// A deferred canvas is a canvas that does not draw immediately but instead stores the drawing operations
/// </remarks>
public interface IDeferredCanvas : IDrawableCanvas
{
    /// <summary>
    /// The actual size of a single page without any margins applied.
    /// </summary>
    Size ActualPageSize { get; }

    /// <summary>
    /// The pageable size of the current canvas layer after margins and reserved regions are applied.
    /// </summary>
    /// <remarks>
    /// Controls use its height as the pagination boundary. For foreground and background layers,
    /// this is the same as <see cref="ActualPageSize"/>.
    /// </remarks>
    Size PageSize { get; }

    /// <summary>
    /// Represents the current translation of the canvas.
    /// </summary>
    /// <remarks>
    /// The translation is a <see cref="Point"/> value that represents the current position of the canvas.
    /// It specifies the distance to move all subsequent drawing operations on the canvas.
    /// </remarks>
    Point Translation { get; }
    
    /// <summary>
    /// Defers the specified action, allowing to draw on the canvas directly.
    /// </summary>
    /// <remarks>
    /// Prefer using the other Draw* methods over this method, as this method may
    /// introduce slowdowns.
    /// </remarks>
    /// <param name="action">The action to defer.</param>
    void Defer(Action<IImmediateCanvas> action);
}
