namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Thrown when a control does not have a parameter that is specified in the template.
/// </summary>
public sealed class ControlParameterIsNotExistingException : Exception
{
    /// <summary>
    /// The type of the control.
    /// </summary>
    public Type ControlType { get; }
    
    /// <summary>
    /// The name of the parameters.
    /// </summary>
    public string[] Parameters { get; }

    internal ControlParameterIsNotExistingException(Type controlType, string[] parameters)
        : base ($"The parameters {string.Join(", ", parameters)} do not exist in control {controlType.FullName()}.")
    {
        ControlType = controlType;
        Parameters   = parameters;
    }
}