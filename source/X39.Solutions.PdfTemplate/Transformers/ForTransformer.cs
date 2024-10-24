using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using X39.Solutions.PdfTemplate.Abstraction;
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
    public async IAsyncEnumerable<XmlNode> TransformAsync(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<XmlNode> nodes,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var scope = templateData.Scope("for");
        var match = ParseArguments().Match(remainingLine);
        if (!match.Success)
            throw new ArgumentException("Invalid arguments.", nameof(remainingLine));
        var variable = match.Groups["variable"].Value;
        var from = Convert.ToDouble(
            await templateData.EvaluateAsync(cultureInfo, match.Groups["from"].Value, cancellationToken)
                .ConfigureAwait(false),
            CultureInfo.InvariantCulture);
        var to = Convert.ToDouble(
            await templateData.EvaluateAsync(cultureInfo, match.Groups["to"].Value, cancellationToken)
                .ConfigureAwait(false),
            CultureInfo.InvariantCulture);
        var step = match.Groups["step"].Success
            ? Convert.ToDouble(
                await templateData.EvaluateAsync(cultureInfo, match.Groups["step"].Value, cancellationToken)
                    .ConfigureAwait(false),
                CultureInfo.InvariantCulture)
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