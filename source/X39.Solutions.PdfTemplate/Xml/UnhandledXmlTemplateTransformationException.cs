namespace X39.Solutions.PdfTemplate.Xml;

/// <summary>
/// Thrown during parsing of an XML document if a transformation faulted.
/// </summary>
public class UnhandledXmlTemplateTransformationException : XmlTemplateReaderException
{
    internal UnhandledXmlTemplateTransformationException(Exception exception, XmlNode node) : base("An error occurred while transforming the node.", exception, node)
    {
    }
}