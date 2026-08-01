using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;

internal static partial class SkiaPdfEmbeddedFileAppender
{
    private static readonly Encoding PdfTextEncoding = Encoding.BigEndianUnicode;

    public static void Append(MemoryStream pdfStream, IReadOnlyList<EmbeddedFile> embeddedFiles)
    {
        ArgumentNullException.ThrowIfNull(pdfStream);
        ArgumentNullException.ThrowIfNull(embeddedFiles);
        if (embeddedFiles.Count is 0)
            return;

        var originalBytes = pdfStream.ToArray();
        var originalXrefOffset = FindStartXref(originalBytes);
        var trailer = ParseTrailer(originalBytes, originalXrefOffset);
        var rootOffset = FindObjectOffset(originalBytes, originalXrefOffset, trailer.RootObjectNumber);
        var rootDictionary = ReadDictionary(originalBytes, rootOffset);
        if (ContainsDictionaryKey(rootDictionary, "/Names") || ContainsDictionaryKey(rootDictionary, "/AF"))
        {
            throw new InvalidOperationException(
                "The Skia PDF catalog already contains attachment structures and cannot be augmented safely.");
        }

        pdfStream.Position = pdfStream.Length;
        WriteAscii(pdfStream, "\n");

        var orderedFiles = embeddedFiles.OrderBy((q) => q.Name, StringComparer.Ordinal).ToArray();
        var firstNewObjectNumber = trailer.Size;
        var nameTreeObjectNumber = firstNewObjectNumber + orderedFiles.Length * 2;
        var newObjectOffsets = new List<(int ObjectNumber, long Offset)>();

        newObjectOffsets.Add((trailer.RootObjectNumber, pdfStream.Position));
        WriteCatalogRevision(
            pdfStream,
            trailer,
            rootDictionary,
            nameTreeObjectNumber,
            orderedFiles.Length,
            firstNewObjectNumber);

        for (var index = 0; index < orderedFiles.Length; index++)
        {
            var embeddedStreamObjectNumber = firstNewObjectNumber + index * 2;
            var fileSpecificationObjectNumber = embeddedStreamObjectNumber + 1;
            newObjectOffsets.Add((embeddedStreamObjectNumber, pdfStream.Position));
            WriteEmbeddedStream(pdfStream, embeddedStreamObjectNumber, orderedFiles[index]);
            newObjectOffsets.Add((fileSpecificationObjectNumber, pdfStream.Position));
            WriteFileSpecification(
                pdfStream,
                fileSpecificationObjectNumber,
                embeddedStreamObjectNumber,
                orderedFiles[index]);
        }

        newObjectOffsets.Add((nameTreeObjectNumber, pdfStream.Position));
        WriteNameTree(pdfStream, nameTreeObjectNumber, firstNewObjectNumber, orderedFiles);

        var xrefOffset = pdfStream.Position;
        WriteXref(pdfStream, newObjectOffsets);
        WriteTrailer(
            pdfStream,
            trailer,
            nameTreeObjectNumber + 1,
            originalXrefOffset,
            xrefOffset);
    }

    private static void WriteCatalogRevision(
        Stream stream,
        ParsedTrailer trailer,
        byte[] rootDictionary,
        int nameTreeObjectNumber,
        int fileCount,
        int firstNewObjectNumber)
    {
        WriteAscii(stream, $"{trailer.RootObjectNumber} {trailer.RootGeneration} obj\n");
        stream.Write(rootDictionary, 0, rootDictionary.Length - 2);
        WriteAscii(stream, $"\n/Names << /EmbeddedFiles {nameTreeObjectNumber} 0 R >>\n/AF [");
        for (var index = 0; index < fileCount; index++)
            WriteAscii(stream, $" {firstNewObjectNumber + index * 2 + 1} 0 R");
        WriteAscii(stream, " ]\n>>\nendobj\n");
    }

    private static void WriteEmbeddedStream(Stream stream, int objectNumber, EmbeddedFile embeddedFile)
    {
        var checksum = Convert.ToHexString(
            PdfEmbeddedFileMetadata.ComputeChecksum(embeddedFile.Content.Span));
        WriteAscii(
            stream,
            $"{objectNumber} 0 obj\n<< /Type /EmbeddedFile /Subtype /{PdfEmbeddedFileMetadata.EscapeName(embeddedFile.MediaType)}\n" +
            $"/Params << /Size {embeddedFile.Content.Length} /CheckSum <{checksum}>");
        if (embeddedFile.Created is { } created)
            WriteAscii(stream, $" /CreationDate ({PdfEmbeddedFileMetadata.FormatDate(created)})");
        if (embeddedFile.Modified is { } modified)
            WriteAscii(stream, $" /ModDate ({PdfEmbeddedFileMetadata.FormatDate(modified)})");
        WriteAscii(stream, $" >>\n/Length {embeddedFile.Content.Length} >>\nstream\n");
        stream.Write(embeddedFile.Content.Span);
        WriteAscii(stream, "\nendstream\nendobj\n");
    }

    private static void WriteFileSpecification(
        Stream stream,
        int objectNumber,
        int embeddedStreamObjectNumber,
        EmbeddedFile embeddedFile)
    {
        var name = ToPdfTextString(embeddedFile.Name);
        WriteAscii(
            stream,
            $"{objectNumber} 0 obj\n<< /Type /Filespec /F {name} /UF {name}");
        if (embeddedFile.Description is not null)
            WriteAscii(stream, $" /Desc {ToPdfTextString(embeddedFile.Description)}");
        WriteAscii(
            stream,
            $"\n/EF << /F {embeddedStreamObjectNumber} 0 R /UF {embeddedStreamObjectNumber} 0 R >>" +
            $"\n/AFRelationship /{PdfEmbeddedFileMetadata.GetRelationshipName(embeddedFile.Relationship)} >>\nendobj\n");
    }

    private static void WriteNameTree(
        Stream stream,
        int objectNumber,
        int firstNewObjectNumber,
        IReadOnlyList<EmbeddedFile> embeddedFiles)
    {
        WriteAscii(stream, $"{objectNumber} 0 obj\n<< /Names [");
        for (var index = 0; index < embeddedFiles.Count; index++)
        {
            WriteAscii(
                stream,
                $" {ToPdfTextString(embeddedFiles[index].Name)} {firstNewObjectNumber + index * 2 + 1} 0 R");
        }

        var firstName = ToPdfTextString(embeddedFiles[0].Name);
        var lastName = ToPdfTextString(embeddedFiles[^1].Name);
        WriteAscii(stream, $" ]\n/Limits [ {firstName} {lastName} ] >>\nendobj\n");
    }

    private static void WriteXref(Stream stream, IReadOnlyList<(int ObjectNumber, long Offset)> offsets)
    {
        WriteAscii(stream, "xref\n");
        foreach (var group in offsets.OrderBy((q) => q.ObjectNumber).GroupAdjacent())
        {
            WriteAscii(stream, $"{group[0].ObjectNumber} {group.Count}\n");
            foreach (var entry in group)
            {
                if (entry.Offset > 9_999_999_999L)
                    throw new InvalidOperationException("The PDF is too large for a classic cross-reference table.");
                WriteAscii(stream, $"{entry.Offset:0000000000} 00000 n \n");
            }
        }
    }

    private static void WriteTrailer(
        Stream stream,
        ParsedTrailer trailer,
        int newSize,
        long previousXrefOffset,
        long xrefOffset)
    {
        WriteAscii(
            stream,
            $"trailer\n<< /Size {newSize} /Root {trailer.RootObjectNumber} {trailer.RootGeneration} R");
        if (trailer.InfoReference is not null)
            WriteAscii(stream, $" /Info {trailer.InfoReference}");
        WriteAscii(
            stream,
            $" /Prev {previousXrefOffset} >>\nstartxref\n{xrefOffset}\n%%EOF");
    }

    private static long FindStartXref(byte[] bytes)
    {
        var markerIndex = LastIndexOf(bytes, "startxref");
        if (markerIndex < 0)
            throw new InvalidOperationException("The Skia PDF has no startxref marker.");

        var index = markerIndex + "startxref".Length;
        SkipWhitespace(bytes, ref index);
        var value = ReadInteger(bytes, ref index);
        if (value < 0 || value >= bytes.Length)
            throw new InvalidOperationException("The Skia PDF startxref offset is invalid.");
        return value;
    }

    private static ParsedTrailer ParseTrailer(byte[] bytes, long xrefOffset)
    {
        var xrefText = Encoding.ASCII.GetString(bytes, checked((int)xrefOffset), bytes.Length - checked((int)xrefOffset));
        var trailerMatch = TrailerRegex().Match(xrefText);
        if (!trailerMatch.Success)
            throw new InvalidOperationException("The Skia PDF trailer could not be parsed.");

        return new ParsedTrailer(
            int.Parse(trailerMatch.Groups["size"].Value, CultureInfo.InvariantCulture),
            int.Parse(trailerMatch.Groups["root"].Value, CultureInfo.InvariantCulture),
            int.Parse(trailerMatch.Groups["generation"].Value, CultureInfo.InvariantCulture),
            trailerMatch.Groups["info"].Success ? trailerMatch.Groups["info"].Value : null);
    }

    private static long FindObjectOffset(byte[] bytes, long xrefOffset, int objectNumber)
    {
        var xrefText = Encoding.ASCII.GetString(bytes, checked((int)xrefOffset), bytes.Length - checked((int)xrefOffset));
        var subsection = XrefSubsectionRegex().Match(xrefText);
        if (!subsection.Success)
            throw new InvalidOperationException("The Skia PDF cross-reference table could not be parsed.");

        var startObject = int.Parse(subsection.Groups["start"].Value, CultureInfo.InvariantCulture);
        var entries = XrefEntryRegex().Matches(subsection.Groups["entries"].Value);
        var entryIndex = objectNumber - startObject;
        if (entryIndex < 0 || entryIndex >= entries.Count || entries[entryIndex].Groups["state"].Value is not "n")
            throw new InvalidOperationException("The Skia PDF catalog has no usable cross-reference entry.");

        return long.Parse(entries[entryIndex].Groups["offset"].Value, CultureInfo.InvariantCulture);
    }

    private static byte[] ReadDictionary(byte[] bytes, long objectOffset)
    {
        var start = IndexOf(bytes, "<<", checked((int)objectOffset));
        if (start < 0)
            throw new InvalidOperationException("The Skia PDF catalog dictionary could not be found.");
        var end = FindDictionaryEnd(bytes, start);
        return bytes[start..end];
    }

    private static int FindDictionaryEnd(byte[] bytes, int start)
    {
        var depth = 0;
        for (var index = start; index < bytes.Length - 1; index++)
        {
            if (bytes[index] == (byte)'%')
            {
                while (index < bytes.Length && bytes[index] is not (byte)'\r' and not (byte)'\n')
                    index++;
                continue;
            }

            if (bytes[index] == (byte)'(')
            {
                SkipLiteralString(bytes, ref index);
                continue;
            }

            if (bytes[index] == (byte)'<' && bytes[index + 1] == (byte)'<')
            {
                depth++;
                index++;
            }
            else if (bytes[index] == (byte)'>' && bytes[index + 1] == (byte)'>')
            {
                depth--;
                index++;
                if (depth is 0)
                    return index + 1;
            }
        }

        throw new InvalidOperationException("The Skia PDF catalog dictionary is not balanced.");
    }

    private static void SkipLiteralString(byte[] bytes, ref int index)
    {
        var depth = 1;
        while (++index < bytes.Length && depth > 0)
        {
            if (bytes[index] == (byte)'\\')
            {
                index++;
                continue;
            }

            if (bytes[index] == (byte)'(')
                depth++;
            else if (bytes[index] == (byte)')')
                depth--;
        }
    }

    private static bool ContainsDictionaryKey(byte[] dictionary, string key)
        => Regex.IsMatch(
            Encoding.ASCII.GetString(dictionary),
            $@"(?<!\S){Regex.Escape(key)}(?=\s|/|<|\[)",
            RegexOptions.CultureInvariant);

    private static string ToPdfTextString(string value)
        => $"<FEFF{Convert.ToHexString(PdfTextEncoding.GetBytes(value))}>";

    private static void WriteAscii(Stream stream, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        stream.Write(bytes);
    }

    private static int LastIndexOf(byte[] bytes, string value)
    {
        var pattern = Encoding.ASCII.GetBytes(value);
        for (var index = bytes.Length - pattern.Length; index >= 0; index--)
        {
            if (bytes.AsSpan(index, pattern.Length).SequenceEqual(pattern))
                return index;
        }

        return -1;
    }

    private static int IndexOf(byte[] bytes, string value, int start)
    {
        var pattern = Encoding.ASCII.GetBytes(value);
        for (var index = start; index <= bytes.Length - pattern.Length; index++)
        {
            if (bytes.AsSpan(index, pattern.Length).SequenceEqual(pattern))
                return index;
        }

        return -1;
    }

    private static void SkipWhitespace(byte[] bytes, ref int index)
    {
        while (index < bytes.Length && char.IsWhiteSpace((char)bytes[index]))
            index++;
    }

    private static long ReadInteger(byte[] bytes, ref int index)
    {
        var start = index;
        while (index < bytes.Length && bytes[index] is >= (byte)'0' and <= (byte)'9')
            index++;
        if (start == index
            || !long.TryParse(
                Encoding.ASCII.GetString(bytes, start, index - start),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var value))
        {
            throw new InvalidOperationException("The Skia PDF contains an invalid integer value.");
        }

        return value;
    }

    [GeneratedRegex(
        @"trailer\s*<<(?:(?!>>).)*?/Size\s+(?<size>\d+)(?:(?!>>).)*?/Root\s+(?<root>\d+)\s+(?<generation>\d+)\s+R(?:(?!>>).)*?(?:/Info\s+(?<info>\d+\s+\d+\s+R))?(?:(?!>>).)*?>>",
        RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex TrailerRegex();

    [GeneratedRegex(
        @"\Axref\s+(?<start>\d+)\s+(?<count>\d+)\s+(?<entries>(?:\d{10}\s+\d{5}\s+[nf]\s*)+)",
        RegexOptions.CultureInvariant)]
    private static partial Regex XrefSubsectionRegex();

    [GeneratedRegex(
        @"(?<offset>\d{10})\s+(?<generation>\d{5})\s+(?<state>[nf])",
        RegexOptions.CultureInvariant)]
    private static partial Regex XrefEntryRegex();

    private sealed record ParsedTrailer(
        int Size,
        int RootObjectNumber,
        int RootGeneration,
        string? InfoReference);

    private static IReadOnlyList<List<(int ObjectNumber, long Offset)>> GroupAdjacent(
        this IEnumerable<(int ObjectNumber, long Offset)> source)
    {
        var groups = new List<List<(int ObjectNumber, long Offset)>>();
        foreach (var item in source)
        {
            if (groups.Count is 0 || groups[^1][^1].ObjectNumber + 1 != item.ObjectNumber)
                groups.Add(new List<(int ObjectNumber, long Offset)>());
            groups[^1].Add(item);
        }

        return groups;
    }
}
