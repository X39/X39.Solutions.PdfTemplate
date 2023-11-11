using System.Diagnostics.CodeAnalysis;

namespace X39.Solutions.PdfTemplate.Xml;

/// <summary>
/// Represents a node in the XML template.
/// </summary>
public sealed class XmlNode
{
    /// <summary>
    /// The data scope of this node.
    /// </summary>
    internal Dictionary<string, object?>? Scope { get; set; }
    
    private readonly Dictionary<string, string> _attributes = new();
    private          XmlNode?                   _parent;
    private readonly List<XmlNode>              _children = new();

    /// <summary>
    /// The attributes of this node.
    /// </summary>
    public IReadOnlyDictionary<string, string> Attributes => _attributes;

    /// <summary>
    /// The parent of this node.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when this node is not attached to a parent.</exception>
    public XmlNode Parent =>
        _parent ?? throw new InvalidOperationException("This node is currently not attached to a parent.");

    /// <summary>
    /// The children of this node.
    /// </summary>
    public IReadOnlyCollection<XmlNode> Children => _children.AsReadOnly();
    
    /// <summary>
    /// Accesses a child of this node.
    /// </summary>
    /// <param name="index">The index of the child to access.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when the index is out of range.</exception>
    public XmlNode this[int index] => _children[index];

    /// <summary>
    /// The line and column of this node.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// The line and column of this node.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// The name of this node.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The namespace of this node.
    /// </summary>
    public string Namespace { get; }

    /// <summary>
    /// The text of this node or <see langword="null"/> if this node is not a text node.
    /// </summary>
    public string? Text { get; private set; }

    /// <summary>
    /// Creates a new <see cref="XmlNode"/>.
    /// </summary>
    /// <param name="line">The line of this node.</param>
    /// <param name="column">The column of this node.</param>
    /// <param name="ns">The namespace of this node.</param>
    /// <param name="name">The name of this node.</param>
    public XmlNode(int line, int column, string ns, string name)
    {
        Name       = name;
        Namespace  = ns;
        Line       = line;
        Column     = column;
        IsTextNode = false;
    }

    /// <summary>
    /// Whether this node is a pure text node.
    /// </summary>
    public bool IsTextNode { get; set; }

    /// <summary>
    /// Creates a new text <see cref="XmlNode"/>.
    /// </summary>
    /// <param name="line">The line of this node.</param>
    /// <param name="column">The column of this node.</param>
    /// <param name="text">The text of this node.</param>
    public XmlNode(int line, int column, string text)
    {
        Name       = string.Empty;
        Namespace  = string.Empty;
        Text       = text;
        Line       = line;
        Column     = column;
        IsTextNode = true;
    }

    /// <summary>
    /// Adds a child to this node.
    /// </summary>
    /// <param name="child">The child to add.</param>
    /// <exception cref="InvalidOperationException">Thrown when the child is already attached to a parent.</exception>
    public void AddChild(XmlNode child)
    {
        if (child._parent is not null)
            throw new InvalidOperationException("This node is already attached to a parent.");
        child._parent = this;
        _children.Add(child);
    }

    /// <summary>
    /// Removes a child from this node.
    /// </summary>
    /// <param name="child">The child to remove.</param>
    /// <exception cref="InvalidOperationException">Thrown when the child is not attached to this parent.</exception>
    public void RemoveChild(XmlNode child)
    {
        if (child._parent != this)
            throw new InvalidOperationException("This node is not attached to this parent.");
        _children.Remove(child);
        child._parent = null;
    }

    /// <summary>
    /// Removes all children from this node.
    /// </summary>
    public void ClearChildren()
    {
        foreach (var child in _children)
        {
            child._parent = null;
        }

        _children.Clear();
    }

    /// <summary>
    /// Sets an attribute of this node.
    /// </summary>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="value">The value of the attribute.</param>
    public void SetAttribute(string name, string value)
    {
        _attributes[name.ToUpperInvariant()] = value;
    }

    /// <summary>
    /// Removes an attribute from this node.
    /// </summary>
    /// <param name="name">The name of the attribute to remove.</param>
    public void RemoveAttribute(string name)
    {
        _attributes.Remove(name);
    }

    /// <summary>
    /// Removes all attributes from this node.
    /// </summary>
    public void ClearAttributes()
    {
        _attributes.Clear();
    }

    /// <summary>
    /// Tries to get an attribute from this node.
    /// </summary>
    /// <param name="name">The name of the attribute to get.</param>
    /// <param name="value">The value of the attribute or <see langword="null"/> if the attribute does not exist.</param>
    /// <returns><see langword="true"/> if the attribute exists, otherwise <see langword="false"/>.</returns>
    public bool TryGetAttribute(string name, out string? value)
    {
        return _attributes.TryGetValue(name.ToUpperInvariant(), out value);
    }

    /// <summary>
    /// Attempts to get a child from this node.
    /// </summary>
    /// <param name="node">The name of the child to get.</param>
    /// <param name="child">The child or <see langword="null"/> if the child does not exist.</param>
    /// <returns><see langword="true"/> if the child exists, otherwise <see langword="false"/>.</returns>
    public bool TryGetChild(string node, [NotNullWhen(true)] out XmlNode? child)
    {
        foreach (var childNode in _children)
        {
            if (!childNode.Name.Equals(node, StringComparison.OrdinalIgnoreCase))
                continue;
            child = childNode;
            return true;
        }

        child = null;
        return false;
    }

    /// <summary>
    /// Changes the text of this node.
    /// </summary>
    /// <param name="text">The new text of this node.</param>
    /// <exception cref="InvalidOperationException">Thrown when this node is not a text node.</exception>
    public void SetText(string text)
    {
        if (!IsTextNode)
            throw new InvalidOperationException("This node is not a text node.");
        Text = text;
    }

    /// <summary>
    /// Creates a deep copy of this node.
    /// </summary>
    /// <returns>A deep copy of this node.</returns>
    public XmlNode DeepCopy()
    {
        if (IsTextNode)
            return new XmlNode(Line, Column, Text!) {Scope = Scope};
        var node = new XmlNode(Line, Column, Namespace, Name){Scope = Scope};
        foreach (var (key, value) in _attributes)
        {
            node.SetAttribute(key, value);
        }

        foreach (var child in _children)
        {
            node.AddChild(child.DeepCopy());
        }

        return node;
    }

    /// <summary>
    /// Inserts a child at the given index.
    /// </summary>
    /// <param name="nodeIndex">The index to insert the child at.</param>
    /// <param name="xmlNode">The child to insert.</param>
    /// <exception cref="InvalidOperationException">Thrown when the child is already attached to a parent.</exception>
    public void InsertChild(int nodeIndex, XmlNode xmlNode)
    {
        if (xmlNode._parent is not null)
            throw new InvalidOperationException("This node is already attached to a parent.");
        xmlNode._parent = this;
        _children.Insert(nodeIndex, xmlNode);
    }
}