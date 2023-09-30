namespace X39.Solutions.PdfTemplate.Functions;

internal class AllTemplateDataVariables : IFunction
{
    private readonly ITemplateData _templateData;

    public AllTemplateDataVariables(ITemplateData templateData)
    {
        _templateData = templateData;
    }

    public string Name => "allVariables";
    public int Arguments => 0;

    public object Execute(object?[] arguments)
    {
        return _templateData.Variables.Select((q) => $"{q.Key}: {q.Value}");
    }
}