using System.Xml;

namespace X39.Solutions.PdfTemplate.Xml;

/// <summary>
/// Thrown during parsing of an XML document if a style information node is not closed immediately using /&gt;.
/// </summary>
public class XmlStyleInformationCannotNestException : XmlException
{
    internal XmlStyleInformationCannotNestException(string message) : base(message)
    {
    }
}