using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Transformers;

/// <summary>
/// The AlternateTransformer class is a transformer that allows repeated alternation
/// of values in a template.
/// </summary>
public sealed partial class AlternateTransformer : ITransformer
{
    // [repeat] on IDENTIFIER with [VALUE1, VALUE2, ...]
    [GeneratedRegex(
        @"\A\s*(?:(?<repeat>repeat)?\s*on\s+(?<variable>[a-zA-Z][a-zA-Z0-9_]*)|on\s+(?<variable>[a-zA-Z][a-zA-Z0-9_]*)\s+with\s+\[(?:(?<values>.+?)\s*,\s*)*(?<values>.+?),?\])\s*\z"
    )]
    private static partial Regex ParseArguments();

    private readonly record struct AlternateValue(int Index, object?[] Values);

    /// <inheritdoc />
    public string Name => "alternate";

    /// <inheritdoc />
    public async IAsyncEnumerable<XmlNode> TransformAsync(
        CultureInfo                                cultureInfo,
        ITemplateData                              templateData,
        string                                     remainingLine,
        IReadOnlyCollection<XmlNode>               nodes,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var match = ParseArguments().Match(remainingLine);
        if (!match.Success)
            throw new ArgumentException("Invalid arguments.", nameof(remainingLine));
        var isRepeat = match.Groups["repeat"].Success;
        var variable = match.Groups["variable"].Value;
        if (isRepeat)
        {
            if (templateData.GetTransformerData<AlternateValue?>(variable) is not var (index, objects))
                throw new InvalidOperationException(
                    "Alternate transformer was not used prior to repeat with the same variable."
                );

            using var scope = templateData.Scope("alternate");
            templateData.SetVariable(variable, objects[index]);
            foreach (var node in nodes)
                yield return node.DeepCopy();
        }
        else
        {
            var valueTasks = match.Groups["values"]
                                  .Captures.Select(
                                      c => templateData.EvaluateAsync(cultureInfo, c.Value, cancellationToken).AsTask()
                                  )
                                  .ToArray();
            await Task.WhenAll(valueTasks).ConfigureAwait(false);
            var values = valueTasks.Select(t => t.Result).ToArray();
            if (templateData.GetTransformerData<AlternateValue?>(variable) is not {} alternateValue
                || (values.Length > 0 && !alternateValue.Values.SequenceEqual(values)))
                alternateValue = new AlternateValue(values.Length - 1, values);

            var index = (alternateValue.Index + 1) % alternateValue.Values.Length;
            templateData.SetTransformerData<AlternateValue?>(variable, alternateValue with { Index = index });

            using var scope = templateData.Scope("alternate");
            templateData.SetVariable(variable, alternateValue.Values[index]);
            foreach (var node in nodes)
                yield return node.DeepCopy();
        }
    }
}
