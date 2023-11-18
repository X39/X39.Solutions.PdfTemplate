namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

public class DummyValueCollectionFunction : IFunction
{
    private readonly Type[]       _inputTypes;
    private readonly List<object> _returnValues;

    public DummyValueCollectionFunction(string name, List<object> returnValues, Type[] inputTypes)
    {
        Name          = name;
        Arguments     = inputTypes.Length;
        _inputTypes   = inputTypes;
        _returnValues = returnValues;
    }

    public string Name { get; }
    public int Arguments { get; }
    public bool IsVariadic => false;

    public object Execute(object?[] arguments)
    {
        if (!arguments.Select((q) => q?.GetType()).SequenceEqual(_inputTypes))
            throw new ArgumentException("Invalid arguments.", nameof(arguments));
        return _returnValues;
    }
}