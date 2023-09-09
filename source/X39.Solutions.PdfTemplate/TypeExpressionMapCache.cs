using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace X39.Solutions.PdfTemplate;

internal sealed class TypeExpressionMapCache<TData> : IDisposable
{
    private readonly Dictionary<string, Func<TData>> _delegates = new();
    
    public TypeExpressionMapCache(Type tCollection, Func<PropertyInfo, bool>? predicate = null)
    {
        predicate ??= (_) => true;
        var properties = tCollection.GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where((q) => q.PropertyType.IsEquivalentTo(typeof(TData)))
            .Where(predicate)
            .ToArray();
        foreach (var property in properties)
        {
            var propertyAccess = Expression.Property(null, property);
            var lambda = Expression.Lambda<Func<TData>>(propertyAccess);
            _delegates[property.Name.ToLowerInvariant()] = lambda.Compile();
        }
    }
    
    public TData Get(string name)
    {
        if (_delegates.TryGetValue(name.ToLowerInvariant(), out var func))
            return func();
        throw new KeyNotFoundException($"The key '{name}' was not found.");
    }
    
    public bool TryGet(string name, out TData data)
    {
        if (_delegates.TryGetValue(name.ToLowerInvariant(), out var func))
        {
            data = func();
            return true;
        }
        data = default!;
        return false;
    }
    
    public bool TryGet(string name, CultureInfo cultureInfo, out TData data)
    {
        if (_delegates.TryGetValue(name.ToLower(cultureInfo), out var func))
        {
            data = func();
            return true;
        }
        data = default!;
        return false;
    }

    public void Dispose()
    {
        _delegates.Clear();
    }
}