using System.Collections;

namespace X39.Solutions.PdfTemplate.Controls.Base;

/// <summary>
/// Base class for alignable controls that can contain other controls.
/// </summary>
[PublicAPI]
public abstract class AlignableContentControl : AlignableControl, IContentControl, IAsyncDisposable
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

    /// <summary>
    /// Dispose pattern method, that can be overridden by derived classes.
    /// </summary>
    /// <remarks>
    /// Always call the base implementation!
    /// </remarks>
    protected virtual async ValueTask DisposeAsyncCore()
    {
        foreach (var child in _children)
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (child is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            // ReSharper disable once SuspiciousTypeConversion.Global
            else if (child is IDisposable disposable)
                disposable.Dispose();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }
}