using System.Xml;

namespace X39.Solutions.PdfTemplate.Xml;

/// <summary>
/// Thrown during parsing of an XML document if a node name is invalid.
/// </summary>
public class XmlNodeNameException : XmlTemplateReaderException
{
    internal XmlNodeNameException(XmlNode xmlNode) : base($"Invalid node name {xmlNode.Name} at L{xmlNode.Line}:C{xmlNode.Column}.", xmlNode)
    {
    }
}