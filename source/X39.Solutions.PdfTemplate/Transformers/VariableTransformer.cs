using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Transformers;

/// <summary>
/// Transformer offering variables for easier access.
/// </summary>
public partial class VariableTransformer : ITransformer
{
    // variable '=' value { ',' variable '=' value }
    [GeneratedRegex(
        @"\A\s*(?<variable>[a-zA-Z][-a-zA-Z0-9_]+)\s*=\s*(?<value>.+?)\s*(?:,\s*(?<variable>[a-zA-Z][-a-zA-Z0-9_]+)\s*=\s*(?<value>.+?)\s*)*\z"
    )]
    private static partial Regex ParseArguments();

    /// <inheritdoc />
    public string Name => "var";

    /// <inheritdoc />
    public async IAsyncEnumerable<XmlNode> TransformAsync(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<XmlNode> nodes,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var match = ParseArguments().Match(remainingLine);
        if (!match.Success)
            throw new ArgumentException("Invalid arguments.", nameof(remainingLine));
        var variables = match.Groups["variable"].Captures;
        var values = match.Groups["value"].Captures;
        if (variables.Count != values.Count)
            throw new ArgumentException("Invalid arguments.", nameof(remainingLine));
        using var scope = templateData.Scope("variable");
        for (var i = 0; i < variables.Count; i++)
        {
            var variable = variables[i].Value;
            var valueString = values[i].Value;
            var value = await templateData.EvaluateAsync(cultureInfo, valueString, cancellationToken);
            templateData.SetVariable(variable, value);
        }

        foreach (var node in nodes)
            yield return node;
    }
}