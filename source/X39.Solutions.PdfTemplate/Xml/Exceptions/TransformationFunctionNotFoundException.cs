namespace X39.Solutions.PdfTemplate.Xml.Exceptions;

/// <summary>
/// The exception that is thrown when a function could not be found.
/// </summary>
public class TransformationFunctionNotFoundException : XmlTemplateTransformationException
{
    /// <summary>
    /// The name of the function that was not found.
    /// </summary>
    public string FunctionName { get; set; }

    internal TransformationFunctionNotFoundException(string text, XmlNode node, string name)
        : base(
            $"Failed to find a corresponding function for '{name}' at L{node.Line}:C{node.Column} in '{text}'.",
            node)
    {
        FunctionName = name;
    }
}