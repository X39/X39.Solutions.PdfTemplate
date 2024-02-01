using System.Runtime.Serialization;

namespace X39.Solutions.PdfTemplate.Xml.Exceptions;

/// <summary>
/// Base class for exceptions thrown transforming an XML document.
/// </summary>
public abstract class XmlTemplateTransformationException : XmlTemplateReaderException
{
    /// <inheritdoc />
    protected XmlTemplateTransformationException(int lineNumber, int linePosition) : base(lineNumber, linePosition)
    {
    }

    /// <inheritdoc />
    protected XmlTemplateTransformationException(XmlNode xmlNode) : base(xmlNode)
    {
    }

    /// <inheritdoc />
    [Obsolete("Obsolete")]
    protected XmlTemplateTransformationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <inheritdoc />
    protected XmlTemplateTransformationException(string? message, XmlNode xmlNode) : base(message, xmlNode)
    {
    }

    /// <inheritdoc />
    protected XmlTemplateTransformationException(string? message, int lineNumber, int linePosition) : base(
        message,
        lineNumber,
        linePosition)
    {
    }

    /// <inheritdoc />
    protected XmlTemplateTransformationException(string? message, Exception? innerException, XmlNode xmlNode) : base(
        message,
        innerException,
        xmlNode)
    {
    }

    /// <inheritdoc />
    protected XmlTemplateTransformationException(
        string? message,
        Exception? innerException,
        int lineNumber,
        int linePosition) : base(message, innerException, lineNumber, linePosition)
    {
    }
}