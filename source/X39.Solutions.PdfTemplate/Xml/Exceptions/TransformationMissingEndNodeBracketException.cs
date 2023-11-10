namespace X39.Solutions.PdfTemplate.Xml.Exceptions;

/// <summary>
/// Thrown during the transformation of an XML document if a transformer is missing an end node.
/// </summary>
public sealed class TransformationMissingEndNodeBracketException : XmlTemplateTransformationException
{
    /// <summary>
    /// The transformer text that failed to parse.
    /// </summary>
    public string TransformerText { get; }
    internal TransformationMissingEndNodeBracketException(string text, XmlNode node)
        : base(
            $"Failed to parse transformer expression '{text}' at L{node.Line}:C{node.Column}, end node is null.",
            node)
    {
        TransformerText = text;
    }
}