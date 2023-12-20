namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Interface that allows a control to be initialized asynchronously after it and all its children have been created.
/// </summary>
[PublicAPI]
public interface IInitializeAsync
{
    /// <summary>
    /// Initializes the control asynchronously.
    /// </summary>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}