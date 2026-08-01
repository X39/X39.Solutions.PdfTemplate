using System.Net.Http.Headers;

namespace X39.Solutions.Papercraft;

/// <summary>
/// Validates the embedded files requested by a document against what a renderer can write.
/// </summary>
/// <remarks>
/// Renderers supporting <see cref="RendererFeatures.EmbeddedFiles"/> fold this into their own
/// validation so that unusable attachment metadata surfaces as a regular render diagnostic instead of
/// an exception from deep inside a backend.
/// </remarks>
public static class EmbeddedFileValidation
{
    /// <summary>
    /// Validates the embedded files of a prepared document.
    /// </summary>
    /// <param name="document">The document to validate.</param>
    /// <returns>The validation result.</returns>
    public static RenderValidationResult Validate(PapercraftDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var diagnostics = GetDiagnostics(document.DocumentOptions.EmbeddedFiles);
        return diagnostics.Count is 0
            ? RenderValidationResult.Supported
            : new RenderValidationResult(diagnostics);
    }

    /// <summary>
    /// Gets diagnostics for embedded files that cannot be written as specified.
    /// </summary>
    /// <param name="embeddedFiles">The embedded files to validate.</param>
    /// <returns>Diagnostics for unusable embedded files, or an empty list if all files are usable.</returns>
    public static IReadOnlyList<RenderDiagnostic> GetDiagnostics(IReadOnlyList<EmbeddedFile> embeddedFiles)
    {
        ArgumentNullException.ThrowIfNull(embeddedFiles);
        if (embeddedFiles.Count is 0)
            return Array.Empty<RenderDiagnostic>();

        var diagnostics = new List<RenderDiagnostic>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var candidate in embeddedFiles)
        {
            EmbeddedFile? embeddedFile = candidate;
            if (embeddedFile is null)
            {
                diagnostics.Add(
                    CreateDiagnostic("The document requested a null embedded file."));
                continue;
            }

            var name = embeddedFile.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                diagnostics.Add(
                    CreateDiagnostic("An embedded file name must not be empty or whitespace."));
            }
            else if (HasInvalidNameCharacters(name))
            {
                diagnostics.Add(
                    CreateDiagnostic(
                        $"Embedded file name '{name}' must not contain path separators or control characters.",
                        "Embedded file names are plain file names inside the document, not paths on disk."));
            }
            else if (!names.Add(name))
            {
                diagnostics.Add(
                    CreateDiagnostic(
                        $"Embedded file name '{name}' is used more than once.",
                        "Embedded file names are keys inside the document and must be unique, ignoring case."));
            }

            if (!HasValidUnicode(name))
            {
                diagnostics.Add(
                    CreateDiagnostic(
                        $"Embedded file name '{name}' contains invalid Unicode data.",
                        "File names must contain complete Unicode scalar values."));
            }

            if (!IsValidMediaType(embeddedFile.MediaType))
            {
                diagnostics.Add(
                    CreateDiagnostic(
                        $"Embedded file '{name}' must declare a valid, concrete media type.",
                        $"Use '{PapercraftMediaTypes.ApplicationOctetStream}' when the content type is unknown."));
            }

            if (!Enum.IsDefined(embeddedFile.Relationship))
            {
                diagnostics.Add(
                    CreateDiagnostic(
                        $"Embedded file '{name}' declares an unknown associated-file relationship value '{(int)embeddedFile.Relationship}'."));
            }

            if (embeddedFile.Description is not null && !HasValidUnicode(embeddedFile.Description))
            {
                diagnostics.Add(
                    CreateDiagnostic(
                        $"Embedded file '{name}' has a description containing invalid Unicode data.",
                        "Descriptions must contain complete Unicode scalar values."));
            }
        }

        return diagnostics;
    }

    private static bool HasInvalidNameCharacters(string name)
    {
        if (name is "." or "..")
            return true;

        foreach (var character in name)
        {
            if (character is '/' or '\\' or ':' || char.IsControl(character))
                return true;
        }

        return false;
    }

    private static bool IsValidMediaType(string? mediaType)
    {
        if (string.IsNullOrWhiteSpace(mediaType)
            || !MediaTypeHeaderValue.TryParse(mediaType, out var parsed)
            || string.IsNullOrWhiteSpace(parsed.MediaType))
        {
            return false;
        }

        var separatorIndex = parsed.MediaType.IndexOf('/');
        return separatorIndex > 0
               && separatorIndex < parsed.MediaType.Length - 1
               && parsed.MediaType[0] is not '*'
               && parsed.MediaType[separatorIndex + 1] is not '*';
    }

    private static bool HasValidUnicode(string value)
    {
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (char.IsHighSurrogate(character))
            {
                if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1]))
                    return false;
                index++;
            }
            else if (char.IsLowSurrogate(character))
            {
                return false;
            }
        }

        return true;
    }

    private static RenderDiagnostic CreateDiagnostic(string message, string? backendLimitation = null)
        => new(
            RenderDiagnosticCodes.InvalidEmbeddedFile,
            RendererSupportLevel.Unsupported,
            RendererFeatures.EmbeddedFiles,
            message,
            backendLimitation);
}
