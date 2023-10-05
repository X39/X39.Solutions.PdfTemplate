using System.Collections;

namespace X39.Solutions.PdfTemplate.Controls.Base;

/// <summary>
/// Base class for alignable controls that can contain other controls.
/// </summary>
public abstract class AlignableContentControl : AlignableControl, IContentControl
{
    private readonly List<IControl> _children = new();
    
    /// <summary>
    /// The children of this content control.
    /// </summary>
    public IReadOnlyCollection<IControl> Children => _children;

    /// <inheritdoc />
    public IEnumerator<IControl> GetEnumerator() => _children.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _children).GetEnumerator();

    /// <inheritdoc />
    public virtual void Add(IControl item) => _children.Add(item);

    /// <inheritdoc />
    public virtual void Clear() => _children.Clear();

    /// <inheritdoc />
    public bool Contains(IControl item) => _children.Contains(item);

    /// <inheritdoc />
    public void CopyTo(IControl[] array, int arrayIndex) => _children.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public virtual bool Remove(IControl item) => _children.Remove(item);

    /// <inheritdoc />
    public int Count => _children.Count;

    /// <inheritdoc />
    public bool IsReadOnly => ((ICollection<IControl>) _children).IsReadOnly;

    /// <inheritdoc />
    public abstract bool CanAdd(Type type);
}