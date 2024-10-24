namespace X39.Solutions.PdfTemplate.Exceptions;

/// <summary>
/// Thrown when a function is not found during evaluation.
/// </summary>
public class FunctionNotFoundDuringEvaluationException : EvaluationException
{
    internal FunctionNotFoundDuringEvaluationException(string functionName)
        : base ($"Function '{functionName}' not found.")
    {
        FunctionName = functionName;
    }

    /// <summary>
    /// The name of the function that was not found.
    /// </summary>
    public string FunctionName { get; set; }
}