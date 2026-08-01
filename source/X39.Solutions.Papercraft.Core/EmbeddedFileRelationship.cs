namespace X39.Solutions.Papercraft;

/// <summary>
/// Describes how an <see cref="EmbeddedFile"/> relates to the visible document content.
/// </summary>
/// <remarks>
/// PDF backends write this as the associated file relationship of the embedded file.
/// Consumers that look for machine-readable payloads, such as hybrid invoice readers, use it to
/// pick the relevant attachment out of a document that carries several.
/// </remarks>
public enum EmbeddedFileRelationship
{
    /// <summary>
    /// The relationship between the embedded file and the document is not specified.
    /// </summary>
    Unspecified,

    /// <summary>
    /// The embedded file is the original source of the visible document content.
    /// </summary>
    Source,

    /// <summary>
    /// The embedded file contains the data used to build the visible document content.
    /// </summary>
    Data,

    /// <summary>
    /// The embedded file is an alternative representation of the visible document content.
    /// </summary>
    Alternative,

    /// <summary>
    /// The embedded file supplements the visible document content.
    /// </summary>
    Supplement,

    /// <summary>
    /// The embedded file is an encrypted payload the document itself does not describe.
    /// </summary>
    EncryptedPayload,

    /// <summary>
    /// The embedded file contains form data matching the visible document content.
    /// </summary>
    FormData,

    /// <summary>
    /// The embedded file is a schema describing another embedded file.
    /// </summary>
    Schema,
}
