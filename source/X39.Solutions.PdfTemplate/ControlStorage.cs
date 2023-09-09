using System.Data;
using System.Globalization;

namespace X39.Solutions.PdfTemplate;

internal class ControlStorage
{
    private readonly ControlExpressionCache                             _controlExpressionCache;
    private readonly Dictionary<(string @namespace, string name), Type> _controls = new();

    public ControlStorage(ControlExpressionCache controlExpressionCache)
    {
        _controlExpressionCache = controlExpressionCache;
    }

    public void Clear()
    {
        _controls.Clear();
    }

    public void Add(string @namespace, string name, Type type)
    {
        var upperCasedNamespace = @namespace.ToUpper(CultureInfo.InvariantCulture);
        var upperCasedName      = name.ToUpper(CultureInfo.InvariantCulture);
        var key = (upperCasedNamespace, upperCasedName);
        if (_controls.TryGetValue(key, out var existingType))
            throw new DuplicateNameException(
                $"The control {@namespace}:{name} already exists. Existing type: {existingType.FullName()}");
        _controls.Add(key, type);
    }

    public IControl Create(
        string @namespace,
        string name,
        IReadOnlyDictionary<string, string> parameterDictionary,
        string? content,
        CultureInfo cultureInfo)
    {
        var upperCasedNamespace = @namespace.ToUpper(CultureInfo.InvariantCulture);
        var upperCasedName      = name.ToUpper(CultureInfo.InvariantCulture);
        var key = (upperCasedNamespace, upperCasedName);
        if (!_controls.TryGetValue(key, out var type))
            throw new InvalidOperationException($"The control {@namespace}:{name} does not exist.");
        var control = _controlExpressionCache.CreateControl(type, parameterDictionary, content, cultureInfo);
        return control;
    }
}