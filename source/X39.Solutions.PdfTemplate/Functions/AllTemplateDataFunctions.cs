namespace X39.Solutions.PdfTemplate.Functions;

internal class AllTemplateDataFunctions : IFunction
{
    private readonly ITemplateData _templateData;

    public AllTemplateDataFunctions(ITemplateData templateData)
    {
        _templateData = templateData;
    }

    public string Name => "allFunctions";
    public int Arguments => 0;

    public object Execute(object?[] arguments)
    {
        return _templateData.Functions
            .Select((q) => $"{q.Name}({string.Join(", ", Enumerable.Range(0, q.Arguments).Select((q) => $"arg{q}"))}");
    }
}