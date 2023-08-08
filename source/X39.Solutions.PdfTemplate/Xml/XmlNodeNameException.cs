using System.Xml;

namespace X39.Solutions.PdfTemplate.Xml;

/// <summary>
/// Thrown during parsing of an XML document if a node name is invalid.
/// </summary>
public class XmlNodeNameException : XmlException
{
    internal XmlNodeNameException(string message) : base(message)
    {
    }
}