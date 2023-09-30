using System.Linq.Expressions;
using System.Reflection;
using X39.Util.Threading;

namespace X39.Solutions.PdfTemplate.Services.PropertyAccessCache;

internal class PropertyAccessCache : IPropertyAccessCache, IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();

    private delegate object? Getter(object instance);

    private delegate void Setter(object instance, object? value);

    private readonly HashSet<Type>                                        _mapped          = new();
    private readonly Dictionary<(Type, string), (Getter Get, Setter Set)> _expressionCache = new();

    public bool Get(object instance, string propertyName, out object? value)
    {
        var (flag, getterSetter) = _lock.ReadLocked(
            () =>
            {
                var lFlag = _expressionCache.TryGetValue((instance.GetType(), propertyName), out var lGetterSetter);
                return (flag: lFlag, getterSetter: lGetterSetter);
            });
        if (!flag)
        {
            Map(instance.GetType());
        }
        else
        {
            value = getterSetter.Get(instance);
            return true;
        }

        (flag, var retValue) = _lock.ReadLocked(
            () =>
            {
                var lFlag = _expressionCache.TryGetValue((instance.GetType(), propertyName), out var lGetterSetter);
                return (flag: lFlag, lFlag ? lGetterSetter.Get(instance) : default);
            });
        value = retValue;
        return flag;
    }

    public bool Set(object instance, string propertyName, object? value)
    {
        var (flag, getterSetter) = _lock.ReadLocked(
            () =>
            {
                var lFlag = _expressionCache.TryGetValue((instance.GetType(), propertyName), out var lGetterSetter);
                return (flag: lFlag, getterSetter: lGetterSetter);
            });
        if (!flag)
        {
            Map(instance.GetType());
        }
        else
        {
            getterSetter.Set(instance, value);
            return true;
        }

        flag = _lock.ReadLocked(
            () =>
            {
                var lFlag = _expressionCache.TryGetValue((instance.GetType(), propertyName), out var lGetterSetter);
                if (lFlag)
                    lGetterSetter.Set(instance, value);
                return lFlag;
            });
        return flag;
    }

    public void Map(Type type)
    {
        if (!_lock.ReadLocked(() => _mapped.Contains(type)))
            return;
        _lock.WriteLocked(
            () =>
            {
                if (_mapped.Contains(type))
                    return;
                _mapped.Add(type);
                foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    var getter = propertyInfo.GetGetMethod();
                    var setter = propertyInfo.GetSetMethod();
                    if (getter is null && setter is null)
                        continue;
                    var instanceParameter = Expression.Parameter(typeof(object), "instance");
                    var valueParameter = Expression.Parameter(typeof(object), "value");
                    var instanceCast = Expression.Convert(instanceParameter, type);
                    var valueCast = Expression.Convert(valueParameter, propertyInfo.PropertyType);
                    Expression getterExpression = getter is null
                        ? Expression.Default(propertyInfo.PropertyType)
                        : Expression.Call(instanceCast, getter);
                    Expression setterExpression = setter is null
                        ? Expression.Empty()
                        : Expression.Call(instanceCast, setter, valueCast);
                    var getterLambda = Expression.Lambda<Getter>(getterExpression, instanceParameter);
                    var setterLambda = Expression.Lambda<Setter>(setterExpression, instanceParameter, valueParameter);
                    _expressionCache.Add((type, propertyInfo.Name), (getterLambda.Compile(), setterLambda.Compile()));
                }
            });
    }

    public void Clear()
    {
        _lock.WriteLocked(
            () =>
            {
                _mapped.Clear();
                _expressionCache.Clear();
            });
    }

    public void Dispose()
    {
        _lock.Dispose();
        _mapped.Clear();
        _expressionCache.Clear();
    }
}