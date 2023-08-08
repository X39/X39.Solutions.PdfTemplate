using System.Collections;

namespace X39.Solutions.PdfTemplate.Xml;

public record XmlNodeInformation(
        int Line,
        int Column,
        string NodeName,
        string NodeNamespace,
        Dictionary<string, string> Attributes,
        IReadOnlyCollection<XmlNodeInformation> Children)
    : IEnumerable<XmlNodeInformation>
{
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