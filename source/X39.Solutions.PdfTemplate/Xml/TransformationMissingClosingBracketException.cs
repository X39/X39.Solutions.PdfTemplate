namespace X39.Solutions.PdfTemplate.Xml;

/// <summary>
/// Thrown during the transformation of an XML document if a transformer is missing a closing bracket.
/// </summary>
public sealed class TransformationMissingClosingBracketException : XmlTemplateTransformationException
{
    /// <summary>
    /// The transformer text that failed to parse.
    /// </summary>
    public string TransformerText { get; }
    internal TransformationMissingClosingBracketException(string text, XmlNode node)
        : base(
            $"Failed to parse transformer expression '{text}' at L{node.Line}:C{node.Column}, missing closing bracket ('}}').",
            node)
    {
        TransformerText = text;
    }
}