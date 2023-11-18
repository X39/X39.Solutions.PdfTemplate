using System.Globalization;

namespace X39.Solutions.PdfTemplate;

internal class EmptyFunction : IFunction
{
    public string Name => string.Empty;
    public int Arguments => 0;
    public bool IsVariadic => false;

    public object? Execute(CultureInfo cultureInfo, object?[] arguments)
    {
        return null;
    }
}