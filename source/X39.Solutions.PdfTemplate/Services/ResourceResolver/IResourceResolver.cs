namespace X39.Solutions.PdfTemplate.Services.ResourceResolver;

/// <summary>
/// A service that resolves resources for controls.
/// </summary>
public interface IResourceResolver
{
    /// <summary>
    ///     Resolves an image from the given source.
    /// </summary>
    /// <param name="source">
    ///     The source of the image.
    /// </param>
    /// <param name="cancellationToken">
    ///     The cancellation token to use.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask"/> that resolves to the image data.
    /// </returns>
    ValueTask<byte[]> ResolveImageAsync(string source, CancellationToken cancellationToken = default);
}