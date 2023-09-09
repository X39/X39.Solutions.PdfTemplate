using System.Collections;

namespace X39.Solutions.PdfTemplate.Xml;

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