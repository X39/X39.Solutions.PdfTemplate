using X39.Solutions.PdfTemplate.Transformers;

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
        generator.AddControl<Controls.TextControl>();
        generator.AddControl<Controls.TableControl>();
        generator.AddControl<Controls.TableCellControl>();
        generator.AddControl<Controls.TableHeaderControl>();
        generator.AddControl<Controls.TableRowControl>();
        
        generator.AddTransformer(new ForTransformer());
        return generator;
    }
}