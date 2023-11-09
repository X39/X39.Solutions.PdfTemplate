using System.Runtime.Serialization;
using System.Xml;

namespace X39.Solutions.PdfTemplate.Xml;

/// <summary>
/// Base class for exceptions thrown during parsing of an XML document.
/// </summary>
public abstract class XmlTemplateReaderException : XmlException
{

    /// <inheritdoc />
    protected XmlTemplateReaderException(int lineNumber, int linePosition) : base(null, null, lineNumber, linePosition)
    {
    }

    /// <inheritdoc />
    protected XmlTemplateReaderException(XmlNode xmlNode) : base(null, null, xmlNode.Line, xmlNode.Column)
    {
    }

    /// <inheritdoc />
    protected XmlTemplateReaderException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <inheritdoc />
    protected XmlTemplateReaderException(string? message, XmlNode xmlNode) : base(message, null, xmlNode.Line, xmlNode.Column)
    {
    }

    /// <inheritdoc />
    protected XmlTemplateReaderException(string? message, int lineNumber, int linePosition) : base(message, null, lineNumber, linePosition)
    {
    }

    /// <inheritdoc />
    protected XmlTemplateReaderException(string? message, Exception? innerException, XmlNode xmlNode) : base(message, innerException, xmlNode.Line, xmlNode.Column)
    {
    }

    /// <inheritdoc />
    protected XmlTemplateReaderException(string? message, Exception? innerException, int lineNumber, int linePosition) : base(message, innerException, lineNumber, linePosition)
    {
    }
}