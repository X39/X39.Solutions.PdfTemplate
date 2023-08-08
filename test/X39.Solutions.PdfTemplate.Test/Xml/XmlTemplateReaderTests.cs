using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Test.Xml;

public class XmlTemplateReaderTests
{
    [Fact]
    public void EffectiveStyle()
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
        var templateReader = new XmlTemplateReader();
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var node = templateReader.Read(xmlReader);

        // Assert all nodes are present
        Assert.Equal("effectiveStyleTest", node.NodeName);
        Assert.Equal(ns, node.NodeNamespace);
        Assert.Equal(2, node.Children.Count);
        Assert.Equal("nested", node.Children.ElementAt(0).NodeName);
        Assert.Equal(ns, node.Children.ElementAt(0).NodeNamespace);
        Assert.Equal(1, node.Children.ElementAt(0).Children.Count);
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
    public void NoDotInName()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $"""
                                 <?xml version="1.0" encoding="utf-8"?>
                                 <noDotInNameTest xmlns="{ns}">
                                     <invalid.element margin="4px"/>
                                 </noDotInNameTest>
                                 """;
        var templateReader = new XmlTemplateReader();
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        Assert.Throws<XmlNodeNameException>(() => templateReader.Read(xmlReader));
    }

    [Fact]
    public void StyleMustBeEmptyTag()
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
        var templateReader = new XmlTemplateReader();
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        Assert.Throws<XmlStyleInformationCannotNestException>(() => templateReader.Read(xmlReader));
    }
}