using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Exceptions;

/// <summary>
/// Thrown when a control does was not being able to be created.
/// </summary>
public sealed class FailedToCreateControlException : XmlTemplateReaderException
{
    internal FailedToCreateControlException(Exception exception, XmlNodeInformation node)
        : base(
            $"The creation of the control {node.NodeNamespace}:{node.NodeName} at L{node.Line}:C{node.Column} failed: {exception.Message}",
            exception,
            node.Line,
            node.Column)
    {
    }
}