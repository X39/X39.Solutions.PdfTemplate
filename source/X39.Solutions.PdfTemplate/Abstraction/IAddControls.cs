namespace X39.Solutions.PdfTemplate.Abstraction;

internal interface IAddControls
{
    void AddControl<
        [MeansImplicitUse(
            ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature | ImplicitUseKindFlags.Assign)]
        TControl>()
        where TControl : IControl;
}