using System.Globalization;
using System.Text.RegularExpressions;
using XmlNode = X39.Solutions.PdfTemplate.Xml.XmlNode;

namespace X39.Solutions.PdfTemplate.Transformers;

/// <summary>
/// A transformer that repeats the given nodes for a given range.
/// </summary>
public partial class ForTransformer : ITransformer
{
    [GeneratedRegex(
        @"\A\s*(?<variable>[a-zA-Z][a-zA-Z0-9_]*)\s+from\s+(?<from>.+?)\s+to\s+(?<to>.+?)(\s+step\s+(?<step>.+?))?\s*\z")]
    private static partial Regex ParseArguments();

    /// <inheritdoc />
    public string Name => "for";

    /// <inheritdoc />
    public IEnumerable<XmlNode> Transform(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<XmlNode> nodes)
    {
        using var scope = templateData.Scope("for");
        var match = ParseArguments().Match(remainingLine);
        if (!match.Success)
            throw new ArgumentException("Invalid arguments.", nameof(remainingLine));
        var variable = match.Groups["variable"].Value;
        var from = Convert.ToDouble(templateData.Evaluate(cultureInfo, match.Groups["from"].Value), CultureInfo.InvariantCulture);
        var to = Convert.ToDouble(templateData.Evaluate(cultureInfo, match.Groups["to"].Value), CultureInfo.InvariantCulture);
        var step = match.Groups["step"].Success
            ? Convert.ToDouble(templateData.Evaluate(cultureInfo, match.Groups["step"].Value), CultureInfo.InvariantCulture)
            : 1D;
        var body = nodes;
        if (from < to && step < 0)
            throw new ArgumentException("Step must be positive.", nameof(remainingLine));
        if (from > to && step > 0)
            throw new ArgumentException("Step must be negative.", nameof(remainingLine));
        if (from < to)
        {
            for (; from < to; from += step)
            {
                templateData.SetVariable(variable, from);
                foreach (var node in body)
                    yield return node.DeepCopy();
            }
        }
        else
        {
            for (; from > to; from += step)
            {
                templateData.SetVariable(variable, from);
                foreach (var node in body)
                    yield return node.DeepCopy();
            }
        }
    }
}