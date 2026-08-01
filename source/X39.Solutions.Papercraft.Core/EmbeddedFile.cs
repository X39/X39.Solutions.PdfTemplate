using System.Text;

namespace X39.Solutions.Papercraft;

/// <summary>
/// A file attached to the rendered document.
/// </summary>
/// <remarks>
/// Renderers declaring <see cref="RendererFeatures.EmbeddedFiles"/> support write embedded files into
/// their output. PDF backends add them to the document embedded file name tree, which viewers surface
/// as document attachments and which machine consumers can extract again.
/// <para>
/// Content is held eagerly rather than as a stream factory, because one prepared
/// <see cref="PapercraftDocument"/> may be rendered more than once.
/// </para>
/// <para>
/// Names and media types are validated when rendering starts, not when this instance is created;
/// invalid values surface as <see cref="RenderDiagnosticCodes.InvalidEmbeddedFile"/> diagnostics.
/// </para>
/// </remarks>
public sealed record EmbeddedFile
{
    private readonly byte[] _content;

    /// <summary>
    /// Creates a new embedded file.
    /// </summary>
    /// <param name="name">The file name shown to the user and used as the embedded file key.</param>
    /// <param name="content">The file content.</param>
    public EmbeddedFile(string name, ReadOnlyMemory<byte> content)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
        _content = content.ToArray();
    }

    /// <summary>
    /// The file name shown to the user and used as the embedded file key.
    /// </summary>
    /// <remarks>
    /// The name must not be empty, must not contain path separators or control characters, and must be
    /// unique within one document.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    /// The file content.
    /// </summary>
    /// <remarks>The constructor copies the supplied memory so the content cannot change later.</remarks>
    public ReadOnlyMemory<byte> Content => _content;

    /// <summary>
    /// The media type of the file content.
    /// </summary>
    public string MediaType { get; init; } = PapercraftMediaTypes.ApplicationOctetStream;

    /// <summary>
    /// An optional description shown next to the attachment in viewers.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// How the file relates to the visible document content.
    /// </summary>
    public EmbeddedFileRelationship Relationship { get; init; } = EmbeddedFileRelationship.Unspecified;

    /// <summary>
    /// The creation timestamp written to the embedded file parameters, if known.
    /// </summary>
    /// <remarks>The timestamp offset is preserved in the PDF date value.</remarks>
    public DateTimeOffset? Created { get; init; }

    /// <summary>
    /// The last modification timestamp written to the embedded file parameters, if known.
    /// </summary>
    /// <remarks>The timestamp offset is preserved in the PDF date value.</remarks>
    public DateTimeOffset? Modified { get; init; }

    /// <summary>
    /// Creates an embedded file from text content.
    /// </summary>
    /// <param name="name">The file name shown to the user.</param>
    /// <param name="text">The text content.</param>
    /// <param name="encoding">The encoding used to convert the text. Defaults to UTF-8 without byte order mark.</param>
    /// <param name="mediaType">The media type of the content. Defaults to <c>text/plain</c>.</param>
    /// <returns>The embedded file.</returns>
    public static EmbeddedFile FromText(
        string name,
        string text,
        Encoding? encoding = null,
        string mediaType = PapercraftMediaTypes.TextPlain)
    {
        ArgumentNullException.ThrowIfNull(text);
        return new EmbeddedFile(name, (encoding ?? Encoding.UTF8).GetBytes(text))
        {
            MediaType = mediaType,
        };
    }

    /// <summary>
    /// Creates an embedded file by reading a stream to its end.
    /// </summary>
    /// <param name="name">The file name shown to the user.</param>
    /// <param name="stream">The stream to read.</param>
    /// <param name="mediaType">The media type of the content. Defaults to <c>application/octet-stream</c>.</param>
    /// <returns>The embedded file.</returns>
    public static EmbeddedFile FromStream(string name, Stream stream, string? mediaType = null)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return new EmbeddedFile(name, buffer.ToArray())
        {
            MediaType = mediaType ?? PapercraftMediaTypes.ApplicationOctetStream,
        };
    }

    /// <summary>
    /// Creates an embedded file by reading a stream to its end.
    /// </summary>
    /// <param name="name">The file name shown to the user.</param>
    /// <param name="stream">The stream to read.</param>
    /// <param name="mediaType">The media type of the content. Defaults to <c>application/octet-stream</c>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The embedded file.</returns>
    public static async ValueTask<EmbeddedFile> FromStreamAsync(
        string name,
        Stream stream,
        string? mediaType = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken)
                    .ConfigureAwait(false);
        return new EmbeddedFile(name, buffer.ToArray())
        {
            MediaType = mediaType ?? PapercraftMediaTypes.ApplicationOctetStream,
        };
    }

    /// <summary>
    /// Creates an embedded file by reading a file from disk.
    /// </summary>
    /// <param name="path">The path of the file to read.</param>
    /// <param name="name">The file name shown to the user. Defaults to the file name of <paramref name="path"/>.</param>
    /// <param name="mediaType">The media type of the content. Defaults to <c>application/octet-stream</c>.</param>
    /// <returns>The embedded file.</returns>
    public static EmbeddedFile FromFile(string path, string? name = null, string? mediaType = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new EmbeddedFile(name ?? Path.GetFileName(path), File.ReadAllBytes(path))
        {
            MediaType = mediaType ?? PapercraftMediaTypes.ApplicationOctetStream,
            Created = File.GetCreationTimeUtc(path),
            Modified = File.GetLastWriteTimeUtc(path),
        };
    }

    /// <summary>
    /// Creates an embedded file by reading a file from disk.
    /// </summary>
    /// <param name="path">The path of the file to read.</param>
    /// <param name="name">The file name shown to the user. Defaults to the file name of <paramref name="path"/>.</param>
    /// <param name="mediaType">The media type of the content. Defaults to <c>application/octet-stream</c>.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The embedded file.</returns>
    public static async ValueTask<EmbeddedFile> FromFileAsync(
        string path,
        string? name = null,
        string? mediaType = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var content = await File.ReadAllBytesAsync(path, cancellationToken)
                                .ConfigureAwait(false);
        return new EmbeddedFile(name ?? Path.GetFileName(path), content)
        {
            MediaType = mediaType ?? PapercraftMediaTypes.ApplicationOctetStream,
            Created = File.GetCreationTimeUtc(path),
            Modified = File.GetLastWriteTimeUtc(path),
        };
    }
}
