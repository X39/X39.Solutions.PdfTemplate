namespace X39.Solutions.PdfTemplate.Exceptions;

/// <summary>
/// Represents an exception that is thrown when an area is incomplete in a PDF template.
/// </summary>
public class AreaIncompleteException : Exception
{
    /// <summary>
    /// Gets the line number where the exception occurred.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Gets the column number associated with the exception.
    /// </summary>
    /// <remarks>
    /// The Column property indicates the specific column where the incomplete area
    /// was encountered during the parsing or processing operation.
    /// </remarks>
    public int Column { get; }

    /// <summary>
    /// Represents an exception that is thrown when an area is found to be incomplete.
    /// </summary>
    internal AreaIncompleteException(int line, int column, string message) : base(message)
    {
        Line   = line;
        Column = column;
    }
}