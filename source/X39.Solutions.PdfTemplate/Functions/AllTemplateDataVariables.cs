using X39.Solutions.PdfTemplate.Abstraction;

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
    public bool IsVariadic => false;

    public ValueTask<object?> ExecuteAsync(
        CultureInfo cultureInfo,
        object?[] arguments,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<object?>(_templateData.Variables.Select((q) => $"{q.Key}: {q.Value}"));
    }
}