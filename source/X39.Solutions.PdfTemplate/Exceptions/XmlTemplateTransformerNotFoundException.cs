namespace X39.Solutions.PdfTemplate.Exceptions;

internal class XmlTemplateTransformerNotFoundException : Exception
{
    public int Line { get; }
    public int Column { get; }
    public string TransformerName { get; }
    public XmlTemplateTransformerNotFoundException(int line, int column, string transformerName, string message) : base(message)
    {
        Line            = line;
        Column     = column;
        TransformerName = transformerName;
    }

}