using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using X39.Solutions.PdfTemplate.Attributes;
using X39.Solutions.PdfTemplate.Data;
using X39.Util.Threading;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Caches the creation of controls.
/// </summary>
public sealed class ControlExpressionCache : IDisposable
{
    private readonly IServiceProvider     _serviceProvider;
    private readonly ReaderWriterLockSlim _setParameterDelegatesLock = new();
    private readonly ReaderWriterLockSlim _createDelegatesLock       = new();

    private readonly
        Dictionary<Type, (string parameter, Action<IControl, string, CultureInfo> setter)[]>
        _setParameterDelegates = new();

    private readonly Dictionary<Type, Func<IServiceProvider, IControl>> _createDelegates = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ControlExpressionCache"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public ControlExpressionCache(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Creates a control of the specified type.
    /// </summary>
    /// <param name="type">The type of the control to create.</param>
    /// <param name="parameterDictionary">The attributes to set on the control.</param>
    /// <param name="cultureInfo">The culture to use for parameter conversion.</param>
    /// <returns>The created control.</returns>
    public IControl CreateControl(
        Type type,
        IReadOnlyDictionary<string, string> parameterDictionary,
        CultureInfo cultureInfo)
    {
        var control = CreateControlInstance(type);
        SetParametersOfControl(type, control, parameterDictionary, cultureInfo);
        return control;
    }

    private IControl CreateControlInstance(Type type)
    {
        return _createDelegatesLock.UpgradeableReadLocked(
            () =>
            {
                if (_createDelegates.TryGetValue(type, out var @delegate))
                    return @delegate(_serviceProvider);
                return _createDelegatesLock.WriteLocked(
                    () =>
                    {
                        if (_createDelegates.TryGetValue(type, out @delegate))
                            return (IControl) @delegate.DynamicInvoke()!;
                        var constructors = type.GetConstructors(
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                        var constructor = constructors.FirstOrDefault(
                                              (q) => q.GetCustomAttribute<ControlConstructorAttribute>() is not null)
                                          ?? constructors.MaxBy((q) => q.GetParameters().Length);
                        var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider));
                        var newExpression = constructor is null || constructor.GetParameters().Length is 0
                            ? Expression.New(type)
                            : Expression.New(
                                constructor,
                                constructor.GetParameters()
                                    .Select(
                                        (q) => Expression.Call(
                                            serviceProviderParameter,
                                            typeof(IServiceProvider).GetMethod(
                                                nameof(IServiceProvider.GetService))!,
                                            Expression.Constant(q.ParameterType)))
                                    .Cast<Expression>()
                                    .ToArray());
                        var expression = Expression.Lambda<Func<IServiceProvider, IControl>>(
                            newExpression,
                            serviceProviderParameter);
                        @delegate = expression.Compile();
                        _createDelegates.Add(type, @delegate);
                        return @delegate(_serviceProvider);
                    });
            });
    }

    private void SetParametersOfControl(
        Type controlType,
        IControl control,
        IReadOnlyDictionary<string, string> parameterDictionary,
        CultureInfo cultureInfo)
    {
        var array = _setParameterDelegatesLock.UpgradeableReadLocked(
            () =>
            {
                if (_setParameterDelegates.TryGetValue(controlType, out var setterTupleArray))
                    return setterTupleArray;
                return _setParameterDelegatesLock.WriteLocked(
                    () =>
                    {
                        if (_setParameterDelegates.TryGetValue(controlType, out var setterTupleArray2))
                            return setterTupleArray2;


                        var controlParameter = Expression.Parameter(typeof(IControl));
                        var valueParameter = Expression.Parameter(typeof(string));
                        var cultureInfoParameter = Expression.Parameter(typeof(CultureInfo));
                        var tuples = controlType
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            .Select(
                                (propertyInfo) => (propertyInfo,
                                    attribute: propertyInfo.GetCustomAttribute<ParameterAttribute>()))
                            .Where((q) => q.Item2 is not null)
                            .Select((q) => (q.propertyInfo, q.attribute!));

                        var setterTuples = new List<(string parameter, Action<IControl, string, CultureInfo> setter)>();
                        foreach (var (propertyInfo, parameterAttribute) in tuples)
                        {
                            var propertySetter = propertyInfo.GetSetMethod(true)
                                                 ?? throw new InvalidOperationException(
                                                     $"The property {propertyInfo} of {controlType.FullName()} has no setter.");
                            var converter = parameterAttribute.Converter
                                            ?? propertyInfo.PropertyType
                                                .GetCustomAttribute<ParameterConverterAttributeBase>()
                                                ?.Converter;
                            if (converter is not null)
                            {
                                var interfaceType = converter.GetInterfaces()
                                                        .FirstOrDefault(
                                                            (q) => q.IsGenericType(typeof(IParameterConverter<>)))
                                                    ?? throw new InvalidOperationException(
                                                        $"The converter {converter} used on {controlType.FullName()} does not implement {typeof(IParameterConverter<>).FullName()}.");
                                var converterConstructors = converter.GetConstructors(
                                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                    BindingFlags.Static);
                                var converterConstructor = converterConstructors.FirstOrDefault(
                                                               (q) =>
                                                                   q.GetCustomAttribute<
                                                                           ParameterConverterConstructorAttribute>() is
                                                                       not
                                                                       null)
                                                           ?? converterConstructors.MaxBy(
                                                               (q) => q.GetParameters().Length);
                                var serviceProviderParameter = Expression.Parameter(typeof(IServiceProvider));
                                var newExpression = converterConstructor is null ||
                                                    converterConstructor.GetParameters().Length is 0
                                    ? Expression.New(converter)
                                    : Expression.New(
                                        converterConstructor,
                                        converterConstructor.GetParameters()
                                            .Select(
                                                (q) => Expression.Call(
                                                    serviceProviderParameter,
                                                    typeof(IServiceProvider).GetMethod(
                                                        nameof(IServiceProvider.GetService))!,
                                                    Expression.Constant(q.ParameterType)))
                                            .Cast<Expression>()
                                            .ToArray());
                                var castExpression = Expression.Convert(newExpression, interfaceType);
                                var callConverterExpression = Expression.Call(
                                    castExpression,
                                    interfaceType.GetMethod(nameof(IParameterConverter<int>.Convert))!,
                                    valueParameter,
                                    Expression.Constant(parameterAttribute.Format),
                                    cultureInfoParameter);
                                var castToControlTypeExpression = Expression.Convert(
                                    controlParameter,
                                    controlType);
                                var setPropertyExpression = Expression.Call(
                                    castToControlTypeExpression,
                                    propertySetter,
                                    callConverterExpression);
                                var expression = Expression.Lambda<Action<IControl, string, CultureInfo>>(
                                    setPropertyExpression,
                                    controlParameter,
                                    valueParameter,
                                    cultureInfoParameter);

                                setterTuples.Add((Validators.ParameterName.GetName(parameterAttribute, propertyInfo), expression.Compile()));
                            }
                            else if (propertyInfo.PropertyType.IsEquivalentTo(typeof(string)))
                            {
                                var castToControlTypeExpression = Expression.Convert(
                                    controlParameter,
                                    controlType);
                                var setPropertyExpression = Expression.Call(
                                    castToControlTypeExpression,
                                    propertySetter,
                                    valueParameter);
                                var expression = Expression.Lambda<Action<IControl, string, CultureInfo>>(
                                    setPropertyExpression,
                                    controlParameter,
                                    valueParameter,
                                    cultureInfoParameter);

                                setterTuples.Add((Validators.ParameterName.GetName(parameterAttribute, propertyInfo), expression.Compile()));
                            }
                            else
                            {
                                var parsable = propertyInfo.PropertyType.GetInterfaces()
                                    .FirstOrDefault((q) => q.IsGenericType(typeof(IParsable<>)));
                                if (parsable is not null && propertyInfo.PropertyType.IsAssignableTo(parsable))
                                {
                                    var parseMethod = propertyInfo.PropertyType.GetMethod(
                                        nameof(IParsable<int>.Parse),
                                        new[] {typeof(string), typeof(CultureInfo)});
                                    var callConverterExpression = Expression.Call(
                                        null,
                                        parseMethod!,
                                        valueParameter,
                                        cultureInfoParameter);
                                    var castToControlTypeExpression = Expression.Convert(
                                        controlParameter,
                                        controlType);
                                    var setPropertyExpression = Expression.Call(
                                        castToControlTypeExpression,
                                        propertySetter,
                                        callConverterExpression);
                                    var expression = Expression.Lambda<Action<IControl, string, CultureInfo>>(
                                        setPropertyExpression,
                                        controlParameter,
                                        valueParameter,
                                        cultureInfoParameter);
                                    setterTuples.Add(
                                        (Validators.ParameterName.GetName(parameterAttribute, propertyInfo), expression.Compile()));
                                }
                                else if (TypeDescriptor.GetConverter(propertyInfo.PropertyType) is { } typeConverter &&
                                         typeConverter.CanConvertFrom(typeof(string)))
                                {
                                    var callConverterExpression = Expression.Call(
                                        Expression.Constant(typeConverter),
                                        typeConverter.GetType()
                                            .GetMethods()
                                            .First(
                                                (q) => q.Name == nameof(TypeConverter.ConvertFromString) &&
                                                       q.GetParameters().Length == 3),
                                        Expression.Constant(null, typeof(ITypeDescriptorContext)),
                                        cultureInfoParameter,
                                        valueParameter);
                                    var castExpression = Expression.Convert(
                                        callConverterExpression,
                                        propertyInfo.PropertyType);
                                    var castToControlTypeExpression = Expression.Convert(
                                        controlParameter,
                                        controlType);
                                    var setPropertyExpression = Expression.Call(
                                        castToControlTypeExpression,
                                        propertySetter,
                                        castExpression);
                                    var expression = Expression.Lambda<Action<IControl, string, CultureInfo>>(
                                        setPropertyExpression,
                                        controlParameter,
                                        valueParameter,
                                        cultureInfoParameter);

                                    setterTuples.Add(
                                        (Validators.ParameterName.GetName(parameterAttribute, propertyInfo), expression.Compile()));
                                }
                                else
                                {
                                    throw new InvalidOperationException(
                                        $"The property {propertyInfo} of {controlType.FullName()} has no converter, is not a string and has no {typeof(TypeConverter).FullName()} that can convert from {typeof(string).FullName()}.");
                                }
                            }
                        }

                        return _setParameterDelegates[controlType] = setterTuples.ToArray();
                    });
            });
        foreach (var (parameter, setter) in array)
        {
            if (parameterDictionary.TryGetValue(parameter, out var value))
                setter(control, value, cultureInfo);
        }
    }

    public void Dispose()
    {
        _setParameterDelegatesLock.Dispose();
        _createDelegatesLock.Dispose();
    }
}