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
            .Select(
                (function) =>
                    $"{function.Name}({string.Join(", ", Enumerable.Range(0, function.Arguments).Select((argNo) => $"arg{argNo}"))})");
    }
}