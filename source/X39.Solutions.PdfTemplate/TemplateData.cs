using System.Diagnostics.CodeAnalysis;
using System.Text;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Exceptions;

namespace X39.Solutions.PdfTemplate;

internal sealed class TemplateData : ITemplateData
{
    private record struct VariableScope(
        [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local", Justification = "Debug help")] string Name,
        Dictionary<string, object?>                                                  Variables);

    private readonly Stack<VariableScope>                _variableStack   = new();
    private readonly Dictionary<string, IFunction>       _functions       = new();
    private readonly Dictionary<(Type, string), object?> _transformerData = new();

    public TemplateData() { _variableStack.Push(new VariableScope("__root__", new Dictionary<string, object?>())); }

    public IDisposable Scope(string scopeName)
    {
        var dict = _variableStack.Count != 0
            ? new VariableScope(scopeName, new Dictionary<string, object?>(_variableStack.Peek().Variables))
            : new VariableScope(scopeName, new Dictionary<string, object?>());
        _variableStack.Push(dict);
        return new Disposable(() => _variableStack.Pop());
    }

    public IDisposable Scope(string scopeName, ITemplateData baseData)
    {
        var disposable = Scope(scopeName);
        foreach (var function in baseData.Functions)
            RegisterFunction(function);
        foreach (var (key, value) in baseData.Variables)
            SetVariable(key, value);
        return disposable;
    }

    public IDisposable Scope(string scopeName, Dictionary<string, object?> data)
    {
        _variableStack.Push(new VariableScope(scopeName, data));
        return new Disposable(() => _variableStack.Pop());
    }

    public IEnumerable<IFunction> Functions => _functions.Values;

    public IEnumerable<KeyValuePair<string, object?>> Variables => _variableStack
                                                                   .SelectMany((d) => d.Variables)
                                                                   .DistinctBy((q) => q.Key);

    public void RegisterFunction(IFunction function) { _functions.Add(function.Name, function); }

    public IFunction? GetFunction(string name) { return _functions.GetValueOrDefault(name); }

    public T? GetTransformerData<T>(string name)
    {
        return (T?) _transformerData.GetValueOrDefault((typeof(T), name));
    }

    public void SetTransformerData<T>(string name, T? data)
    {
        _transformerData[(typeof(T), name)] = data;
    }

    public void SetVariable(string name, object? value)
    {
        if (_variableStack.Count == 0)
            return;
        _variableStack.Peek().Variables[name] = value;
    }

    public object? GetVariable(string name)
    {
        return _variableStack.Count == 0 ? null : _variableStack.Peek().Variables.GetValueOrDefault(name);
    }

    public bool TryGetVariable(string name, out object? value)
    {
        if (_variableStack.Count == 0)
        {
            value = null;
            return false;
        }

        var dict = _variableStack.Peek().Variables;
        return dict.TryGetValue(name, out value);
    }

    public async ValueTask<object?> EvaluateAsync(
        CultureInfo       cultureInfo,
        string            expression,
        CancellationToken cancellationToken = default
    )
    {
        var firstChar = expression.Length > 0 ? expression[0] : '\0';
        if (expression.Length > 1 && firstChar == '"')
            return HandleStringExpression(expression);

        if (IsFunctionExpression(expression))
            return await HandleFunctionExpressionAsync(cultureInfo, expression, cancellationToken);
        if (IsTrue(expression))
            return true;
        if (IsFalse(expression))
            return false;
        if (IsNumericExpression(firstChar))
            return HandleNumericExpression(expression);

        return GetVariable(expression);
    }

    private static bool IsTrue(string expression) { return expression == "true"; }

    private static bool IsFalse(string expression) { return expression == "false"; }

    private static bool IsFunctionExpression(string expression) { return expression.Contains('('); }

    private static bool IsNumericExpression(char firstChar)
    {
        return firstChar.IsDigit() || firstChar == '-' || firstChar == '+';
    }

    private object? HandleNumericExpression(string expression)
    {
        if (expression.Contains('.') && double.TryParse(expression, out var d))
            return d;
        if (int.TryParse(expression, out var i))
            return i;
        return GetVariable(expression);
    }

    private async ValueTask<object?> HandleFunctionExpressionAsync(
        CultureInfo cultureInfo,
        string expression,
        CancellationToken cancellationToken
    )
    {
        var argsStartIndex = expression.IndexOf('(');
        var functionName = expression[..argsStartIndex];
        var function = GetFunction(functionName);
        if (function is null)
            throw new FunctionNotFoundDuringEvaluationException(functionName);

        var args = new List<object?>();
        var braceCount = 1;
        var currentStart = argsStartIndex + 1;
        var i = argsStartIndex + 1;
        for (; i < expression.Length && braceCount > 0; i++)
        {
            var c = expression[i];
            switch (c)
            {
                case '(':
                    braceCount++;
                    break;
                case ')' when braceCount > 1:
                    braceCount--;
                    break;
                case ',' when braceCount == 1:
                case ')':
                {
                    var nestedExpression = expression[currentStart..i].Trim();
                    if (nestedExpression.Length is 0)
                        break;
                    var value = await EvaluateAsync(cultureInfo, nestedExpression, cancellationToken);
                    args.Add(value);
                    currentStart = i + 1;
                    break;
                }
            }
        }
        if (i != expression.Length)
            throw new FunctionExpressionNotFullyHandledException(functionName, expression, expression[i..]);

        return await function.ExecuteAsync(cultureInfo, args.ToArray(), cancellationToken)
            .ConfigureAwait(false);
    }

    private static string HandleStringExpression(string expression)
    {
        var builder = new StringBuilder(expression.Length - 2);
        var escaped = false;
        for (var i = 1; i < expression.Length - 1; i++)
        {
            var c = expression[i];
            if (escaped)
            {
                switch (c)
                {
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    case 'b':
                        builder.Append('\b');
                        break;
                    case 'f':
                        builder.Append('\f');
                        break;
                    case '\'':
                        builder.Append('\'');
                        break;
                    case '"':
                        builder.Append('"');
                        break;
                    case '\\':
                        builder.Append('\\');
                        break;
                    default: throw new InvalidOperationException($"Unknown escape sequence '\\{c}'.");
                }

                escaped = false;
            }
            else if (c == '\\')
                escaped = true;
            else
                builder.Append(c);
        }

        return builder.ToString();
    }

    public Dictionary<string, object?> PeekScope()
    {
        return _variableStack.Count != 0 ? _variableStack.Peek().Variables : new Dictionary<string, object?>();
    }
}
