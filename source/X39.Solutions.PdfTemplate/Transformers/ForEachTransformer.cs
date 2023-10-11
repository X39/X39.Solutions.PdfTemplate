using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using XmlNode = X39.Solutions.PdfTemplate.Xml.XmlNode;

namespace X39.Solutions.PdfTemplate.Transformers;

/// <summary>
/// A transformer that repeats the given nodes for a given range.
/// </summary>
public partial class ForEachTransformer : ITransformer
{
    [GeneratedRegex(
        @"\A\s*(?<variable>[a-zA-Z][a-zA-Z0-9_]*)\s+in\s+(?<in>.+?)(\s+with\s+(?<indexVariable>[a-zA-Z][a-zA-Z0-9_]*))?\s*\z")]
    private static partial Regex ParseArguments();

    /// <inheritdoc />
    public string Name => "forEach";

    /// <inheritdoc />
    public IEnumerable<XmlNode> Transform(
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<XmlNode> nodes)
    {
        using var scope = templateData.Scope("forEach");
        var match = ParseArguments().Match(remainingLine);
        if (!match.Success)
            throw new ArgumentException("Invalid arguments.", nameof(remainingLine));
        var variable = match.Groups["variable"].Value;
        var @in = templateData.Evaluate(match.Groups["in"].Value);
        if (@in is not IEnumerable enumerable)
            throw new ArgumentException("In must be an enumerable.", nameof(remainingLine));
        if (match.Groups["indexVariable"].Success)
        {
            var indexVariable = match.Groups["indexVariable"].Value;
            var index = 0;
            foreach (var item in enumerable)
            {
                templateData.SetVariable(variable, item);
                templateData.SetVariable(indexVariable, index++);
                foreach (var node in nodes)
                    yield return node.DeepCopy();
            }
        }
        else
        {
            foreach (var item in enumerable)
            {
                templateData.SetVariable(variable, item);
                foreach (var node in nodes)
                    yield return node.DeepCopy();
            }
        }
    }
}