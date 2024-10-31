namespace X39.Solutions.PdfTemplate.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a content control
/// does not support the provided child control type.
/// </summary>
/// <remarks>
/// This exception is specifically used within the context of PDF template processing
/// where content controls have strict type constraints on the child controls they can contain.
/// </remarks>
/// <example>
/// An instance of this exception may be thrown during the creation of a control hierarchy
/// when a child control that does not match the expected type is added to a content control.
/// </example>
/// <seealso cref="X39.Solutions.PdfTemplate.Abstraction.IContentControl"/>
[PublicAPI]
public class ContentControlDoesNotSupportTheProvidedChildException : Exception
{
    /// <summary>
    /// Gets the line number where the exception occurred.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Gets the column number where the exception occurred.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Gets the type of the unsupported child control.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Exception thrown when a content control does not support the provided child type.
    /// This exception is used to indicate that an attempt was made to add a child control to a content control,
    /// but the content control does not support adding children of the specified type.
    /// </summary>
    /// <param name="line">The line number in the source where the error occurred.</param>
    /// <param name="column">The column number in the source where the error occurred.</param>
    /// <param name="type">The type of the child control that caused the exception.</param>
    /// <param name="message">The message of the exception.</param>
    internal ContentControlDoesNotSupportTheProvidedChildException(int line, int column, Type type, string message) : base(message)
    {
        Line   = line;
        Column = column;
        Type   = type;
    }
}