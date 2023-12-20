namespace X39.Solutions.PdfTemplate.Services.ResourceResolver;

/// <summary>
/// This is the default resource resolver that is used if no other is specified.
/// It will never attempt to resolve any resources linking to the file system or the internet,
/// for security reasons. Use a custom <see cref="IResourceResolver"/> if you want to use
/// other sources than base64 encoded data.
/// </summary>
public class DefaultResourceResolver : IResourceResolver
{
    /// <inheritdoc />
    public ValueTask<byte[]> ResolveImageAsync(string source, CancellationToken cancellationToken = default)
    {
        if (source.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            // remove the data:image/...;base64, part
            var index = source.IndexOf(',') + 1;
            if (index == 0)
                throw new NotSupportedException("Invalid uri-style base64 image. Expected data:image/...;base64,...");
            var base64 = source.AsSpan()[index..];
            return new ValueTask<byte[]>(Convert.FromBase64String(base64.ToString()));
        }

        if (source.All((c) => char.IsLetterOrDigit(c) || c == '+' || c == '/' || c == '='))
        {
            return ValueTask.FromResult(Convert.FromBase64String(source));
        }

        throw new NotSupportedException("Only base64 encoded images are supported by the default resource resolver.");
    }
}