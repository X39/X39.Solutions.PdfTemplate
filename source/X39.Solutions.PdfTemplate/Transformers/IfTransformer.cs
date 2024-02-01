using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using XmlNode = X39.Solutions.PdfTemplate.Xml.XmlNode;

namespace X39.Solutions.PdfTemplate.Transformers;

/// <summary>
/// A transformer that repeats the given nodes for a given range.
/// </summary>
public partial class IfTransformer : ITransformer
{
    [GeneratedRegex(@"\A\s*(?<leftExpression>.+?)(?:\s+(?<operator>[><=!]{1,3}|in)\s+(?<rightExpression>.+?))?\s*\z")]
    private static partial Regex ParseArguments();

    /// <inheritdoc />
    public string Name => "if";

    /// <inheritdoc />
    public async IAsyncEnumerable<XmlNode> TransformAsync(
        CultureInfo cultureInfo,
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<XmlNode> nodes,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var scope = templateData.Scope("if");
        var match = ParseArguments().Match(remainingLine);
        if (!match.Success)
            throw new ArgumentException(
                "Expression could not be parsed, please make sure it is of the form: " +
                "@if <expression> [operator <expression>] (supported operators: >, <, >=, <=, ==, !=, ===, !==, in)",
                nameof(remainingLine));
        var leftExpressionString = match.Groups["leftExpression"].Value;
        var leftExpression = await templateData.EvaluateAsync(cultureInfo, leftExpressionString, cancellationToken)
            .ConfigureAwait(false);

        if (!match.Groups["operator"].Success)
        {
            if (leftExpression is bool flag)
            {
                if (flag)
                    foreach (var node in nodes)
                        yield return node.DeepCopy();
            }
            else
            {
                throw new ArgumentException(
                    "The left expression could not be evaluated to a boolean but has to be if no operator is given.",
                    nameof(remainingLine));
            }

            yield break;
        }

        var @operator = match.Groups["operator"].Value;
        var rightExpressionString = match.Groups["rightExpression"].Value;
        var rightExpression = await templateData.EvaluateAsync(cultureInfo, rightExpressionString, cancellationToken)
            .ConfigureAwait(false);
        var result = @operator switch
        {
            ">"   => Compare(leftExpression, rightExpression) > 0,
            "<"   => Compare(leftExpression, rightExpression) < 0,
            ">="  => Compare(leftExpression, rightExpression) >= 0,
            "<="  => Compare(leftExpression, rightExpression) <= 0,
            "=="  => Similar(leftExpression, rightExpression),
            "!="  => !Similar(leftExpression, rightExpression),
            "===" => Same(leftExpression, rightExpression),
            "!==" => !Same(leftExpression, rightExpression),
            "in"  => Contains(leftExpression, rightExpression),
            _ => throw new ArgumentException(
                "Invalid operator, only >, <, >=, <=, ==, !=, ===, !==, in are supported.",
                nameof(remainingLine)),
        };
        if (!result)
            yield break;
        foreach (var node in nodes)
            yield return node.DeepCopy();
    }

    private static bool Similar(object? leftExpression, object? rightExpression)
    {
        if (leftExpression is null && rightExpression is null)
            return true;
        if (leftExpression is string leftString && rightExpression is string rightString)
            return string.Equals(leftString, rightString, StringComparison.OrdinalIgnoreCase);
        return Same(leftExpression, rightExpression);
    }

    private static bool Same(object? leftExpression, object? rightExpression)
    {
        return Equals(leftExpression, rightExpression);
    }

    private static bool Contains(object? leftExpression, object? rightExpression)
    {
        return rightExpression switch
        {
            string rightString when leftExpression is string leftString => rightString.Contains(leftString),
            string rightString when leftExpression is char leftChar => rightString.Contains(leftChar),
            IEnumerable enumerable => enumerable.Cast<object?>().Contains(leftExpression),
            _ => throw new ArgumentException("Right expression must be enumerable.", nameof(rightExpression)),
        };
    }

    private static int Compare(object? variableValue, object? value)
    {
        if (variableValue is null && value is null)
            return 0;
        if (variableValue is null)
            return -1;
        if (value is null)
            return 1;
        if (variableValue is IComparable comparable)
            return comparable.CompareTo(value);
        throw new ArgumentException("Variable is not comparable.", nameof(variableValue));
    }
}