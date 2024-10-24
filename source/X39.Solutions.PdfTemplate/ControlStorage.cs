using System.Data;
using System.Reflection;
using X39.Solutions.PdfTemplate.Abstraction;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Services;

namespace X39.Solutions.PdfTemplate;

internal class ControlStorage : IAddControls
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

    public void AddControl<
        [MeansImplicitUse(
            ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign)]
        TControl>()
        where TControl : IControl
    {
        var type = typeof(TControl);

        if (type.IsGenericType)
            throw new InvalidOperationException(
                $"The type {type.FullName} is a generic type and cannot be used as a control.");
        var attribute = typeof(TControl).GetCustomAttribute<ControlAttribute>();
        if (attribute is null)
            throw new InvalidOperationException(
                $"The type {typeof(TControl).FullName} does not have a {nameof(ControlAttribute)}.");
        var name = attribute.Name;
        if (name.IsNullOrEmpty())
        {
            const string controlSuffix = "Control";
            name = typeof(TControl).Name();

            if (name.EndsWith(controlSuffix, StringComparison.Ordinal))
                name = name[..^controlSuffix.Length];
        }

        Add(
            attribute.Namespace,
            name,
            typeof(TControl));
    }
}