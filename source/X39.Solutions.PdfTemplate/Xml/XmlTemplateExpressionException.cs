namespace X39.Solutions.PdfTemplate.Xml;

internal class XmlTemplateExpressionException : Exception
{
    public int Line { get; }
    public int Column { get; }

    public XmlTemplateExpressionException(int line, int column, string message) : base(message)
    {
        Line   = line;
        Column = column;
    }
}