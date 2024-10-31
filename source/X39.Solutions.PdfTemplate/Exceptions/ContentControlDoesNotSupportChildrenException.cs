namespace X39.Solutions.PdfTemplate.Exceptions;

/// <summary>
/// Exception thrown when an attempt is made to add child controls to a content control
/// that does not support children.
/// </summary>
/// <remarks>
/// This exception provides details about the location in the template where the
/// unsupported children were encountered, including the line and column numbers.
/// </remarks>
public class ContentControlDoesNotSupportChildrenException : Exception
{
    /// <summary>
    /// Gets the line number where the exception occurred.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Gets the column number where the exception occurred.
    /// </summary>
    /// <remarks>
    /// This property holds the column number which can help in identifying
    /// the exact location in the document or template that caused the exception.
    /// </remarks>
    public int Column { get; }

    /// <summary>
    /// Represents an exception that is thrown when a content control
    /// does not support the inclusion of child controls.
    /// </summary>
    /// <remarks>
    /// This exception provides information about the line and column
    /// where the issue occurred, aiding in debugging and error tracking.
    /// </remarks>
    internal ContentControlDoesNotSupportChildrenException(int line, int column, string message) : base(message)
    {
        Line   = line;
        Column = column;
    }
}