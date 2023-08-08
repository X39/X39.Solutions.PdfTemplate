namespace X39.Solutions.PdfTemplate;

/// <summary>
/// A control that can contain other controls.
/// </summary>
[PublicAPI]
public interface IContentControl : IControl, ICollection<IControl>
{
    /// <summary>
    /// Whether the control can contain the given control.
    /// </summary>
    /// <param name="type">The type of the control to check.</param>
    /// <returns>Whether the control can contain the given control.</returns>
    bool CanAdd(Type type);
}