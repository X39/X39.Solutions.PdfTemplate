using System.Xml;
using X39.Solutions.PdfTemplate.Xml;
using XmlNode = X39.Solutions.PdfTemplate.Xml.XmlNode;

namespace X39.Solutions.PdfTemplate;

/// <summary>
/// Interface providing the ability to transform template elements into a different format,
/// allowing to eg. repeat elements.
/// </summary>
public interface ITransformer
{
    /// <summary>
    /// The name of the transformer.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Transforms the given template element into a different format.
    /// </summary>
    /// <param name="templateData">The data storage available to the template.</param>
    /// <param name="remainingLine">The remaining line of the template.</param>
    /// <param name="nodes">The nodes contained in the template.</param>
    /// <returns>The transformed nodes.</returns>
    IEnumerable<XmlNode> Transform(
        ITemplateData templateData,
        string remainingLine,
        IReadOnlyCollection<XmlNode> nodes);
}