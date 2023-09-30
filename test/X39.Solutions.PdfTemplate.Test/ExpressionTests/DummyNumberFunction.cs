namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

public class DummyNumberFunction : IFunction
{
    private readonly Type[] _inputTypes;
    private readonly int    _returnValue;

    public DummyNumberFunction(string name, int returnValue, Type[] inputTypes)
    {
        Name         = name;
        Arguments    = inputTypes.Length;
        _inputTypes  = inputTypes;
        _returnValue = returnValue;
    }

    public string Name { get; }
    public int Arguments { get; }
    public object Execute(object?[] arguments)
    {
        if (!arguments.Select((q)=>q?.GetType()).SequenceEqual(_inputTypes))
            throw new ArgumentException("Invalid arguments.", nameof(arguments));
        return _returnValue;
    }
}