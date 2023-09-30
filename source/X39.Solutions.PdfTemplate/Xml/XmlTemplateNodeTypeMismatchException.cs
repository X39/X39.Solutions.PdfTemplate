using System.Xml;

namespace X39.Solutions.PdfTemplate.Xml;

internal class XmlTemplateNodeTypeMismatchException : Exception
{
    public int Line { get; }
    public int Column { get; }
    public XmlNodeType Expected { get; }
    public XmlNodeType Got { get; }

    public XmlTemplateNodeTypeMismatchException(int line, int column, XmlNodeType expected, XmlNodeType got, string message) : base(message)
    {
        Line        = line;
        Column = column;
        Expected    = expected;
        Got         = got;
    }
}