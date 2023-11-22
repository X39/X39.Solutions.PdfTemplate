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
    /// The parameters that are not existing on the control.
    /// </summary>
    public string[] MissingParameters { get; }

    /// <summary>
    /// The available parameters.
    /// </summary>
    public string[] AvailableParameters { get; }

    internal ControlParameterIsNotExistingException(
        Type controlType,
        string[] missingParameters,
        string[] availableParameters)
        : base ($"The parameters {string.Join(", ", missingParameters)} do not exist in control {controlType.FullName()}.{Environment.NewLine}Available parameters: {string.Join($", ", availableParameters)}")
    {
        ControlType              = controlType;
        MissingParameters        = missingParameters;
        AvailableParameters = availableParameters;
    }
}