namespace X39.Solutions.PdfTemplate.Abstraction;

/// <summary>
/// Represents a deferred canvas for drawing on.
/// </summary>
/// <remarks>
/// This canvas is immediately drawing on the underlying canvas.
/// </remarks>
public interface IImmediateCanvas : IDrawableCanvas
{
    /// <summary>
    /// The current page number.
    /// </summary>
    ushort PageNumber { get; }
    
    /// <summary>
    /// 
    /// </summary>
    ushort TotalPages { get; }
}