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
    /// <param name="self">The generator to add the controls to.</param>
    /// <returns>The <paramref name="self"/> passed to allow chaining.</returns>
    public static Generator AddDefaults(this Generator self)
    {
        AddDefaultControls(self);
        self.AddDefaultTransformers();
        return self;
    }

    internal static T AddDefaultControls<T>(this T self) where T : IAddControls
    {
        self.AddControl<Controls.LineControl>();
        self.AddControl<Controls.TextControl>();
        self.AddControl<Controls.TableControl>();
        self.AddControl<Controls.TableCellControl>();
        self.AddControl<Controls.TableHeaderControl>();
        self.AddControl<Controls.TableRowControl>();
        self.AddControl<Controls.BorderControl>();
        return self;
    }

    internal static T AddDefaultTransformers<T>(this T self) where T : IAddTransformers
    {
        self.AddTransformer(new ForTransformer());
        self.AddTransformer(new IfTransformer());
        self.AddTransformer(new ForEachTransformer());
        self.AddTransformer(new AlternateTransformer());
        return self;
    }
}