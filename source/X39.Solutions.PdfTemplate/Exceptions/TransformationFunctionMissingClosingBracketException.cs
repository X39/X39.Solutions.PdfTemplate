using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Exceptions;

/// <summary>
/// Thrown during the transformation of an XML document if a function is missing a closing bracket.
/// </summary>
public sealed class TransformationFunctionMissingClosingBracketException : XmlTemplateTransformationException
{
    /// <summary>
    /// The function text that failed to parse.
    /// </summary>
    public string FunctionText { get; }

    /// <summary>
    /// The number of brackets that are missing.
    /// </summary>
    public int BracketsMissing { get; }

    internal TransformationFunctionMissingClosingBracketException(string text, XmlNode node, int bracketsMissing)
        : base(
            $"Failed to parse function expression '{text}' at L{node.Line}:C{node.Column}, missing closing bracket (bracket count: {bracketsMissing}).",
            node
        )
    {
        FunctionText    = text;
        BracketsMissing = bracketsMissing;
    }
}