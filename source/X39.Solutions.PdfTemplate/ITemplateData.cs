namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Allows access to pdf template data.
/// </summary>
public interface ITemplateData
{
    /// <summary>
    /// Creates a new data scope.
    /// </summary>
    /// <param name="scopeName">The name of the scope.</param>
    /// <returns>A disposable that will dispose the scope when disposed.</returns>
    IDisposable Scope(string scopeName);

    /// <summary>
    /// All functions currently available.
    /// </summary>
    IEnumerable<IFunction> Functions { get; }

    /// <summary>
    /// All variables currently available.
    /// </summary>
    /// <remarks>
    /// This is affected by <see cref="Scope"/>.
    /// </remarks>
    IEnumerable<KeyValuePair<string, object?>> Variables { get; }

    /// <summary>
    /// Registers a function.
    /// </summary>
    /// <param name="function">The function to register.</param>
    void RegisterFunction(IFunction function);

    /// <summary>
    /// Sets a variable.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    void SetVariable(string name, object? value);

    /// <summary>
    /// Gets a variable.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <returns>The value of the variable.</returns>
    object? GetVariable(string name);
    
    /// <summary>
    /// Tries to get a variable.
    /// </summary>
    /// <param name="name">The name of the variable.</param>
    /// <param name="value">The value of the variable.</param>
    /// <returns><see langword="true"/> if the variable was found, otherwise <see langword="false"/>.</returns>
    bool TryGetVariable(string name, out object? value);

    /// <summary>
    /// The expression to evaluate.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="Evaluate"/>, this method may execute <see cref="IFunction"/>'s, access <see cref="Variables"/>
    /// or parse <paramref name="expression"/> to the expected type (eg. <see cref="int"/>).
    /// </remarks>
    /// <param name="expression">The expression to evaluate.</param>
    /// <returns>The result of the expression.</returns>
    object? Evaluate(string expression);

    
    /// <summary>
    /// Returns a function by name.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <returns>The function or null if not found.</returns>
    IFunction? GetFunction(string name);
}