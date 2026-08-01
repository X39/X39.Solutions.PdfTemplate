using PdfSharp.Pdf;

namespace X39.Solutions.Papercraft.Rendering.PdfSharp.Services;

internal static class PdfSharpEmbeddedFileWriter
{
    public static void Write(PdfDocument pdfDocument, IReadOnlyList<EmbeddedFile> embeddedFiles)
    {
        ArgumentNullException.ThrowIfNull(pdfDocument);
        ArgumentNullException.ThrowIfNull(embeddedFiles);
        if (embeddedFiles.Count is 0)
            return;

        var associatedFiles = new PdfArray(pdfDocument);
        foreach (var embeddedFile in embeddedFiles.OrderBy((q) => q.Name, StringComparer.Ordinal))
        {
            pdfDocument.AddEmbeddedFile(
                embeddedFile.Name,
                new MemoryStream(embeddedFile.Content.ToArray(), writable: false));

            var fileSpecification = GetFileSpecification(pdfDocument, embeddedFile.Name);
            ApplyFileSpecificationMetadata(fileSpecification, embeddedFile);
            ApplyStreamMetadata(fileSpecification, embeddedFile);
            associatedFiles.Elements.Add(fileSpecification.ReferenceNotNull);
        }

        pdfDocument.Internals.Catalog.Elements.SetObject("/AF", associatedFiles);
    }

    private static PdfDictionary GetFileSpecification(PdfDocument pdfDocument, string name)
    {
        var names = pdfDocument.Internals.Catalog.Elements.GetDictionary("/Names")
                    ?? throw new InvalidOperationException("PDFsharp did not create a document name dictionary.");
        var embeddedFiles = names.Elements.GetDictionary("/EmbeddedFiles")
                            ?? throw new InvalidOperationException("PDFsharp did not create an embedded-file name tree.");
        var nameTree = new PdfNameTreeNode(embeddedFiles);
        return nameTree.GetValue(name) as PdfDictionary
               ?? throw new InvalidOperationException($"PDFsharp did not create a file specification for '{name}'.");
    }

    private static void ApplyFileSpecificationMetadata(
        PdfDictionary fileSpecification,
        EmbeddedFile embeddedFile)
    {
        fileSpecification.Elements["/F"] = new PdfString(embeddedFile.Name, PdfStringEncoding.PDFDocEncoding);
        fileSpecification.Elements["/UF"] = new PdfString(embeddedFile.Name, PdfStringEncoding.Unicode);
        fileSpecification.Elements.SetName(
            "/AFRelationship",
            "/" + PdfEmbeddedFileMetadata.GetRelationshipName(embeddedFile.Relationship));

        if (embeddedFile.Description is not null)
        {
            fileSpecification.Elements["/Desc"] = new PdfString(
                embeddedFile.Description,
                PdfStringEncoding.Unicode);
        }
    }

    private static void ApplyStreamMetadata(
        PdfDictionary fileSpecification,
        EmbeddedFile embeddedFile)
    {
        var embeddedFileReferences = fileSpecification.Elements.GetDictionary("/EF")
                                     ?? throw new InvalidOperationException("The file specification has no embedded-file reference dictionary.");
        var embeddedStream = embeddedFileReferences.Elements.GetDictionary("/F")
                             ?? throw new InvalidOperationException("The file specification has no embedded-file stream.");
        embeddedStream.Elements.SetName(
            "/Subtype",
            "/" + embeddedFile.MediaType);

        var parameters = embeddedStream.Elements.GetDictionary("/Params")
                         ?? new PdfDictionary(fileSpecification.Owner);
        parameters.Elements.SetInteger("/Size", embeddedFile.Content.Length);
        parameters.Elements["/CheckSum"] = new PdfLiteral(
            $"<{Convert.ToHexString(PdfEmbeddedFileMetadata.ComputeChecksum(embeddedFile.Content.Span))}>");
        if (embeddedFile.Created is { } created)
            parameters.Elements.SetString("/CreationDate", PdfEmbeddedFileMetadata.FormatDate(created));
        else
            parameters.Elements.Remove("/CreationDate");
        if (embeddedFile.Modified is { } modified)
            parameters.Elements.SetString("/ModDate", PdfEmbeddedFileMetadata.FormatDate(modified));
        else
            parameters.Elements.Remove("/ModDate");

        embeddedStream.Elements.SetObject("/Params", parameters);
    }
}
