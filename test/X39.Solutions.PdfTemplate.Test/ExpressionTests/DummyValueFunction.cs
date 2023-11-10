﻿namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

public class DummyValueFunction : IFunction
{
    private readonly Type[]                   _inputTypes;
    private readonly Func<object?[], object?> _returnFunction;

    public DummyValueFunction(string name, object? returnValue, Type[] inputTypes)
    {
        Name            = name;
        Arguments       = inputTypes.Length;
        _inputTypes     = inputTypes;
        _returnFunction = (_) => returnValue;
    }
    public DummyValueFunction(string name, Func<object?[], object?> returnFunction, Type[] inputTypes)
    {
        Name         = name;
        Arguments    = inputTypes.Length;
        _inputTypes  = inputTypes;
        _returnFunction = returnFunction;
    }

    public string Name { get; }
    public int Arguments { get; }

    public object? Execute(object?[] arguments)
    {
        if (!arguments.Select((q) => q?.GetType()).SequenceEqual(_inputTypes))
            throw new ArgumentException("Invalid arguments.", nameof(arguments));
        return _returnFunction(arguments);
    }
}