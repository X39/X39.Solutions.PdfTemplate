namespace X39.Solutions.PdfTemplate;

internal class ContentControlDoesNotSupportChild : Exception
{
    public int Line { get; }
    public int Column { get; }
    public Type Type { get; }

    public ContentControlDoesNotSupportChild(int line, int column, Type type, string message) : base(message)
    {
        Line   = line;
        Column = column;
        Type   = type;
    }
}