namespace X39.Solutions.PdfTemplate.Abstraction;

/// <summary>
/// Canvas extension methods.
/// </summary>
public static class Canvas
{
    /// <summary>
    /// This call saves the current matrix, clip, and draw filter,
    /// and pushes a copy onto a private stack.
    /// Subsequent calls to translate, scale, rotate, skew, concatenate or clipping path or drawing filter
    /// all operate on this copy.
    /// When the balancing call is done by disposing the return value,
    /// the previous matrix, clipping, and drawing filters are restored.
    /// </summary>
    public static IDisposable CreateState(this ICanvas canvas)
    {
        canvas.PushState();
        return new Disposable(canvas.PopState);
    }
}