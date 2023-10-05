using System.Collections;

namespace X39.Solutions.PdfTemplate.Xml;

/// <summary>
/// Contains the necessary information to create a <see cref="IControl"/>
/// </summary>
/// <param name="Line">The line the node is on.</param>
/// <param name="Column">The column the node is on.</param>
/// <param name="NodeName">The name of the control to create.</param>
/// <param name="NodeNamespace">The namespace the control is in.</param>
/// <param name="TextContent">The text content of the node.</param>
/// <param name="Attributes">The attributes of the node.</param>
/// <param name="Children">The children of the node.</param>
public record XmlNodeInformation(
        int Line,
        int Column,
        string NodeName,
        string NodeNamespace,
        string TextContent,
        Dictionary<string, string> Attributes,
        IReadOnlyCollection<XmlNodeInformation> Children)
    : IEnumerable<XmlNodeInformation>
{
    /// <summary>
    /// Returns the first child with the given <paramref name="controlName"/> and <paramref name="controlNamespace"/>.
    /// </summary>
    /// <param name="controlName">The name of the control to search for.</param>
    /// <param name="controlNamespace">The namespace the control is in.</param>
    public XmlNodeInformation? this[string controlName, string controlNamespace]
        => Children.FirstOrDefault(x => x.NodeName == controlName && x.NodeNamespace == controlNamespace);

    /// <inheritdoc />
    public IEnumerator<XmlNodeInformation> GetEnumerator()
    {
        return Children.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) Children).GetEnumerator();
    }
}