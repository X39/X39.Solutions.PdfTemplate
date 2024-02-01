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
    /// <param name="cancellationToken">A cancellation token to cancel the execution.</param>
    /// <returns>
    ///     A <see cref="ValueTask"/> that will complete when the function has finished executing,
    ///     returning the result of the function.
    /// </returns>
    ValueTask<object?> ExecuteAsync(
        CultureInfo cultureInfo,
        object?[] arguments,
        CancellationToken cancellationToken = default);
}