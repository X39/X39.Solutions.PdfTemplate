using System.Globalization;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate;

internal class Template
{
    public Template(
        IReadOnlyCollection<IControl> headerControls,
        IReadOnlyCollection<IControl> bodyControls,
        IReadOnlyCollection<IControl> footerControls)
    {
        HeaderControls = headerControls;
        BodyControls   = bodyControls;
        FooterControls = footerControls;
    }

    public IReadOnlyCollection<IControl> HeaderControls { get; }
    public IReadOnlyCollection<IControl> BodyControls { get; }
    public IReadOnlyCollection<IControl> FooterControls { get; }


    public static Template Create(XmlNodeInformation rootNode, ControlStorage cache, CultureInfo cultureInfo)
    {
        var headerControls = new List<IControl>();
        var bodyControls = new List<IControl>();
        var footerControls = new List<IControl>();
        if (rootNode["header", rootNode.NodeNamespace] is { } headerNode)
            headerControls.AddRange(headerNode.Children.Select((node) => CreateControl(node, cache, cultureInfo)));
        if (rootNode["body", rootNode.NodeNamespace] is { } bodyNode)
            bodyControls.AddRange(bodyNode.Children.Select((node) => CreateControl(node, cache, cultureInfo)));
        if (rootNode["footer", rootNode.NodeNamespace] is { } footerNode)
            footerControls.AddRange(footerNode.Children.Select((node) => CreateControl(node, cache, cultureInfo)));
        return new Template(headerControls.AsReadOnly(), bodyControls.AsReadOnly(), footerControls.AsReadOnly());
    }

    private static IControl CreateControl(XmlNodeInformation node, ControlStorage storage, CultureInfo cultureInfo)
    {
        var control = storage.Create(node.NodeNamespace, node.NodeName, node.Attributes, node.TextContent, cultureInfo);
        if (control is not IContentControl contentControl)
        {
            if (node.Children.Any())
                throw new InvalidOperationException(
                    $"The control {node.NodeNamespace}:{node.NodeName} does not support child controls.");
        }
        else
        {
            foreach (var child in node.Children)
                contentControl.Add(CreateControl(child, storage, cultureInfo));
        }

        return control;
    }
}