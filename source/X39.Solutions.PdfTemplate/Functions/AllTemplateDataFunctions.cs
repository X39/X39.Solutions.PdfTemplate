using X39.Util.Collections;

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
    public bool IsVariadic => false;

    public ValueTask<object?> ExecuteAsync(
        CultureInfo cultureInfo,
        object?[] arguments,
        CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<object?>(_templateData.Functions
            .Select(
                (function) =>
                    $"{function.Name}({string.Join(", ", Enumerable.Range(0, function.Arguments).Select((argNo) => $"arg{argNo}").Append(IsVariadic ? "..." : null).NotNull())})"));
    }
}