using System.Xml;

namespace X39.Solutions.PdfTemplate.Xml.Exceptions;

/// <summary>
/// Thrown during parsing of an XML document if a style information node is not closed immediately using /&gt;.
/// </summary>
public class XmlStyleInformationCannotNestException : XmlException
{
    /// <summary>
    /// The line where the error occured.
    /// </summary>
    public int Line { get; }
    
    /// <summary>
    /// The column where the error occured.
    /// </summary>
    public int Column { get; }

    internal XmlStyleInformationCannotNestException(int line, int column, string message) : base(message)
    {
        Line        = line;
        Column = column;
    }
}