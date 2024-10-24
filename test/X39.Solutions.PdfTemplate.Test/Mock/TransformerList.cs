using X39.Solutions.PdfTemplate.Abstraction;

namespace X39.Solutions.PdfTemplate.Test.Mock;

public sealed class TransformerList : List<ITransformer>, IAddTransformers
{
    public void AddTransformer(ITransformer transformer)
    {
        Add(transformer);
    }
}