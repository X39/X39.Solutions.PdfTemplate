using X39.Solutions.PdfTemplate.Xml;
using X39.Solutions.PdfTemplate.Xml.Exceptions;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Thrown when a control does was not being able to be created.
/// </summary>
public sealed class FailedToCreateControlException : XmlTemplateReaderException
{
    internal FailedToCreateControlException(Exception exception, XmlNodeInformation node)
        : base(
            $"The creation of the control {node.NodeNamespace}:{node.NodeName} at L{node.Line}:C{node.Column} failed.",
            exception,
            node.Line,
            node.Column)
    {
    }
}