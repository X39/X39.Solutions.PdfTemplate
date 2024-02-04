using System.Globalization;
using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Transformers;
using X39.Solutions.PdfTemplate.Xml;
using X39.Solutions.PdfTemplate.Xml.Exceptions;

namespace X39.Solutions.PdfTemplate.Test.Xml;

public class XmlTemplateReaderTests
{
    [Fact]
    public async Task EffectiveStyle()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <effectiveStyleTest xmlns="{ns}">
                                     <nested>
                                         <nested.style>
                                             <line margin="2px" padding="2px"/>
                                         </nested.style>
                                         <line margin="3px"/>
                                     </nested>
                                     <effectiveStyleTest.style>
                                         <line margin="1px" padding="1px"/>
                                     </effectiveStyleTest.style>
                                     <line margin="4px"/>
                                 </effectiveStyleTest>
                                 """;
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, new TemplateData(), ArraySegment<ITransformer>.Empty);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var node = await templateReader.ReadAsync(xmlReader);

        // Assert all nodes are present
        Assert.Equal("effectiveStyleTest", node.NodeName);
        Assert.Equal(ns, node.NodeNamespace);
        Assert.Equal(2, node.Children.Count);
        Assert.Equal("nested", node.Children.ElementAt(0).NodeName);
        Assert.Equal(ns, node.Children.ElementAt(0).NodeNamespace);
        Assert.Single(node.Children.ElementAt(0).Children);
        Assert.Equal("line", node.Children.ElementAt(0).Children.ElementAt(0).NodeName);
        Assert.Equal(ns, node.Children.ElementAt(0).Children.ElementAt(0).NodeNamespace);
        Assert.Equal("line", node.Children.ElementAt(1).NodeName);
        Assert.Equal(ns, node.Children.ElementAt(1).NodeNamespace);

        // Assert all effective styles are as expected
        Assert.Equal("3px", node["nested", ns]!["line", ns]!.Attributes["MARGIN"]);
        Assert.Equal("2px", node["nested", ns]!["line", ns]!.Attributes["PADDING"]);
        Assert.Equal("4px", node["line", ns]!.Attributes["MARGIN"]);
        Assert.Equal("1px", node["line", ns]!.Attributes["PADDING"]);
    }

    [Fact]
    public async Task LowestLevelStyleAppliedLast()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <styleTest xmlns="{ns}">
                                     <styleTest.style>
                                         <line margin="1px" padding="1px"/>
                                     </styleTest.style>
                                     <nested>
                                         <nested.style>
                                             <line margin="2px" padding="2px"/>
                                         </nested.style>
                                         <line margin="0px"/>
                                     </nested>
                                    <line margin="0px"/>
                                 </styleTest>
                                 """;
        var templateReader = new XmlTemplateReader(
            CultureInfo.InvariantCulture,
            new TemplateData(),
            ArraySegment<ITransformer>.Empty
        );
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var       node      = await templateReader.ReadAsync(xmlReader);

        // Assert effective styles are as expected
        Assert.Equal("0px", node["nested", ns]!["line", ns]!.Attributes["MARGIN"]);
        Assert.Equal("2px", node["nested", ns]!["line", ns]!.Attributes["PADDING"]);
        Assert.Equal("0px", node["line", ns]!.Attributes["MARGIN"]);
        Assert.Equal("1px", node["line", ns]!.Attributes["PADDING"]);
    }

    [Fact]
    public async Task NoDotInName()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <noDotInNameTest xmlns="{ns}">
                                     <invalid.element margin="4px"/>
                                 </noDotInNameTest>
                                 """;
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, new TemplateData(), ArraySegment<ITransformer>.Empty);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        await Assert.ThrowsAsync<XmlNodeNameException>(() => templateReader.ReadAsync(xmlReader));
    }

    [Fact]
    public async Task StyleMustBeEmptyTag()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <styleMustBeEmptyTagTest xmlns="{ns}">
                                    <styleMustBeEmptyTagTest.style>
                                        <nonEmptyElement>
                                            <someElement/>
                                        </nonEmptyElement>
                                    </styleMustBeEmptyTagTest.style>
                                 </styleMustBeEmptyTagTest>
                                 """;
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, new TemplateData(), ArraySegment<ITransformer>.Empty);
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        await Assert.ThrowsAsync<XmlStyleInformationCannotNestException>(() => templateReader.ReadAsync(xmlReader));
    }

    [Fact]
    public async Task ForLoop()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                    @for i from 0 to 10 {
                                        <text>@i</text>
                                    }
                                 </styleMustBeEmptyTagTest>
                                 """;
        var templateReader = new XmlTemplateReader(CultureInfo.InvariantCulture, new TemplateData(), new []{new ForTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = await templateReader.ReadAsync(xmlReader);
        Assert.Equal(10, nodeInformation.Children.Count);
    }
}