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
    /// Executes the function.
    /// </summary>
    /// <remarks>
    /// The number of arguments passed to this function is guaranteed to be equal to <see cref="Arguments"/>.
    /// </remarks>
    /// <param name="arguments">The arguments to pass to the function.</param>
    /// <returns>The result of the function.</returns>
    object? Execute(object?[] arguments);
}