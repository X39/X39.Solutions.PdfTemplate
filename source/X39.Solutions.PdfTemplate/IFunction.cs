using System.Globalization;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// A function that can be used in a template.
/// </summary>
public interface IFunction
{
    /// <summary>
    /// The name of the function.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// The number of arguments the function takes.
    /// </summary>
    int Arguments { get; }
    
    /// <summary>
    /// If set to <c>true</c>, the function may be called with a variable number of arguments but
    /// at least <see cref="Arguments"/> arguments.
    /// </summary>
    bool IsVariadic { get; }

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <remarks>
    /// The number of arguments passed to this function is guaranteed to be equal to <see cref="Arguments"/>.
    /// </remarks>
    /// <param name="cultureInfo">The culture info to use for the function.</param>
    /// <param name="arguments">The arguments to pass to the function.</param>
    /// <returns>The result of the function.</returns>
    object? Execute(CultureInfo cultureInfo, object?[] arguments);
}