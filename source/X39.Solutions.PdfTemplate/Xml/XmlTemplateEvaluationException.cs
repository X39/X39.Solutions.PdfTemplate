namespace X39.Solutions.PdfTemplate.Xml;

internal class XmlTemplateEvaluationException : Exception
{
    public int Line { get; }
    public int Column { get; }

    public XmlTemplateEvaluationException(int line, int column, string message, Exception exception) : base(message, exception)
    {
        Line        = line;
        Column = column;
    }
}