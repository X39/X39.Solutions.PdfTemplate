using System.Security.Cryptography;
using System.Text;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Shared, backend-neutral helpers for writing <see cref="EmbeddedFile"/> metadata into PDF output.
/// </summary>
/// <remarks>
/// Both PDF backends produce the same embedded file metadata even though one builds PDF objects through
/// a library and the other appends them as bytes, so the formatting rules live here instead of being
/// duplicated per backend.
/// </remarks>
internal static class PdfEmbeddedFileMetadata
{
    /// <summary>
    /// Formats a timestamp as a PDF date string, for example <c>D:20260613151620+02'00'</c>.
    /// </summary>
    /// <param name="value">The timestamp to format.</param>
    /// <returns>The PDF date string.</returns>
    public static string FormatDate(DateTimeOffset value)
    {
        var offset = value.Offset;
        var builder = new StringBuilder("D:")
            .Append(value.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture));
        if (offset == TimeSpan.Zero)
            return builder.Append('Z').ToString();

        return builder
               .Append(offset < TimeSpan.Zero ? '-' : '+')
               .Append(Math.Abs(offset.Hours).ToString("D2", CultureInfo.InvariantCulture))
               .Append('\'')
               .Append(Math.Abs(offset.Minutes).ToString("D2", CultureInfo.InvariantCulture))
               .Append('\'')
               .ToString();
    }

    /// <summary>
    /// Computes the embedded file parameter checksum of the content.
    /// </summary>
    /// <param name="content">The embedded file content.</param>
    /// <returns>The checksum bytes.</returns>
    public static byte[] ComputeChecksum(ReadOnlySpan<byte> content)
        => MD5.HashData(content);

    /// <summary>
    /// Escapes a value so it can be written as a PDF name, for example <c>application#2Fxml</c>.
    /// </summary>
    /// <param name="value">The value to escape.</param>
    /// <returns>The escaped value, without the leading name marker.</returns>
    public static string EscapeName(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var bytes = Encoding.UTF8.GetBytes(value);
        var builder = new StringBuilder(bytes.Length);
        foreach (var utf8Byte in bytes)
        {
            var character = (char)utf8Byte;
            if (utf8Byte < 0x80
                && (char.IsAsciiLetterOrDigit(character) || character is '-' or '+' or '.' or '_'))
            {
                builder.Append(character);
                continue;
            }

            builder.Append('#').Append(utf8Byte.ToString("X2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Gets the PDF associated file relationship name for a relationship.
    /// </summary>
    /// <param name="relationship">The relationship to map.</param>
    /// <returns>The relationship name, without the leading name marker.</returns>
    public static string GetRelationshipName(EmbeddedFileRelationship relationship)
        => relationship switch
        {
            EmbeddedFileRelationship.Source => "Source",
            EmbeddedFileRelationship.Data => "Data",
            EmbeddedFileRelationship.Alternative => "Alternative",
            EmbeddedFileRelationship.Supplement => "Supplement",
            EmbeddedFileRelationship.EncryptedPayload => "EncryptedPayload",
            EmbeddedFileRelationship.FormData => "FormData",
            EmbeddedFileRelationship.Schema => "Schema",
            _ => "Unspecified",
        };
}
