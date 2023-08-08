namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Methods to set up the generator with default controls.
/// </summary>
[PublicAPI]
public static class GeneratorDefaults
{
    /// <summary>
    /// Adds the default controls to the generator.
    /// </summary>
    /// <param name="generator">The generator to add the controls to.</param>
    /// <returns>The <paramref name="generator"/> passed to allow chaining.</returns>
    public static Generator AddDefaultControls(this Generator generator)
    {
        generator.AddControl<Controls.LineControl>();
        return generator;
    }
}