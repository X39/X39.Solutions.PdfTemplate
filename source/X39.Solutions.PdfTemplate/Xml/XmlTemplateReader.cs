using System.Xml;

namespace X39.Solutions.PdfTemplate.Xml;

/// <summary>
/// Class to read a template from a <see cref="XmlReader"/>.
/// </summary>
public class XmlTemplateReader
{
    private readonly Stack<XmlStyleInformation> _styles = new();

    /// <summary>
    /// Reads the template from the given <paramref name="reader"/> and returns the root node.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <returns>The root node of the template.</returns>
    public XmlNodeInformation Read(XmlReader reader)
    {
        reader.MoveToContent();
        return ReadNode(reader);
    }

    private XmlNodeInformation ReadNode(XmlReader reader)
    {
        PushStyle();
        var nodeName = reader.Name;
        if (!Validators.ControlName.IsValid(nodeName))
            throw new XmlNodeNameException(
                $"Invalid node name {nodeName} at L{((IXmlLineInfo) reader).LineNumber}:C{((IXmlLineInfo) reader).LinePosition}.");
        var nodeNamespace = reader.NamespaceURI;
        var styleNodeName = $"{nodeName}.style";
        var isEmptyElement = reader.IsEmptyElement;
        var attributes = new Dictionary<string, string>();
        if (reader.HasAttributes)
        {
            for (var i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);
                // Make uppercase to make it case insensitive
                attributes.Add(reader.Name.ToUpperInvariant(), reader.Value);
            }
        }

        var nodeLocation = (line: ((IXmlLineInfo) reader).LineNumber, column: ((IXmlLineInfo) reader).LinePosition);
        reader.ReadStartElement();
        reader.MoveToContent();
        var children = new List<XmlNodeInformation>();
        if (!isEmptyElement)
        {
            while (reader.NodeType != XmlNodeType.EndElement)
            {
                if (reader.Name.Equals(styleNodeName, StringComparison.OrdinalIgnoreCase))
                    ReadStyle(reader);
                else
                    children.Add(ReadNode(reader));
            }

            reader.ReadEndElement();
            reader.MoveToContent();
        }

        var effectiveStyle = GetEffectiveStyle();
        PopStyle();
        return new XmlNodeInformation(
            nodeLocation.line,
            nodeLocation.column,
            nodeName,
            nodeNamespace,
            effectiveStyle.Of(nodeName, nodeNamespace, attributes),
            children.AsReadOnly());
    }

    private XmlStyleInformation GetEffectiveStyle()
    {
        Dictionary<(string controlName, string controlNamespace), IReadOnlyDictionary<string, string>> effectiveStyles =
            new();
        foreach (var style in _styles)
        {
            foreach (var (key, value) in style.GetAll())
            {
                effectiveStyles[key] = value;
            }
        }

        return new XmlStyleInformation(effectiveStyles);
    }

    private void ReadStyle(XmlReader reader)
    {
        var style = CurrentStyle;
        reader.ReadStartElement();
        reader.MoveToContent();
        while (reader.NodeType != XmlNodeType.EndElement)
        {
            var controlName = reader.Name;
            var controlNamespace = reader.NamespaceURI;
            if (!reader.IsEmptyElement)
                throw new XmlStyleInformationCannotNestException(
                    $"Expected empty element for {controlNamespace}:{controlName} at L{((IXmlLineInfo) reader).LineNumber}:C{((IXmlLineInfo) reader).LinePosition}.");
            var attributes = new Dictionary<string, string>();
            if (reader.HasAttributes)
            {
                for (var i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    // Make uppercase to make it case insensitive
                    attributes.Add(reader.Name.ToUpperInvariant(), reader.Value);
                }
            }

            reader.ReadStartElement();
            reader.MoveToContent();
            style.Add((IXmlLineInfo) reader, controlName, controlNamespace, attributes);
        }

        reader.ReadEndElement();
        reader.MoveToContent();
    }

    private void PushStyle() => _styles.Push(new XmlStyleInformation());
    private void PopStyle() => _styles.Pop();
    private XmlStyleInformation CurrentStyle => _styles.Peek();
}