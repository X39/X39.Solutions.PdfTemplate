using System.Text;
using System.Xml;
using X39.Solutions.PdfTemplate.Transformers;
using X39.Solutions.PdfTemplate.Xml;

namespace X39.Solutions.PdfTemplate.Test.ExpressionTests;

public class ForEachTransformerTests
{
    [Fact]
    public void ForEachLoopWithVariableSourceAndIndex()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @foreach i in list with index {
                                         <text>@index -- @i</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var data = new TemplateData();
        data.SetVariable("list", new[] {1, 2, 3});
        var templateReader = new XmlTemplateReader(data, new[] {new ForEachTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = templateReader.Read(xmlReader);
        Assert.Equal(3, nodeInformation.Children.Count);
        Assert.Equal("0 -- 1", nodeInformation.Children.ElementAt(0).TextContent);
        Assert.Equal("1 -- 2", nodeInformation.Children.ElementAt(1).TextContent);
        Assert.Equal("2 -- 3", nodeInformation.Children.ElementAt(2).TextContent);
    }
    
    [Fact]
    public void ForEachLoopWithVariableSource()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @foreach i in list {
                                         <text>@i</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var data = new TemplateData();
        data.SetVariable("list", new[] {1, 2, 3});
        var templateReader = new XmlTemplateReader(data, new[] {new ForEachTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = templateReader.Read(xmlReader);
        Assert.Equal(3, nodeInformation.Children.Count);
        Assert.Equal("1", nodeInformation.Children.ElementAt(0).TextContent);
        Assert.Equal("2", nodeInformation.Children.ElementAt(1).TextContent);
        Assert.Equal("3", nodeInformation.Children.ElementAt(2).TextContent);
    }

    [Fact]
    public void ForEachLoopWithFunctionSourceAndIndex()
    {
        const string ns = Constants.ControlsNamespace;
        const string template = $$"""
                                  <?xml version="1.0" encoding="utf-8"?>
                                  <styleMustBeEmptyTagTest xmlns="{{ns}}" someAttribute="asd">
                                     @foreach i in list(1, "") with index {
                                         <text>@index: @i</text>
                                     }
                                  </styleMustBeEmptyTagTest>
                                  """;
        var data = new TemplateData();
        data.RegisterFunction(new DummyValueCollectionFunction("list", Enumerable.Range(1, 10).Cast<object>().ToList(), new[] {typeof(int), typeof(string)}));
        var templateReader = new XmlTemplateReader(data, new[] {new ForEachTransformer()});
        using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(template));
        using var xmlReader = XmlReader.Create(xmlStream);
        var nodeInformation = templateReader.Read(xmlReader);
        Assert.Equal(10, nodeInformation.Children.Count);
        Assert.Equal("0: 1", nodeInformation.Children.ElementAt(0).TextContent);
        Assert.Equal("1: 2", nodeInformation.Children.ElementAt(1).TextContent);
        Assert.Equal("2: 3", nodeInformation.Children.ElementAt(2).TextContent);
        Assert.Equal("3: 4", nodeInformation.Children.ElementAt(3).TextContent);
        Assert.Equal("4: 5", nodeInformation.Children.ElementAt(4).TextContent);
        Assert.Equal("5: 6", nodeInformation.Children.ElementAt(5).TextContent);
        Assert.Equal("6: 7", nodeInformation.Children.ElementAt(6).TextContent);
        Assert.Equal("7: 8", nodeInformation.Children.ElementAt(7).TextContent);
        Assert.Equal("8: 9", nodeInformation.Children.ElementAt(8).TextContent);
        Assert.Equal("9: 10", nodeInformation.Children.ElementAt(9).TextContent);
    }
}