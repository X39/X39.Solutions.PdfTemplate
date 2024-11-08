﻿namespace X39.Solutions.PdfTemplate.Abstraction;

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
    /// Unlike <see cref="EvaluateAsync"/>, this method may execute <see cref="IFunction"/>'s, access <see cref="Variables"/>
    /// or parse <paramref name="expression"/> to the expected type (eg. <see cref="int"/>).
    /// </remarks>
    /// <param name="cultureInfo"></param>
    /// <param name="expression">The expression to evaluate.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the evaluation.</param>
    /// <returns>
    ///     A <see cref="ValueTask"/> that will complete when the expression was evaluated,
    ///     returning the result of the expression.
    /// </returns>
    ValueTask<object?> EvaluateAsync(
        CultureInfo cultureInfo,
        string expression,
        CancellationToken cancellationToken = default);


    /// <summary>
    /// Returns a function by name.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <returns>The function or null if not found.</returns>
    IFunction? GetFunction(string name);

    /// <summary>
    /// Gets the transformer data for a specified name.
    /// </summary>
    /// <typeparam name="T">The type of transformer data.</typeparam>
    /// <param name="name">The name of the data to receive.</param>
    /// <returns>The transformer data or <see langword="default"/>(<typeparamref name="T"/>) if not found.</returns>
    /// <remarks>
    /// This method allows transformers to share data, making some advanced scenarios possible.
    /// The data is alive for the lifetime of the <see cref="ITemplateData"/> instance.
    /// The data is unique at type and name level, not only at name level, allowing name collisions,
    /// given that the types are different.
    /// </remarks>
    /// <example>
    /// <code>
    /// var data = GetTransformerData&lt;string&gt;("MyData");
    /// </code>
    /// </example>
    T? GetTransformerData<T>(string name);

    /// <summary>
    /// Sets the transformer data for a given name.
    /// </summary>
    /// <typeparam name="T">The type of the transformer data.</typeparam>
    /// <param name="name">The name of the transformer data.</param>
    /// <param name="data">The transformer data.</param>
    /// <remarks>
    /// This method allows transformers to share data, making some advanced scenarios possible.
    /// The data is alive for the lifetime of the <see cref="ITemplateData"/> instance.
    /// The data is unique at type and name level, not only at name level, allowing name collisions,
    /// given that the types are different.
    /// </remarks>
    /// <example>
    /// <code>
    /// SetTransformerData("MyData", "Hello World");
    /// </code>
    /// </example>
    void SetTransformerData<T>(string name, T? data);
}