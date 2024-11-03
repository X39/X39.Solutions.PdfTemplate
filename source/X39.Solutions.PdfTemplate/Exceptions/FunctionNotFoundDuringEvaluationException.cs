namespace X39.Solutions.PdfTemplate.Exceptions;

/// <summary>
/// Thrown when a function is not found during evaluation.
/// </summary>
public class FunctionNotFoundDuringEvaluationException : EvaluationException
{
    internal FunctionNotFoundDuringEvaluationException(string functionName)
        : base($"Function '{functionName}' not found.")
    {
        FunctionName = functionName;
    }

    /// <summary>
    /// The name of the function that was not found.
    /// </summary>
    public string FunctionName { get; set; }
}

/// <summary>
/// Thrown when a function expression has additional data at the end.
/// </summary>
public class FunctionExpressionNotFullyHandledException : EvaluationException
{
    internal FunctionExpressionNotFullyHandledException(string functionName, string expression, string unhandledData)
        : base(
            $"Expression '{expression}' was properly matched to function {functionName} but has an invalid format, leaving '{unhandledData}' unhandled."
        )
    {
        FunctionName  = functionName;
        Expression    = expression;
        UnhandledData = unhandledData;
    }

    /// <summary>
    /// The name of the function that was found.
    /// </summary>
    public string FunctionName { get; set; }

    /// <summary>
    /// The full function expression, including the function name and the args.
    /// </summary>
    public string Expression { get; }

    /// <summary>
    /// The data passed into the function that could not be handled.
    /// </summary>
    public string UnhandledData { get; }
}