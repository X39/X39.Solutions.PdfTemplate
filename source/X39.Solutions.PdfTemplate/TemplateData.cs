using System.Text;

namespace X39.Solutions.PdfTemplate;

internal sealed class TemplateData : ITemplateData
{
    private readonly Stack<Dictionary<string, object?>> _variableStack = new();
    private readonly Dictionary<string, IFunction>      _functions     = new();

    public TemplateData()
    {
        _variableStack.Push(new Dictionary<string, object?>());
    }

    public IDisposable Scope(string scopeName)
    {
        var dict = _variableStack.Any()
            ? new Dictionary<string, object?>(_variableStack.Peek())
            : new Dictionary<string, object?>();
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

    public IDisposable Scope(Dictionary<string, object?> data)
    {
        _variableStack.Push(data);
        return new Disposable(() => _variableStack.Pop());
    }

    public IEnumerable<IFunction> Functions => _functions.Values;

    public IEnumerable<KeyValuePair<string, object?>> Variables => _variableStack
        .SelectMany((d) => d)
        .DistinctBy((q) => q.Key);

    public void RegisterFunction(IFunction function)
    {
        _functions.Add(function.Name, function);
    }

    public IFunction? GetFunction(string name)
    {
        return _functions.GetValueOrDefault(name);
    }

    public void SetVariable(string name, object? value)
    {
        if (_variableStack.Count == 0)
            return;
        _variableStack.Peek()[name] = value;
    }

    public object? GetVariable(string name)
    {
        return _variableStack.Count == 0
            ? null
            : _variableStack.Peek().GetValueOrDefault(name);
    }

    public bool TryGetVariable(string name, out object? value)
    {
        if (_variableStack.Count == 0)
        {
            value = null;
            return false;
        }

        var dict = _variableStack.Peek();
        return dict.TryGetValue(name, out value);
    }

    public object? Evaluate(string expression)
    {
        var firstChar = expression.Length > 0 ? expression[0] : '\0';
        if (expression.Length > 1 && firstChar == '"')
            return HandleStringExpression(expression);

        if (IsFunctionExpression(expression))
            return HandleFunctionExpression(expression);
        if (IsTrue(expression))
            return true;
        if (IsFalse(expression))
            return false;
        if (IsNumericExpression(firstChar))
            return HandleNumericExpression(expression);

        return GetVariable(expression);
    }

    private bool IsTrue(string expression)
    {
        return expression == "true";
    }

    private bool IsFalse(string expression)
    {
        return expression == "false";
    }

    private static bool IsFunctionExpression(string expression)
    {
        return expression.IndexOf('(') != -1;
    }

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

    private object? HandleFunctionExpression(string expression)
    {
        var functionName = expression[..expression.IndexOf('(')];
        var function = GetFunction(functionName);
        var argumentEnd = expression.LastIndexOf(')');
        var argumentsExpression = expression[(functionName.Length + 1)..(argumentEnd)].Trim();
        var arguments = argumentsExpression.IsNullOrEmpty()
            ? Array.Empty<object?>()
            : argumentsExpression
                .Split(',')
                .Select((s) => s.Trim())
                .Select(Evaluate)
                .ToArray();
        if (function is null)
            throw new FunctionNotFoundDuringEvaluationException(functionName);
        return function.Execute(arguments);
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
                    default:
                        throw new InvalidOperationException($"Unknown escape sequence '\\{c}'.");
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
        return _variableStack.Any()
            ? _variableStack.Peek()
            : new Dictionary<string, object?>();
    }
}

internal class EmptyFunction : IFunction
{
    public string Name => string.Empty;
    public int Arguments => 0;

    public object? Execute(object?[] arguments)
    {
        return null;
    }
}