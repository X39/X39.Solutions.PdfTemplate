namespace X39.Solutions.PdfTemplate.Xml.Exceptions;

/// <summary>
/// Thrown when the evaluation of a transformation expression fails during the transformation of an XML document.
/// </summary>
public sealed class TransformationEvaluationFailedException : XmlTemplateTransformationException
{
    /// <summary>
    /// Represents an expression used in XML template transformation.
    /// </summary>
    public string Expression { get; }

    internal TransformationEvaluationFailedException(string expression, XmlNode node, Exception exception)
        : base(
            $"Failed to evaluate expression '{expression}' at L{node.Line}:C{node.Column} with exception: {exception.Message}",
            exception,
            node
        )
    {
        Expression = expression;
    }
}
