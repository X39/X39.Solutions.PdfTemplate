using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using PdfSharp.Pdf;
using PdfSharp.Pdf.Advanced;
using PdfSharp.Pdf.IO;
using X39.Solutions.Papercraft;
using X39.Solutions.Papercraft.Data;
using X39.Solutions.Papercraft.Display;
using X39.Solutions.Papercraft.Rendering.EscPos;
using X39.Solutions.Papercraft.Rendering.PdfSharp;
using X39.Solutions.Papercraft.Rendering.SkiaSharp;
using X39.Solutions.Papercraft.Rendering.SkiaSharp.Services;
using X39.Solutions.Papercraft.Rendering.Svg;

namespace X39.Solutions.PdfTemplate.Test;

public sealed class PapercraftEmbeddedFileTests
{
    private static readonly DateTimeOffset Created = new(2026, 6, 13, 15, 16, 20, TimeSpan.FromHours(2.5));
    private static readonly DateTimeOffset Modified = new(2026, 7, 14, 8, 9, 10, TimeSpan.Zero);

    [Fact]
    public async Task PdfSharpWritesCompleteEmbeddedFileMetadata()
    {
        var attachments = CreateAttachments();
        var bytes = await RenderPdfSharpAsync(CreateDocument(attachments));

        AssertAttachments(bytes, attachments);
    }

    [Fact]
    public async Task SkiaSharpWritesCompleteEmbeddedFileMetadata()
    {
        var attachments = CreateAttachments();
        var bytes = await RenderSkiaSharpAsync(CreateDocument(attachments));

        AssertAttachments(bytes, attachments);
        Assert.DoesNotContain("pdfaid:part>2", Encoding.ASCII.GetString(bytes), StringComparison.Ordinal);
    }

    [Fact]
    public async Task BothPdfBackendsWriteEmptyAttachmentContent()
    {
        var attachment = new EmbeddedFile("empty.bin", ReadOnlyMemory<byte>.Empty);
        var document = CreateDocument(new[] { attachment });

        AssertAttachments(await RenderPdfSharpAsync(document), new[] { attachment });
        AssertAttachments(await RenderSkiaSharpAsync(document), new[] { attachment });
    }

    [Fact]
    public async Task PreparedDocumentSnapshotsAttachmentListAndContentAndCanRenderRepeatedly()
    {
        var source = new byte[] { 1, 2, 3, 4 };
        var attachment = new EmbeddedFile("stable.bin", source);
        var attachmentList = new List<EmbeddedFile> { attachment };
        var document = CreateDocument(attachmentList);
        source[0] = 99;
        attachmentList.Clear();

        var pdfSharpFirst = await RenderPdfSharpAsync(document);
        var pdfSharpSecond = await RenderPdfSharpAsync(document);
        var skiaFirst = await RenderSkiaSharpAsync(document);
        var skiaSecond = await RenderSkiaSharpAsync(document);

        var expected = new[] { new EmbeddedFile("stable.bin", new byte[] { 1, 2, 3, 4 }) };
        AssertAttachments(pdfSharpFirst, expected);
        AssertAttachments(pdfSharpSecond, expected);
        AssertAttachments(skiaFirst, expected);
        AssertAttachments(skiaSecond, expected);
    }

    [Fact]
    public async Task PdfBackendsSupportAttachmentsOnZeroPageDocuments()
    {
        var attachment = EmbeddedFile.FromText("empty-document.txt", "attached");
        var document = new PapercraftDocument(
            Array.Empty<PapercraftPage>(),
            CultureInfo.InvariantCulture,
            new DocumentOptions { EmbeddedFiles = new[] { attachment } });

        AssertAttachments(await RenderPdfSharpAsync(document), new[] { attachment });
        AssertAttachments(await RenderSkiaSharpAsync(document), new[] { attachment });
    }

    [Fact]
    public async Task SkiaSharpWritesAttachmentsToNonSeekableDestination()
    {
        var attachment = EmbeddedFile.FromText("stream.txt", "non-seekable");
        var document = CreateDocument(new[] { attachment });
        using var destination = new WriteOnlyNonSeekableStream();
        using var paintCache = new SkPaintCache();
        var backend = new SkiaSharpRenderBackend(new SkiaSharpDisplayListRenderer(paintCache), paintCache);

        await backend.RenderAsync(document, new RenderOutput(RenderTarget.Pdf, destination));

        AssertAttachments(destination.ToArray(), new[] { attachment });
    }

    [Fact]
    public void EmbeddedFileValidationReportsAllInvalidMetadata()
    {
        var invalidUnicode = "bad\ud800";
        var files = new EmbeddedFile[]
        {
            null!,
            new(" ", Array.Empty<byte>()),
            new("..", Array.Empty<byte>()),
            new("folder/file.txt", Array.Empty<byte>()),
            new("duplicate.txt", Array.Empty<byte>()),
            new("DUPLICATE.TXT", Array.Empty<byte>()),
            new(invalidUnicode, Array.Empty<byte>()),
            new("media.txt", Array.Empty<byte>()) { MediaType = "not-a-media-type" },
            new("wildcard.txt", Array.Empty<byte>()) { MediaType = "text/*" },
            new("relationship.txt", Array.Empty<byte>()) { Relationship = (EmbeddedFileRelationship)999 },
            new("description.txt", Array.Empty<byte>()) { Description = invalidUnicode },
        };

        var diagnostics = EmbeddedFileValidation.GetDiagnostics(files);

        Assert.All(diagnostics, (q) => Assert.Equal(RenderDiagnosticCodes.InvalidEmbeddedFile, q.Code));
        Assert.All(diagnostics, (q) => Assert.Equal(RendererFeatures.EmbeddedFiles, q.Feature));
        Assert.Contains(diagnostics, (q) => q.Message.Contains("null embedded file", StringComparison.Ordinal));
        Assert.Contains(diagnostics, (q) => q.Message.Contains("empty or whitespace", StringComparison.Ordinal));
        Assert.Contains(diagnostics, (q) => q.Message.Contains("path separators", StringComparison.Ordinal));
        Assert.Contains(diagnostics, (q) => q.Message.Contains("used more than once", StringComparison.Ordinal));
        Assert.Contains(diagnostics, (q) => q.Message.Contains("invalid Unicode", StringComparison.Ordinal));
        Assert.Contains(diagnostics, (q) => q.Message.Contains("valid, concrete media type", StringComparison.Ordinal));
        Assert.Contains(diagnostics, (q) => q.Message.Contains("unknown associated-file relationship", StringComparison.Ordinal));
    }

    [Fact]
    public async Task UnsupportedBackendsReportAttachmentsInsteadOfDroppingThem()
    {
        var document = CreateDocument(new[] { EmbeddedFile.FromText("data.txt", "payload") });
        using var paintCache = new SkPaintCache();
        var skia = new SkiaSharpRenderBackend(new SkiaSharpDisplayListRenderer(paintCache), paintCache);

        var validations = new[]
        {
            await skia.ValidateAsync(document, RenderTarget.ImagePng),
            await new SvgRenderBackend().ValidateAsync(document, RenderTarget.FromMediaType(SvgRenderBackend.MediaType)),
            await new EscPosRenderBackend().ValidateAsync(document, EscPosRenderBackend.Target),
        };

        Assert.All(
            validations,
            (validation) => Assert.Contains(
                validation.Diagnostics,
                (q) => q.Code == RenderDiagnosticCodes.UnsupportedFeature
                       && q.Feature == RendererFeatures.EmbeddedFiles));
    }

    [Fact]
    public void UndeclaredRendererFeaturesAreUnsupported()
    {
        var capabilities = new RendererCapabilities(
            "minimal",
            "Minimal",
            RendererOutputKind.Pdf,
            new[] { PapercraftMediaTypes.ApplicationPdf });
        var document = CreateDocument(new[] { EmbeddedFile.FromText("data.txt", "payload") });

        var validation = capabilities.ValidateDocument(document);

        Assert.Contains(
            validation.Diagnostics,
            (q) => q.Code == RenderDiagnosticCodes.UnsupportedFeature
                   && q.Feature == RendererFeatures.EmbeddedFiles);
    }

    [Fact]
    public async Task LoweredXmlReportsAttachmentsAndWritesNoOutput()
    {
        var services = new ServiceCollection();
        services.AddPapercraftCore();
        await using var provider = services.BuildServiceProvider();
        var papercraft = provider.GetRequiredService<global::X39.Solutions.Papercraft.Papercraft>();
        await using var session = papercraft.CreateSession();
        await using var output = new MemoryStream();
        using var reader = XmlReader.Create(new StringReader("<text>payload</text>"));
        var options = new PapercraftRenderOptions
        {
            DocumentOptions = new DocumentOptions
            {
                EmbeddedFiles = new[] { EmbeddedFile.FromText("data.txt", "payload") },
            },
        };

        var exception = await Assert.ThrowsAsync<RenderValidationException>(
            async () => await session.RenderAsync(
                reader,
                new RenderOutput(RenderTarget.LoweredXml, output),
                CultureInfo.InvariantCulture,
                options));

        Assert.Equal(0, output.Length);
        Assert.Contains(
            exception.ValidationResult.Diagnostics,
            (q) => q.Code == RenderDiagnosticCodes.UnsupportedFeature
                   && q.Feature == RendererFeatures.EmbeddedFiles);
    }

    private static EmbeddedFile[] CreateAttachments()
        =>
        [
            new EmbeddedFile("dätä.json", new byte[] { 0, 1, 2, 3, 0xFE, 0xFF })
            {
                MediaType = "application/json",
                Description = "Machine-readable payload ✓",
                Relationship = EmbeddedFileRelationship.Data,
                Created = Created,
                Modified = Modified,
            },
            EmbeddedFile.FromText("source.txt", "Papercraft attachment") with
            {
                Description = "Original source",
                Relationship = EmbeddedFileRelationship.Source,
                Modified = Modified,
            },
        ];

    private static PapercraftDocument CreateDocument(IReadOnlyList<EmbeddedFile> attachments)
        => new(
            new[]
            {
                new PapercraftPage(
                    0,
                    1,
                    1,
                    new Size(96, 72),
                    DocumentOptions.Default.DotsPerMillimeter,
                    new DisplayList()),
            },
            CultureInfo.InvariantCulture,
            new DocumentOptions
            {
                Modified = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                Producer = "Papercraft tests",
                EmbeddedFiles = attachments,
            });

    private static async Task<byte[]> RenderPdfSharpAsync(PapercraftDocument document)
    {
        await using var stream = new MemoryStream();
        await new PdfSharpRenderBackend().RenderAsync(document, new RenderOutput(RenderTarget.Pdf, stream));
        return stream.ToArray();
    }

    private static async Task<byte[]> RenderSkiaSharpAsync(PapercraftDocument document)
    {
        using var paintCache = new SkPaintCache();
        var backend = new SkiaSharpRenderBackend(new SkiaSharpDisplayListRenderer(paintCache), paintCache);
        await using var stream = new MemoryStream();
        await backend.RenderAsync(document, new RenderOutput(RenderTarget.Pdf, stream));
        return stream.ToArray();
    }

    private static void AssertAttachments(byte[] pdfBytes, IReadOnlyList<EmbeddedFile> expectedFiles)
    {
        using var stream = new MemoryStream(pdfBytes);
        using var pdfDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Modify);
        var catalog = pdfDocument.Internals.Catalog;
        var names = Assert.IsType<PdfDictionary>(catalog.Elements.GetDictionary("/Names"));
        var embeddedFilesDictionary = Assert.IsType<PdfDictionary>(names.Elements.GetDictionary("/EmbeddedFiles"));
        var nameTree = new PdfNameTreeNode(embeddedFilesDictionary);
        var associatedFiles = Assert.IsType<PdfArray>(catalog.Elements.GetArray("/AF"));

        Assert.Equal(expectedFiles.Count, nameTree.NamesCountTotal);
        Assert.Equal(expectedFiles.Count, associatedFiles.Elements.Count);
        foreach (var expected in expectedFiles)
        {
            var fileSpecification = Assert.IsType<PdfDictionary>(nameTree.GetValue(expected.Name, includeKids: true));
            Assert.Equal(expected.Name, fileSpecification.Elements.GetString("/F"));
            Assert.Equal(expected.Name, fileSpecification.Elements.GetString("/UF"));
            Assert.Equal(expected.Description ?? "", fileSpecification.Elements.GetString("/Desc"));
            Assert.Equal(
                "/" + PdfEmbeddedFileMetadata.GetRelationshipName(expected.Relationship),
                fileSpecification.Elements.GetName("/AFRelationship"));

            var embeddedFileReferences = Assert.IsType<PdfDictionary>(fileSpecification.Elements.GetDictionary("/EF"));
            var embeddedStream = Assert.IsType<PdfDictionary>(embeddedFileReferences.Elements.GetDictionary("/F"));
            Assert.False(embeddedStream.Elements.ContainsKey("/Filter"));
            Assert.Equal(
                "/" + expected.MediaType,
                embeddedStream.Elements.GetName("/Subtype"));
            Assert.Equal(expected.Content.ToArray(), embeddedStream.Stream?.UnfilteredValue);

            var parameters = Assert.IsType<PdfDictionary>(embeddedStream.Elements.GetDictionary("/Params"));
            Assert.Equal(expected.Content.Length, parameters.Elements.GetInteger("/Size"));
            var checksum = Assert.IsType<PdfString>(parameters.Elements.GetValue("/CheckSum"));
            Assert.Equal(MD5.HashData(expected.Content.Span), checksum.Value.Select((q) => (byte)q).ToArray());
            Assert.Equal(
                expected.Created is { } created ? PdfEmbeddedFileMetadata.FormatDate(created) : "",
                parameters.Elements.GetString("/CreationDate"));
            Assert.Equal(
                expected.Modified is { } modified ? PdfEmbeddedFileMetadata.FormatDate(modified) : "",
                parameters.Elements.GetString("/ModDate"));
        }
    }

    private sealed class WriteOnlyNonSeekableStream : Stream
    {
        private readonly MemoryStream _inner = new();

        public byte[] ToArray() => _inner.ToArray();

        public override void Flush() => _inner.Flush();

        public override void Write(byte[] buffer, int offset, int count)
            => _inner.Write(buffer, offset, count);

        public override void Write(ReadOnlySpan<byte> buffer) => _inner.Write(buffer);

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
